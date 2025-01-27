# Due to Powershell limitations, this script can only be opened with GBK encoding
$AppName = "BiliLite.Packages"
$Version = "{version}"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Output "scriptDir: $scriptDir"

# 获取系统架构
$arch = $env:PROCESSOR_ARCHITECTURE
# 判断架构并赋值
if ($arch -eq "AMD64") {
    $systemArch = "x64"
} elseif ($arch -eq "x86") {
    $systemArch = "x86"
} elseif ($arch -eq "ARM64") {
    $systemArch = "ARM64"
} else {
    $systemArch = "Unknown"
}

$Package = "{0}_{1}_{2}" -f $AppName, $Version, $systemArch
Write-Output "Package: $Package"

# 找到对应的安装包并解压到unpack路径
$PackageZipRelativePath = "..\resources\{0}.zip" -f $Package
$PackageZipPath = Join-Path $scriptDir $PackageZipRelativePath
$UnpackPath = Join-Path $scriptDir "..\unpack"

# 检查文件是否存在并解压
if (Test-Path $PackageZipPath) {
    # 确保解压路径存在
    if (-not (Test-Path $UnpackPath)) {
        New-Item -ItemType Directory -Path $UnpackPath | Out-Null
    }

    # 解压文件
    Expand-Archive -Path $PackageZipPath -DestinationPath $UnpackPath -Force
    Write-Host "文件已解压到 $UnpackPath"
} else {
    Write-Host "文件 $PackageZipPath 不存在，即将从 GitHub 下载当前平台适配的安装包"

    # 从 GitHub 下载软件包并解压
    $downloadUrl = "https://github.com/ywmoyue/biliuwp-lite/releases/download/v{0}/{1}.zip" -f $Version, $Package
    Write-Host "下载地址: $downloadUrl"

    try {
        # 下载文件
        Invoke-WebRequest -Uri $downloadUrl -OutFile $PackageZipPath
        Write-Host "文件下载完成: $PackageZipPath"

        # 确保解压路径存在
        if (-not (Test-Path $UnpackPath)) {
            New-Item -ItemType Directory -Path $UnpackPath | Out-Null
        }

        # 解压文件
        Expand-Archive -Path $PackageZipPath -DestinationPath $UnpackPath -Force
        Write-Host "文件已解压到 $UnpackPath"
    } catch {
        Write-Host "下载或解压失败: $_"
        exit 1
    }
}

# 设置开发者模式允许安装未知来源包
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name "AllowDevelopmentWithoutDevLicense" -Value 1
Write-Host "已允许安装未知来源包"

# 证书路径
$certRelativePath = "..\unpack\{0}_Test\{0}.cer" -f $Package
$certPath = Resolve-Path (Join-Path $scriptDir $certRelativePath)

# 安装脚本路径
$installScriptPath = "$scriptDir\..\unpack\{0}_Test\install.ps1" -f $Package
Write-Host "安装路径 $installScriptPath"

# 安装证书
try {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cert.Import($certPath.Path)

    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    $store.Open("ReadWrite")
    $store.Add($cert)
    $store.Close()
    Write-Host "证书安装完成"
} catch {
    Write-Host "证书安装失败: $_"
    exit 1
}

# 执行安装脚本
try {
    . "$installScriptPath"
    Write-Host "安装脚本执行成功"
} catch {
    Write-Host "安装脚本执行失败: $_"
    exit 1
}