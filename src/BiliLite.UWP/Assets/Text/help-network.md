### 应用无法联网

如果你开着网络代理时本应用无法联网，关掉代理就正常，那么原因是：

Win10 的 UWP 应用都运行在名为 App Container 的沙箱环境中，沙箱保证了应用安全，但同时阻止了网络流量发往本机（loopback）。所以即使你在系统设置里启用了代理，UWP 应用也无法访问本机的代理服务器。

解决办法是解除本应用的网络隔离，下面两种方法任选其一：

方法一：使用 Fiddler 解除（比较简单）

* 下载并安装 [Fiddler](https://www.telerik.com/download/fiddler)（下载前的资料可以随便填）

* 打开 Fiddler，点击工具栏上的 `WinConfig`

* 在弹出的列表中找到并勾选 `哔哩哔哩 UWP`，然后点击 `Save Changes` 保存

* 之后开着代理再打开应用，就可以正常联网了

方法二：使用 CheckNetIsolation.exe 解除（稍微麻烦一点）

* `Win` + `R` 打开运行窗口，输入 `Regedit` 打开注册表编辑器，在地址栏粘贴 `HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Mappings` 并回车

* `Mappings` 文件夹里的每一项就是本机所有 UWP 应用的 SID，逐个点击，右侧会显示对应的应用名称，找到本应用后复制它的 SID

* `Win` + `R` 输入 `CMD` 打开命令行，执行 `CheckNetIsolation.exe loopbackexempt -a -p=SID`，其中等号后面换成刚刚复制的 SID

* 命令行提示完成后一般就成功了。一次只能解除一个应用，需要解除多个应用时，找到对应的 SID 依次执行即可

### 参考原文

* 《解决使用代理时 Win10 UWP 应用无法联网问题》[https://www.jianshu.com/p/9d1566aa94cf](https://www.jianshu.com/p/9d1566aa94cf)
