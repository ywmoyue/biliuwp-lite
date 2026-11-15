<#
.SYNOPSIS
检查并更新ShakaPlayer版本

.DESCRIPTION
1. 检查Assets/ShakaPlayer下是否有version.json文件
2. 如果没有，则从GitHub release下载最新版本
3. 如果有，则检查是否是最新版本
#>

# 设置当前脚本所在目录为工作目录
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
Set-Location -Path $scriptDir

# 配置参数
$repoOwner = "ywmoyue"
$repoName = "BiliLite-WebPlayer"
$assetsPath = Join-Path -Path $scriptDir -ChildPath "Assets/ShakaPlayer"
$versionFile = Join-Path -Path $assetsPath -ChildPath "version.json"
$tempZipPath = Join-Path -Path $env:TEMP -ChildPath "WebPlayer-latest.zip"

# 创建目录（如果不存在）
if (-not (Test-Path -Path $assetsPath)) {
    New-Item -ItemType Directory -Path $assetsPath -Force | Out-Null
}

# 获取GitHub最新release信息
try {
    $releaseInfo = Invoke-RestMethod -Uri "https://api.github.com/repos/$repoOwner/$repoName/releases/latest" -Headers @{
        "Accept" = "application/vnd.github.v3+json"
    }
    
    $latestTag = $releaseInfo.tag_name
    $latestCommit = $releaseInfo.target_commitish
    $asset = $releaseInfo.assets | Where-Object { $_.name -like "WebPlayer-*.zip" } | Select-Object -First 1
    
    if (-not $asset) {
        throw "未找到WebPlayer-*.zip文件"
    }
    
    $downloadUrl = $asset.browser_download_url
}
catch {
    Write-Error "获取GitHub release信息失败: $_"
    # 检查是否是API限制且本地版本文件存在
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode -eq 403 -and 
        $_.Exception.Message -like "*API rate limit*" -and 
        (Test-Path -Path $versionFile)) {
        Write-Warning "GitHub API限制，但本地版本文件存在，跳过更新"
        exit 0
    }
    exit 1
}

# 检查本地版本文件
$needUpdate = $true

if (Test-Path -Path $versionFile) {
    try {
        $localVersion = Get-Content -Path $versionFile | ConvertFrom-Json
        
        if ($localVersion.tag -eq $latestTag -and $localVersion.commit -eq $latestCommit) {
            Write-Host "当前已是最新版本 (Tag: $latestTag, Commit: $latestCommit)"
            $needUpdate = $false
        }
        else {
            Write-Host "发现新版本:"
            Write-Host "本地版本 - Tag: $($localVersion.tag), Commit: $($localVersion.commit)"
            Write-Host "最新版本 - Tag: $latestTag, Commit: $latestCommit"
        }
    }
    catch {
        Write-Warning "读取本地version.json失败: $_"
        $needUpdate = $true
    }
}
else {
    Write-Host "未找到version.json文件，将下载最新版本"
}

# 如果需要更新
if ($needUpdate) {
    try {
        Write-Host "正在下载最新版本..."
        Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZipPath
        
        # 清空目标目录（保留目录本身）
        Get-ChildItem -Path $assetsPath -Exclude version.json | Remove-Item -Force -Recurse
        
        Write-Host "解压文件..."
        # 不使用 Expand-Archive：它依赖 Microsoft.PowerShell.Archive 模块，而该模块在部分环境下
        # （例如 PowerShell 7.6.10 自带的版本被 Windows PowerShell 5.1 加载时）会调用 .NET Framework
        # 并不存在的 ExtractToDirectory 三参重载，导致构建失败。
        # 这里直接用 .NET Framework 自带的 ZipFile API 逐条解压，Windows PowerShell 5.1 与 PowerShell 7 都可用。
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($tempZipPath)
        try {
            foreach ($entry in $zip.Entries) {
                # 目录条目（Name 为空）跳过，解压文件时会按需创建目录
                if ([string]::IsNullOrEmpty($entry.Name)) {
                    continue
                }

                $relativePath = $entry.FullName -replace '/', '\'
                $targetPath = Join-Path -Path $assetsPath -ChildPath $relativePath
                $targetDir = Split-Path -Path $targetPath -Parent
                if (-not (Test-Path -Path $targetDir)) {
                    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
                }

                # 第三个参数为 true，表示覆盖已存在的文件
                [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $targetPath, $true)
            }
        }
        finally {
            $zip.Dispose()
        }
        
        # 创建/更新version.json
        @{
            commit = $latestCommit
            tag    = $latestTag
        } | ConvertTo-Json | Out-File -FilePath $versionFile -Force
        
        Write-Host "更新成功! 版本: $latestTag"
    }
    catch {
        Write-Error "下载或解压失败: $_"
        exit 1
    }
    finally {
        # 清理临时文件
        if (Test-Path -Path $tempZipPath) {
            Remove-Item -Path $tempZipPath -Force -ErrorAction SilentlyContinue
        }
    }
}

exit 0
