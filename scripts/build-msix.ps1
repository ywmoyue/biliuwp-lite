<#
.SYNOPSIS
    BiliLite（WinUI3）编译 + 打包 MSIX。

.DESCRIPTION
    流程：清理 bin/obj → 解析 MSBuild → （可选）临时改版本号 → 编译并生成 .msix → 校验产物 → 还原清单。

    产物校验会拦截混入 app-local 运行时文件（hostfxr.dll / coreclr.dll 等）的包：
    这类包在任何机器上都无法启动，命中时脚本会删除 .msix 并非 0 退出。

.PARAMETER Platform
    x64 / x86 / ARM64，默认 x64。包内 apphost 的架构就是它，目标机必须装同架构运行时。

.PARAMETER Configuration
    Release / Debug，默认 Release。

.PARAMETER Version
    四段版本号，例如 4.8.999.19010。不传则沿用 Package.appxmanifest 里的版本（仓库里是 1.0.0.0）。
    同一 PackageFamilyName 的包必须版本递增，否则 Windows 拒绝覆盖安装（0x80073D06）。

.PARAMETER CertificatePath
    签名用 .pfx。不传时若给了 -CertificatePassword 则用项目自带的 BiliLite.UWP_TemporaryKey.pfx，
    否则用 csproj 里 PackageCertificateThumbprint 对应的证书（从证书存储里取）。

.PARAMETER CertificatePassword
    .pfx 密码。

.PARAMETER NoSign
    不签名。产物无法用 Add-AppxPackage 安装，仅用于检查包内容。

.PARAMETER NoClean
    跳过清理 bin/obj。

.PARAMETER MsBuildPath
    手动指定 MSBuild.exe；默认用 vswhere 找 Visual Studio 自带的 MSBuild。

.EXAMPLE
    pwsh .\scripts\build-msix.ps1 -Platform x64 -Version 4.8.999.19010

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\scripts\build-msix.ps1 -Platform x86 -Version 4.8.999.19010 -NoSign

