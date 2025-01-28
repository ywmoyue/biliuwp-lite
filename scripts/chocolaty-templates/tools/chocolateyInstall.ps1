# Due to Powershell limitations, this script can only be opened with GBK encoding
$AppName = "BiliLite.Packages"
$Version = "{version}"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Output "scriptDir: $scriptDir"

# ��ȡϵͳ�ܹ�
$arch = $env:PROCESSOR_ARCHITECTURE
# �жϼܹ�����ֵ
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

# �ҵ���Ӧ�İ�װ������ѹ��unpack·��
$PackageZipRelativePath = "..\resources\{0}.zip" -f $Package
$PackageZipPath = Join-Path $scriptDir $PackageZipRelativePath
$UnpackPath = Join-Path $scriptDir "..\unpack"

# ����ļ��Ƿ���ڲ���ѹ
if (Test-Path $PackageZipPath) {
    # ȷ����ѹ·������
    if (-not (Test-Path $UnpackPath)) {
        New-Item -ItemType Directory -Path $UnpackPath | Out-Null
    }

    # ��ѹ�ļ�
    Expand-Archive -Path $PackageZipPath -DestinationPath $UnpackPath -Force
    Write-Host "�ļ��ѽ�ѹ�� $UnpackPath"
} else {
    Write-Host "�ļ� $PackageZipPath �����ڣ������� GitHub ���ص�ǰƽ̨����İ�װ��"

    # �� GitHub �������������ѹ
    $downloadUrl = "https://github.com/ywmoyue/biliuwp-lite/releases/download/v{0}/{1}.zip" -f $Version, $Package
    Write-Host "���ص�ַ: $downloadUrl"

    try {
        # �����ļ�
        Invoke-WebRequest -Uri $downloadUrl -OutFile $PackageZipPath
        Write-Host "�ļ��������: $PackageZipPath"

        # ȷ����ѹ·������
        if (-not (Test-Path $UnpackPath)) {
            New-Item -ItemType Directory -Path $UnpackPath | Out-Null
        }

        # ��ѹ�ļ�
        Expand-Archive -Path $PackageZipPath -DestinationPath $UnpackPath -Force
        Write-Host "�ļ��ѽ�ѹ�� $UnpackPath"
    } catch {
        Write-Host "���ػ��ѹʧ��: $_"
        exit 1
    }
}

# ���ÿ�����ģʽ����װδ֪��Դ��
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name "AllowDevelopmentWithoutDevLicense" -Value 1
Write-Host "������װδ֪��Դ��"

# ֤��·��
$certRelativePath = "..\unpack\{0}_Test\{0}.cer" -f $Package
$certPath = Resolve-Path (Join-Path $scriptDir $certRelativePath)

# ��װ�ű�·��
$installScriptPath = "$scriptDir\..\unpack\{0}_Test\install.ps1" -f $Package
Write-Host "��װ·�� $installScriptPath"

# ��װ֤��
try {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cert.Import($certPath.Path)

    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    $store.Open("ReadWrite")
    $store.Add($cert)
    $store.Close()
    Write-Host "֤�鰲װ���"
} catch {
    Write-Host "֤�鰲װʧ��: $_"
    exit 1
}

# ִ�а�װ�ű�
try {
    . "$installScriptPath"
    Write-Host "��װ�ű�ִ�гɹ�"
} catch {
    Write-Host "��װ�ű�ִ��ʧ��: $_"
    exit 1
}