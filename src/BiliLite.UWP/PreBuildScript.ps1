<#
.SYNOPSIS
��鲢����ShakaPlayer�汾

.DESCRIPTION
1. ���Assets/ShakaPlayer���Ƿ���version.json�ļ�
2. ���û�У����GitHub release�������°汾
3. ����У������Ƿ������°汾
#>

# ���õ�ǰ�ű�����Ŀ¼Ϊ����Ŀ¼
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
Set-Location -Path $scriptDir

# ���ò���
$repoOwner = "ywmoyue"
$repoName = "BiliLite-WebPlayer"
$assetsPath = Join-Path -Path $scriptDir -ChildPath "Assets/ShakaPlayer"
$versionFile = Join-Path -Path $assetsPath -ChildPath "version.json"
$tempZipPath = Join-Path -Path $env:TEMP -ChildPath "WebPlayer-latest.zip"

# ����Ŀ¼����������ڣ�
if (-not (Test-Path -Path $assetsPath)) {
    New-Item -ItemType Directory -Path $assetsPath -Force | Out-Null
}

# ��ȡGitHub����release��Ϣ
try {
    $releaseInfo = Invoke-RestMethod -Uri "https://api.github.com/repos/$repoOwner/$repoName/releases/latest" -Headers @{
        "Accept" = "application/vnd.github.v3+json"
    }
    
    $latestTag = $releaseInfo.tag_name
    $latestCommit = $releaseInfo.target_commitish
    $asset = $releaseInfo.assets | Where-Object { $_.name -like "WebPlayer-*.zip" } | Select-Object -First 1
    
    if (-not $asset) {
        throw "δ�ҵ�WebPlayer-*.zip�ļ�"
    }
    
    $downloadUrl = $asset.browser_download_url
}
catch {
    Write-Error "��ȡGitHub release��Ϣʧ��: $_"
    exit 1
}

# ��鱾�ذ汾�ļ�
$needUpdate = $true

if (Test-Path -Path $versionFile) {
    try {
        $localVersion = Get-Content -Path $versionFile | ConvertFrom-Json
        
        if ($localVersion.tag -eq $latestTag -and $localVersion.commit -eq $latestCommit) {
            Write-Host "��ǰ�������°汾 (Tag: $latestTag, Commit: $latestCommit)"
            $needUpdate = $false
        }
        else {
            Write-Host "�����°汾:"
            Write-Host "���ذ汾 - Tag: $($localVersion.tag), Commit: $($localVersion.commit)"
            Write-Host "���°汾 - Tag: $latestTag, Commit: $latestCommit"
        }
    }
    catch {
        Write-Warning "��ȡ����version.jsonʧ��: $_"
        $needUpdate = $true
    }
}
else {
    Write-Host "δ�ҵ�version.json�ļ������������°汾"
}

# �����Ҫ����
if ($needUpdate) {
    try {
        Write-Host "�����������°汾..."
        Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZipPath
        
        # ���Ŀ��Ŀ¼������Ŀ¼����
        Get-ChildItem -Path $assetsPath -Exclude version.json | Remove-Item -Force -Recurse
        
        Write-Host "��ѹ�ļ�..."
        Expand-Archive -Path $tempZipPath -DestinationPath $assetsPath -Force
        
        # ����/����version.json
        @{
            commit = $latestCommit
            tag    = $latestTag
        } | ConvertTo-Json | Out-File -FilePath $versionFile -Force
        
        Write-Host "���³ɹ�! �汾: $latestTag"
    }
    catch {
        Write-Error "���ػ��ѹʧ��: $_"
        exit 1
    }
    finally {
        # ������ʱ�ļ�
        if (Test-Path -Path $tempZipPath) {
            Remove-Item -Path $tempZipPath -Force -ErrorAction SilentlyContinue
        }
    }
}

exit 0
