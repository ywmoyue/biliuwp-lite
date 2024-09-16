<link rel="stylesheet" href="https://support.microsoft.com/css/glyphs/glyphs.css?v=N9jMfMIoO_s7OATN0j5LYqmO9MCqHDjfpaUV2RuaEy8"/> <!-- <p>先别删这行，在测试显示windowslogo</p> -->
<link rel="stylesheet" href="https://support.microsoft.com/css/fonts/site-fonts.css?v=4M_1wOASateOs9zdphCtIqMvtKo366Gf6pkOjDqzkYo">

# Biliuwp-lite安装教程
- [Windows10 安装教程（脚本安装方式）](install-readme.md#Windows10)
- [Windows11 安装教程（Appx直接安装方式/不推荐）](install-readme.md#Windows11)
- [常见问题](install-readme.md#其他问题)
  - [网络/代理问题](install-readme.md#网络代理问题)
## 安装要求系统版本信息

- Windows10 1903及以上
- 4.6.x 版本更新 4.7.x， 需要卸载并重启系统后使用脚本方式安装
- 4.7.x第一次安装需要使用脚本方式安装

## Windows10

> Win10系统请勿直接运行`.msix`安装包进行安装，可能导致奇怪的问题

### 启用开发者模式

1. 点<kbd>Windows</kbd><span class="icon-fluent icon-windowslogo" aria-hidden="true"></span>+<kbd>I</kbd>键打开 `Windows设置` 按图所示操作
![](./_img/win10-developer-mode-01.drawio.png)
![](./_img/win10-developer-mode-02.drawio.png)

### 安装证书

1. 双击打开 `.cer` 证书文件. 如果没有 `.cer` 证书文件则右键 `.appx` 文件-属性-数字签名-详细
信息-查看证书
![](./_img/install-cert-01.drawio.png)

2. 点击安装证书，将证书安装到本地计算机

![](./_img/install-cert-02.drawio.png)

3. 选择受信任的根证书颁发机构，下一步，完成即可

![](./_img/install-cert-03.drawio.png)

### 检查脚本执行权限

1. 点<kbd>Windows</kbd>+<kbd>X</kbd>键并选择以管理员身份启动 终端/PowerShell

![](./_img/check-ps1-permission-01.drawio.png)

2. 执行命令 `get-ExecutionPolicy`, 如果输出 `Restricted` 表示 禁止执行脚本,如果输出 `RemoteSigned` 表示 可以执行脚本

![](./_img/check-ps1-permission-02.drawio.png)

3. 执行命令`Set-ExecutionPolicy -ExecutionPolicy RemoteSigned`并按<kbd>Y</kbd>键启用脚本执行权限

### 运行安装脚本

1. 在安装包解压目录中按住键盘shift并单击鼠标右键打开powershell，执行脚本 `.\Install.ps1` 开始安装

![](./_img/run-ps1-script.drawio.png)


## Windows11
### 安装证书

1. 双击打开 `.cer` 证书文件. 如果没有 `.cer` 证书文件则 **右键 `.appx` 文件-属性-数字签名-详细信息-查看证书**
![](./_img/install-cert-01.drawio.png)

2. 点击安装证书，将证书安装到`本地计算机`

![](./_img/install-cert-02.drawio.png)

3. 选择受`信任的根证书颁发机构`，下一步，完成即可

![](./_img/install-cert-03.drawio.png)

### 运行Msix安装包

![](./_img/install-msix.drawio.png)

## 其他问题：
### 网络/代理问题
1. 脚本安装后无法在 Clash For Windows<sup>1</sup> 中解除对此UWP应用的联网限制
   - 可尝试再脚本安装后再使用`.msix`安装包包 Reinstall 一遍（此解决方案仅限 Windows11, WIndows10 未经测试）
   - 如有更好解决方案可[点此前往 Github](https://github.com/ywmoyue/biliuwp-lite/issues/new/choose)提交 issue
  
---
  #### 注1: [Clash For Windows](https://github.com/Z-Siqi/Clash-for-Windows_Chinese) 是一款基于Clash核心的图形界面代理软件，它通过提供一个友好的界面来帮助你简单配置和管理网络代理，让你可以更自由、灵活地浏览全球互联网内容。


