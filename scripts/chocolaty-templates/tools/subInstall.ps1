# Due to Powershell limitations, this script can only be opened with GBK encoding

# 在 $TempPath 目录下新建文件 error
function Create-ErrorFile {
    $errorFilePath = Join-Path $TempPath "error"
    if (-not (Test-Path $errorFilePath)) {
        New-Item -ItemType File -Path $errorFilePath | Out-Null
    }
    Write-Host "错误文件已创建: $errorFilePath"
}

# 在 $TempPath 目录下新建文件 success
function Create-SuccessFile {
    $successFilePath = Join-Path $TempPath "success"
    if (-not (Test-Path $successFilePath)) {
        New-Item -ItemType File -Path $successFilePath | Out-Null
    }
    Write-Host "成功文件已创建: $successFilePath"
}

# 检查管理员权限
If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
   Write-Warning "当前脚本需要以管理员权限运行，请以管理员身份运行 PowerShell 并重新执行此脚本。"
   $arguments = "& '" +$myinvocation.mycommand.definition + "'"
   Start-Process powershell -Verb runAs -ArgumentList $arguments
   Break
}

$AppName = "BiliLite.Packages"
$Version = "{version}"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Output "scriptDir: $scriptDir"

$TempPath = Join-Path $scriptDir "..\temp"
if (-not (Test-Path $TempPath)) {
    New-Item -ItemType Directory -Path $TempPath | Out-Null
}

try{

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
            Create-ErrorFile
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
    $installPackagePath = "$scriptDir\..\unpack\{0}_Test" -f $Package
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
        Create-ErrorFile
        exit 1
    }

    # 执行安装脚本
    try {
        . "$installScriptPath" -Force
        Write-Host "安装脚本执行成功"
    } catch {
        Write-Host "安装脚本执行失败: $_"
        Create-ErrorFile
        exit 1
    }

    $PackageId = "5422.502643927C6AD"

    $PackageNamePrefix = "{0}_{1}" -f $PackageId, $Version
    Write-Host "PackageNamePrefix: $PackageNamePrefix"

    # 检测是否安装成功
    $PackageFullName = (Get-AppxPackage | Where-Object { $_.PackageFullName -like "$PackageNamePrefix*" }).PackageFullName
    if (-not $PackageFullName) {
        Write-Host "应用安装后验证失败，请检查安装日志"
        Create-ErrorFile
        exit 1
    } else {
        Write-Host "应用已安装"
        Create-SuccessFile
    }
} catch {
    Write-Host "发生错误："
    Write-Host $_.Exception.Message
    #Read-Host -Prompt "按任意键继续..."
}