.EXAMPLE
    .\scripts\build-msix.ps1 -Platform x64 -Version 4.8.999.19010 `
        -CertificatePath .\src\BiliLite.UWP\BiliLite.UWP_TemporaryKey.pfx -CertificatePassword 'pwd'
#>
[CmdletBinding()]
param(
    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64',

    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [string]$Version,

    [string]$CertificatePath,

    [string]$CertificatePassword,

    [switch]$NoSign,

    [switch]$NoClean,

    [string]$MsBuildPath
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------- 基础信息

$repoRoot     = Split-Path -Parent $PSScriptRoot
$projectDir   = Join-Path $repoRoot 'src\BiliLite.UWP'
$projectFile  = Join-Path $projectDir 'BiliLite.UWP.csproj'
$manifestFile = Join-Path $projectDir 'Package.appxmanifest'
$appPackages  = Join-Path $projectDir 'AppPackages'

$tfm      = 'net8.0-windows10.0.19041.0'
$ridMap   = @{ 'x64' = 'win-x64'; 'x86' = 'win-x86'; 'ARM64' = 'win-arm64' }
$rid      = $ridMap[$Platform]
$archLower = $Platform.ToLower()
$confSuffix = if ($Configuration -eq 'Debug') { '_Debug' } else { '' }

$outputDir = Join-Path $projectDir ("bin\{0}\{1}\{2}\{3}" -f $Platform, $Configuration, $tfm, $rid)

# 出现在应用目录就会让 .NET host 按自带运行时解析，启动即报缺少 .NET
$runtimeFileNames = @(
    'hostfxr.dll',
    'hostpolicy.dll',
    'coreclr.dll',
    'clrjit.dll',
    'mscordaccore.dll',
    'System.Private.CoreLib.dll'
)

function Write-Step([string]$Text) { Write-Host "==> $Text" -ForegroundColor Cyan }
function Write-Ok([string]$Text)   { Write-Host "    $Text" -ForegroundColor Green }

# ---------------------------------------------------------------- 参数校验

foreach ($required in @($projectFile, $manifestFile)) {
    if (-not (Test-Path -LiteralPath $required)) { throw "找不到文件：$required" }
}

$manifestOriginalBytes = [IO.File]::ReadAllBytes($manifestFile)
[xml]$manifestXml = Get-Content -LiteralPath $manifestFile -Raw
$currentVersion = [string]$manifestXml.Package.Identity.Version

$targetVersion = $currentVersion
if ($Version) {
    if ($Version -notmatch '^\d{1,5}(\.\d{1,5}){3}$') {
        throw "版本号格式不对：'$Version'，应为四段数字，例如 4.8.999.19010"
    }
    $targetVersion = $Version
}

$versionParts = $targetVersion.Split('.')
if ([int]$versionParts[3] -ne 0) {
    Write-Warning "版本号第四段通常必须为 0（当前 $targetVersion），否则可能签名或安装失败。"
}

if (-not $Version -and $targetVersion -eq '1.0.0.0') {
    Write-Warning "仍在使用仓库里的默认版本 1.0.0.0。发给已装过新版本的用户会装不上，建议用 -Version 指定递增版本号。"
}

# ---------------------------------------------------------------- 签名参数

$signingArgs = @()
if ($NoSign) {
    $signingArgs += '/p:AppxPackageSigningEnabled=false'
    Write-Warning '已选择不签名：产物无法用 Add-AppxPackage 安装，只能检查包内容。'
} else {
    $pfxPath = $null
    if ($CertificatePath) {
        if (-not (Test-Path -LiteralPath $CertificatePath)) { throw "找不到签名证书：$CertificatePath" }
        $pfxPath = (Resolve-Path -LiteralPath $CertificatePath).Path
    } elseif ($CertificatePassword) {
        $candidate = Join-Path $projectDir 'BiliLite.UWP_TemporaryKey.pfx'
        if (-not (Test-Path -LiteralPath $candidate)) { throw "找不到 $candidate，请用 -CertificatePath 指定 pfx。" }
        $pfxPath = (Resolve-Path -LiteralPath $candidate).Path
    }

    if ($pfxPath) {
        if (-not $CertificatePassword) { throw '指定了 .pfx 但没给 -CertificatePassword。' }
        $signingArgs += "/p:PackageCertificateKeyFile=$pfxPath"
        $signingArgs += "/p:PackageCertificatePassword=$CertificatePassword"
        Write-Ok "签名证书文件：$pfxPath"
    } else {
        $match = Select-String -LiteralPath $projectFile -Pattern '<PackageCertificateThumbprint>([0-9A-Fa-f]+)</PackageCertificateThumbprint>' |
            Select-Object -First 1
        if (-not $match) { throw 'csproj 里没有 PackageCertificateThumbprint，且没有指定 -CertificatePath。' }

        $thumbprint = $match.Matches[0].Groups[1].Value
        $cert = @(
            Get-ChildItem -LiteralPath "Cert:\CurrentUser\My\$thumbprint" -ErrorAction SilentlyContinue
            Get-ChildItem -LiteralPath "Cert:\LocalMachine\My\$thumbprint" -ErrorAction SilentlyContinue
        )
        if ($cert.Count -eq 0) {
            throw "证书存储里找不到指纹 $thumbprint 的证书。请用 -CertificatePath/-CertificatePassword 指定 pfx，或用 -NoSign。"
        }
        Write-Ok "使用证书存储里的证书签名（指纹 $thumbprint）。"
    }
}

# ---------------------------------------------------------------- 清理旧的编译产物

if (-not $NoClean) {
    $projectRootFull = [IO.Path]::GetFullPath($projectDir)
    foreach ($dir in @((Join-Path $projectDir 'bin'), (Join-Path $projectDir 'obj'))) {
        $full = [IO.Path]::GetFullPath($dir)
        if (-not $full.StartsWith($projectRootFull, [StringComparison]::OrdinalIgnoreCase)) {
            throw "拒绝删除项目目录以外的路径：$full"
        }
        if (Test-Path -LiteralPath $full) {
            Remove-Item -LiteralPath $full -Recurse -Force
            Write-Ok "已删除 $full"
        }
    }
}

# ---------------------------------------------------------------- 解析 MSBuild

if ($MsBuildPath) {
    if (-not (Test-Path -LiteralPath $MsBuildPath)) { throw "指定的 MSBuild 路径不存在：$MsBuildPath" }
    $msbuild = (Resolve-Path -LiteralPath $MsBuildPath).Path
} else {
    $msbuild = $null
    $programFilesX86 = ${env:ProgramFiles(x86)}
    if ($programFilesX86) {
        $vswhere = Join-Path $programFilesX86 'Microsoft Visual Studio\Installer\vswhere.exe'
        if (Test-Path -LiteralPath $vswhere) {
            try {
                $found = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'
                if ($found) { $msbuild = ($found | Select-Object -First 1) }
            } catch {
                # vswhere 调用失败时继续走下面的兜底查找
                Write-Warning "vswhere 调用失败：$($_.Exception.Message)"
            }
        }
    }
    if (-not $msbuild) {
        $cmd = Get-Command 'msbuild.exe' -ErrorAction SilentlyContinue
        if ($cmd) { $msbuild = $cmd.Source }
    }
    if (-not $msbuild) {
        throw '找不到 MSBuild.exe。请安装 Visual Studio（含 MSIX 打包工具），或用 -MsBuildPath 指定路径。'
    }
}
Write-Ok "MSBuild：$msbuild"

# ---------------------------------------------------------------- 编译 + 打包

$logFile = Join-Path $env:TEMP ("bililite-msbuild-{0}-{1}.log" -f $Platform, (Get-Date -Format 'yyyyMMdd-HHmmss'))

$buildArgs = @(
    $projectFile
    '/restore'
    '/m'
    '/nologo'
    '/v:m'
    "/p:Platform=$Platform"
    "/p:Configuration=$Configuration"
    '/p:GenerateAppxPackageOnBuild=true'
    '/p:AppxBundle=Never'
    '/p:AppxSymbolPackageEnabled=false'
    '/p:BuildMode=SideloadOnly'
    "/flp:LogFile=$logFile;Verbosity=normal"
) + $signingArgs

Write-Step "编译打包：$Platform / $Configuration / 版本 $targetVersion"

$manifestPatched = $false
try {
    if ($targetVersion -ne $currentVersion) {
        Write-Ok "临时把 Package.appxmanifest 版本改为 $targetVersion（结束后还原）"
        $manifestXml.Package.Identity.Version = $targetVersion
        $manifestXml.Save($manifestFile)
        $manifestPatched = $true
    }

    & $msbuild @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild 失败（exit $LASTEXITCODE）。日志：$logFile"
    }
} finally {
    # 无论成功失败都还原清单，避免把版本号改动留在工作区
    [IO.File]::WriteAllBytes($manifestFile, $manifestOriginalBytes)
    if ($manifestPatched) { Write-Ok 'Package.appxmanifest 已还原。' }
}

# ---------------------------------------------------------------- 定位产物

$expectedDir = Join-Path $appPackages ("BiliLite.UWP_{0}_{1}{2}_Test" -f $targetVersion, $archLower, $confSuffix)

$msix = Get-ChildItem -LiteralPath $expectedDir -Filter '*.msix' -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $msix) {
    $msix = Get-ChildItem -LiteralPath $appPackages -Recurse -Filter '*.msix' -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "*_${archLower}.msix" } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
}
if (-not $msix) { throw "没有生成 .msix。日志：$logFile" }

# ---------------------------------------------------------------- 校验产物

Write-Step '校验产物'

$problems = New-Object System.Collections.Generic.List[string]

if (-not (Test-Path -LiteralPath $outputDir)) {
    $problems.Add("找不到编译输出目录：$outputDir")
} else {
    $leaked = @(Get-ChildItem -LiteralPath $outputDir -File -ErrorAction SilentlyContinue |
        Where-Object { $runtimeFileNames -contains $_.Name })
    if ($leaked.Count -gt 0) {
        $problems.Add(("编译输出目录里混入了 app-local 运行时文件：{0}。多为 self-contained 构建的残留，增量构建不会删除。清理 bin/obj 后重新打包。" -f (($leaked | ForEach-Object { $_.Name }) -join ', ')))
    }
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = $null
try {
    $zip = [IO.Compression.ZipFile]::OpenRead($msix.FullName)

    $badEntries = @($zip.Entries | Where-Object { $runtimeFileNames -contains [IO.Path]::GetFileName($_.FullName) })
    if ($badEntries.Count -gt 0) {
        $problems.Add(("MSIX 里包含 app-local 运行时文件：{0}。这会让 .NET host 把包目录当成 .NET 根，启动时报缺少 .NET 运行时。" -f (($badEntries | ForEach-Object { $_.FullName }) -join ', ')))
    }

    $rcEntry = $zip.GetEntry('BiliLite.runtimeconfig.json')
    if (-not $rcEntry) {
        $problems.Add('MSIX 里没有 BiliLite.runtimeconfig.json，无法确定框架依赖。')
    } else {
        $reader = New-Object IO.StreamReader($rcEntry.Open())
        try { $rcText = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $rc = $rcText | ConvertFrom-Json

        if (-not $rc.runtimeOptions.framework) {
            $problems.Add("BiliLite.runtimeconfig.json 里没有 framework 节点（当前内容：$($rcText.Trim())）。")
        }
        if ($rc.runtimeOptions.includedFrameworks) {
            $problems.Add('BiliLite.runtimeconfig.json 里出现了 includedFrameworks：这是 self-contained 布局，和 framework-dependent 的依赖声明冲突。')
        }
    }

    $manifestEntry = $zip.GetEntry('AppxManifest.xml')
    if (-not $manifestEntry) {
        $problems.Add('MSIX 里没有 AppxManifest.xml？')
    } else {
        $reader = New-Object IO.StreamReader($manifestEntry.Open())
        try { $packagedManifestText = $reader.ReadToEnd() } finally { $reader.Dispose() }
        [xml]$packagedManifest = $packagedManifestText
        $identity = $packagedManifest.Package.Identity

        if ([string]$identity.Version -ne $targetVersion) {
            $problems.Add("包内版本是 $($identity.Version)，期望 $targetVersion。")
        }
        if ([string]$identity.ProcessorArchitecture -ne $Platform) {
            $problems.Add("包内架构是 $($identity.ProcessorArchitecture)，期望 $Platform。")
        }
    }
} finally {
    if ($zip) { $zip.Dispose() }
}

if ($problems.Count -gt 0) {
    Remove-Item -LiteralPath $msix.FullName -Force
    throw ("产物校验不通过，已删除 {0}`n  - {1}" -f $msix.FullName, ($problems -join "`n  - "))
}

# ---------------------------------------------------------------- 输出摘要

$hash = (Get-FileHash -LiteralPath $msix.FullName -Algorithm SHA256).Hash
$sizeMB = [math]::Round($msix.Length / 1MB, 1)

Write-Step '打包完成'
Write-Host ("    MSIX      : " + $msix.FullName)
Write-Host ("    大小      : {0} MB" -f $sizeMB)
Write-Host ("    SHA256    : " + $hash)
Write-Host ("    Identity  : {0} / {1} / {2}" -f $identity.Name, $identity.Version, $identity.ProcessorArchitecture)
Write-Host  "    依赖      : Microsoft.WindowsAppRuntime.1.8（目标机缺的话先装同目录 Dependencies\*_ARCH_\Microsoft.WindowsAppRuntime.1.8.msix）"
Write-Host ("    目标机需要: Microsoft.NETCore.App 8.0.x（$archLower）")
Write-Host  "    安装      : Add-AppxPackage -Path '<上面的 MSIX 路径>'"
Write-Host ("    构建日志  : " + $logFile)
Write-Host ''
Write-Host '    提示：包内不自带 .NET，目标机必须预装同架构的 .NET 8 运行时；' -ForegroundColor DarkGray
Write-Host '          若要让用户"装完即用"，需要改成 self-contained 发布（SelfContained=true）。' -ForegroundColor DarkGray
