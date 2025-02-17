# Due to Powershell limitations, this script can only be opened with GBK encoding

# 获取当前脚本所在的目录
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# 指定要运行的脚本文件路径
$subInstallScriptPath = Join-Path $scriptDir "subInstall.ps1"

# 启动一个新的PowerShell窗口并运行subInstall.ps1脚本
Start-Process powershell.exe -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$subInstallScriptPath`"" -Wait

# 等待脚本执行完成后，检查是否成功
# 错误和成功文件的路径
$errorFilePath = Join-Path $scriptDir "..\temp\error"
$successFilePath = Join-Path $scriptDir "..\temp\success"

# 检查成功文件是否存在
if (Test-Path $successFilePath) {
    Write-Host "脚本执行成功"
    # 可以选择删除成功文件
    Remove-Item $successFilePath
} else {
    # 检查错误文件是否存在
    if (Test-Path $errorFilePath) {
        Write-Host "脚本执行失败，错误文件已创建"
        # 读取错误文件内容并打印
        $errorContent = Get-Content $errorFilePath
        Write-Host "错误内容: $errorContent"
        # 可以选择删除错误文件
        Remove-Item $errorFilePath
    } else {
        Write-Host "脚本执行失败，但未创建错误文件"
        exit 1
    }
}