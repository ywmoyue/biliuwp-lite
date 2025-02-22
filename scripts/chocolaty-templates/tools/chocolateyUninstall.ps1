# Due to Powershell limitations, this script can only be opened with GBK encoding
$PackageId = "5422.502643927C6AD"

$PackageNamePrefix = "{0}_" -f $PackageId
Write-Host "PackageNamePrefix: $PackageNamePrefix"

# 获取UWP包完整名称
$PackageFullName = (Get-AppxPackage | Where-Object { $_.PackageFullName -like "$PackageNamePrefix*" }).PackageFullName
Write-Host "PackageFullName: $PackageFullName"

if ($PackageFullName) {
    Write-Host "找到 UWP 包: $PackageFullName"
    # 卸载 UWP 包
    Remove-AppxPackage -Package $PackageFullName
    Write-Host "UWP 包已卸载"
} else {
    Write-Host "未找到匹配的 UWP 包"
}