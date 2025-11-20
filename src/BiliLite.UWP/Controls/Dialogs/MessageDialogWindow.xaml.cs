using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using BiliLite;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BiliLite.Controls.Dialogs
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessageDialogWindow : WindowEx
    {
        public delegate void CommandSelectedHandler(IUICommand command);
        public event CommandSelectedHandler OnCommandSelected;

        private List<UICommand> _commands;

        public MessageDialogWindow(string title, string content, List<UICommand> commands)
        {
            this.InitializeComponent();

            _commands = commands;

            // ���ô�������
            this.Title = string.IsNullOrEmpty(title) ? "��ʾ" : title;
            
            MessageTextBlock.Text = content;

            // ������ť
            CreateButtons();

            // ������ʾ
            CenterToScreen();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 400, Height = 250 });
        }

        private void CreateButtons()
        {
            ButtonsPanel.Children.Clear();

            foreach (var command in _commands)
            {
                var button = new Button
                {
                    Content = command.Label,
                    Width = 80,
                    Height = 32,
                    Margin = new Thickness(10, 0, 10, 0),
                    Tag = command // �洢�������
                };

                button.Click += (sender, e) =>
                {
                    var cmd = (sender as Button)?.Tag as UICommand;
                    if (cmd != null)
                    {
                        // ��������� Invoked ����
                        cmd.Invoked?.Invoke(new UICommand { Label = cmd.Label, Id = cmd.Id });

                        // ����ѡ���¼�
                        OnCommandSelected?.Invoke(new UICommand { Label = cmd.Label, Id = cmd.Id });

                        this.Close();
                    }
                };

                ButtonsPanel.Children.Add(button);
            }
        }

        private void CenterToScreen()
        {
            try
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

                if (displayArea != null)
                {
                    int windowWidth = (int)appWindow.Size.Width;
                    int windowHeight = (int)appWindow.Size.Height;
                    int screenWidth = displayArea.WorkArea.Width;
                    int screenHeight = displayArea.WorkArea.Height;

                    int x = (screenWidth - windowWidth) / 2;
                    int y = (screenHeight - windowHeight) / 2;

                    appWindow.Move(new Windows.Graphics.PointInt32(x, y));
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"���д���ʧ��: {ex.Message}");
            }
        }
    }


    public class MessageDialog
    {
        private string _content;
        private string _title;
        private List<UICommand> _commands = new List<UICommand>();

        public MessageDialog(string content) : this(content, string.Empty)
        {
        }

        public MessageDialog(string content, string title)
        {
            _content = content;
            _title = title;
        }

        public IList<UICommand> Commands => _commands;

        public async Task<IUICommand> ShowAsync()
        {
            var tcs = new TaskCompletionSource<IUICommand>();

            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    // �����Զ���Ի��򴰿�
                    var dialogWindow = new MessageDialogWindow(_title, _content, _commands);

                    dialogWindow.OnCommandSelected += (command) =>
                    {
                        tcs.TrySetResult(command);
                    };

                    dialogWindow.Activate();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"��ʾ�Ի���ʧ��: {ex.Message}");
                    // ����Ĭ�ϵ�ȡ������
                    tcs.TrySetResult(new UICommand { Label = "ȡ��", Id = false });
                }
            });

            return await tcs.Task;
        }
    }

}
