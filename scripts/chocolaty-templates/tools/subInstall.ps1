# Due to Powershell limitations, this script can only be opened with GBK encoding

# �� $TempPath Ŀ¼���½��ļ� error
function Create-ErrorFile {
    $errorFilePath = Join-Path $TempPath "error"
    if (-not (Test-Path $errorFilePath)) {
        New-Item -ItemType File -Path $errorFilePath | Out-Null
    }
    Write-Host "�����ļ��Ѵ���: $errorFilePath"
}

# �� $TempPath Ŀ¼���½��ļ� success
function Create-SuccessFile {
    $successFilePath = Join-Path $TempPath "success"
    if (-not (Test-Path $successFilePath)) {
        New-Item -ItemType File -Path $successFilePath | Out-Null
    }
    Write-Host "�ɹ��ļ��Ѵ���: $successFilePath"
}

# ������ԱȨ��
If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
   Write-Warning "��ǰ�ű���Ҫ�Թ���ԱȨ�����У����Թ���Ա������� PowerShell ������ִ�д˽ű���"
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
            Create-ErrorFile
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
    $installPackagePath = "$scriptDir\..\unpack\{0}_Test" -f $Package
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
        Create-ErrorFile
        exit 1
    }

    # ִ�а�װ�ű�
    try {
        . "$installScriptPath" -Force
        Write-Host "��װ�ű�ִ�гɹ�"
    } catch {
        Write-Host "��װ�ű�ִ��ʧ��: $_"
        Create-ErrorFile
        exit 1
    }

    $PackageId = "5422.502643927C6AD"

    $PackageNamePrefix = "{0}_{1}" -f $PackageId, $Version
    Write-Host "PackageNamePrefix: $PackageNamePrefix"

    # ����Ƿ�װ�ɹ�
    $PackageFullName = (Get-AppxPackage | Where-Object { $_.PackageFullName -like "$PackageNamePrefix*" }).PackageFullName
    if (-not $PackageFullName) {
        Write-Host "Ӧ�ð�װ����֤ʧ�ܣ����鰲װ��־"
        Create-ErrorFile
        exit 1
    } else {
        Write-Host "Ӧ���Ѱ�װ"
        Create-SuccessFile
    }
} catch {
    Write-Host "��������"
    Write-Host $_.Exception.Message
    #Read-Host -Prompt "�����������..."
}