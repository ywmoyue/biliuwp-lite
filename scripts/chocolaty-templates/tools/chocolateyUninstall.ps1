# Due to Powershell limitations, this script can only be opened with GBK encoding
$PackageId = "5422.502643927C6AD"

$PackageNamePrefix = "{0}_" -f $PackageId
Write-Host "PackageNamePrefix: $PackageNamePrefix"

# ��ȡUWP����������
$PackageFullName = (Get-AppxPackage | Where-Object { $_.PackageFullName -like "$PackageNamePrefix*" }).PackageFullName
Write-Host "PackageFullName: $PackageFullName"

if ($PackageFullName) {
    Write-Host "�ҵ� UWP ��: $PackageFullName"
    # ж�� UWP ��
    Remove-AppxPackage -Package $PackageFullName
    Write-Host "UWP ����ж��"
} else {
    Write-Host "δ�ҵ�ƥ��� UWP ��"
}