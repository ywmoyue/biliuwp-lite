import os
import requests
import shutil
import subprocess

# python choco_pack.py --version 4.7.13.1403 --githubtoken YOUR_GITHUB_TOKEN --chocotoken YOUR_CHOCO_TOKEN

def replace_string_in_file(file_path, target_string, replacement_string):
    try:
        # 根据文件扩展名选择编码
        if file_path.endswith('.ps1'):
            encoding = 'gbk'  # 对于 .ps1 文件，使用 GBK 编码
        else:
            encoding = 'utf-8'  # 对于其他文件，使用 UTF-8 编码
        
        # 读取文件内容
        with open(file_path, 'r', encoding=encoding) as file:
            file_contents = file.read()
        
        # 替换文件内容中的目标字符串
        updated_contents = file_contents.replace(target_string, replacement_string)
        
        # 将更新后的内容写回文件
        with open(file_path, 'w', encoding=encoding) as file:
            file.write(updated_contents)
        
        print(f"Successfully replaced '{target_string}' with '{replacement_string}' in {file_path}")
    except FileNotFoundError:
        print(f"The file at {file_path} was not found.")
    except IOError as e:
        print(f"An IOError occurred: {e.strerror}.")
    except UnicodeDecodeError:
        print(f"Failed to decode the file {file_path} with {encoding} encoding.")
    except UnicodeEncodeError:
        print(f"Failed to encode the file {file_path} with {encoding} encoding.")

def download_github_release_asset(tag, github_token, arch, output_dir):
    # 获取所有 releases
    url = "https://api.github.com/repos/ywmoyue/biliuwp-lite/releases"
    headers = {
        "Authorization": f"token {github_token}",
        "Accept": "application/vnd.github.v3+json"
    }
    
    response = requests.get(url, headers=headers)
    if response.status_code != 200:
        print(f"Failed to fetch releases. Status code: {response.status_code}")
        return False
    
    releases = response.json()
    
    # 根据 tag 查找对应的 release
    normalized_tag = tag[:-2] if tag.endswith('.0') else tag
    release_item = None
    for release in releases:
        if release["tag_name"] == normalized_tag:
            release_item = release
            break
    
    if not release_item:
        print(f"Release with tag {normalized_tag} not found.")
        return False
    
    # 查找匹配的 asset
    zip_filename = f"BiliLite.Packages_{tag[1:]}_{arch}.zip"
    asset_url = None
    
    for asset in release_item["assets"]:
        if asset["name"] == zip_filename:
            asset_url = asset["url"]
            break
    
    if not asset_url:
        print(f"Asset {zip_filename} not found in release {tag}.")
        return False
    
    # 下载 asset
    headers["Accept"] = "application/octet-stream"
    response = requests.get(asset_url, headers=headers, stream=True)
    if response.status_code != 200:
        print(f"Failed to download asset {zip_filename}. Status code: {response.status_code}")
        return False
    
    os.makedirs(output_dir, exist_ok=True)
    zip_path = os.path.join(output_dir, zip_filename)
    with open(zip_path, 'wb') as f:
        for chunk in response.iter_content(chunk_size=8192):
            f.write(chunk)
    
    print(f"Downloaded {zip_filename} to {output_dir}")
    return True

def process_architecture(arch, version, github_token, chocotoken):
    # 定义路径
    template_dir = "scripts/chocolaty-templates"
    pack_parent_dir  = "C:/choco/pack"
    pack_dir = f"{pack_parent_dir}/{arch}"
    nuspec_file = f"{pack_dir}/BiliLite-uwp.nuspec"
    install_script = f"{pack_dir}/tools/subInstall.ps1"
    resources_dir = f"{pack_dir}/resources"
    
    # 复制模板文件夹到目标目录
    if not os.path.exists(template_dir):
        print(f"Template directory {template_dir} not found.")
        return
    
    os.makedirs(pack_parent_dir, exist_ok=True)
    shutil.copytree(template_dir, pack_dir, dirs_exist_ok=True)

    # 替换 nuspec 文件中的 {version} 和 {arch}
    replace_string_in_file(nuspec_file, "{version}", version)
    replace_string_in_file(nuspec_file, "{arch}", arch)
    
    # 替换 chocolateyInstall.ps1 文件中的 {version}
    replace_string_in_file(install_script, "{version}", version)
    
    # 创建 resources 文件夹
    os.makedirs(resources_dir, exist_ok=True)
    
    # 下载 GitHub Release 的 zip 包
    tag = f"v{version}"
    if not download_github_release_asset(tag, github_token, arch, resources_dir):
        print(f"Skipping architecture {arch} due to missing asset.")
        return
    
    # 在目标目录下执行 choco pack 命令
    try:
        subprocess.run(["choco", "pack"], cwd=pack_dir, check=True)
    except subprocess.CalledProcessError as e:
        print(f"Failed to pack for architecture {arch}. Error: {e}")
        return
    
    # 拼接包名
    package_name = f"BiliLite-uwp-{arch}.{version}-beta.nupkg"
    package_path = os.path.join(pack_dir, package_name)
    
    # 执行 choco push 命令
    try:
        subprocess.run(["choco", "push", "--source", "https://push.chocolatey.org/", "--api-key", chocotoken], cwd=pack_dir, check=True)
    except subprocess.CalledProcessError as e:
        print(f"Failed to push package for architecture {arch}. Error: {e}")

def main(version, github_token, chocotoken):
    architectures = ["x64", "x86", "ARM64"]
    
    for arch in architectures:
        print(f"Processing architecture: {arch}")
        process_architecture(arch, version, github_token, chocotoken)

if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description="Process Chocolatey packages for different architectures.")
    parser.add_argument("--version", required=True, help="The version of the package.")
    parser.add_argument("--githubtoken", required=True, help="GitHub token for accessing the release assets.")
    parser.add_argument("--chocotoken", required=True, help="Chocolatey API key for pushing the package.")
    
    args = parser.parse_args()
    
    main(args.version, args.githubtoken, args.chocotoken)