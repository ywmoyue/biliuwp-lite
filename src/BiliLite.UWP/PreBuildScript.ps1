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
        Expand-Archive -Path $tempZipPath -DestinationPath $assetsPath -Force
        
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
