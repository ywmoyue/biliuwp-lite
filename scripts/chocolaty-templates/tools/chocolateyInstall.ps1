# Due to Powershell limitations, this script can only be opened with GBK encoding

# ��ȡ��ǰ�ű����ڵ�Ŀ¼
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ָ��Ҫ���еĽű��ļ�·��
$subInstallScriptPath = Join-Path $scriptDir "subInstall.ps1"

# ����һ���µ�PowerShell���ڲ�����subInstall.ps1�ű�
Start-Process powershell.exe -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$subInstallScriptPath`"" -Wait

# �ȴ��ű�ִ����ɺ󣬼���Ƿ�ɹ�
# ����ͳɹ��ļ���·��
$errorFilePath = Join-Path $scriptDir "..\temp\error"
$successFilePath = Join-Path $scriptDir "..\temp\success"

# ���ɹ��ļ��Ƿ����
if (Test-Path $successFilePath) {
    Write-Host "�ű�ִ�гɹ�"
    # ����ѡ��ɾ���ɹ��ļ�
    Remove-Item $successFilePath
} else {
    # �������ļ��Ƿ����
    if (Test-Path $errorFilePath) {
        Write-Host "�ű�ִ��ʧ�ܣ������ļ��Ѵ���"
        # ��ȡ�����ļ����ݲ���ӡ
        $errorContent = Get-Content $errorFilePath
        Write-Host "��������: $errorContent"
        # ����ѡ��ɾ�������ļ�
        Remove-Item $errorFilePath
    } else {
        Write-Host "�ű�ִ��ʧ�ܣ���δ���������ļ�"
        exit 1
    }
}