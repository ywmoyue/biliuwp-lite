using BiliLite.UWP;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{

    public sealed partial class MessageToast : UserControl
    {
        private Popup m_Popup;

        private string m_TextBlockContent = "";
        private TimeSpan m_ShowTime;
        public MessageToast(XamlRoot xamlRoot)
        {
            InitializeComponent();
            this.XamlRoot = xamlRoot;
            m_Popup = new Popup();
            m_Popup.XamlRoot = xamlRoot;
            m_Popup.Child = this;
            Loaded += NotifyPopup_Loaded;
            Unloaded += NotifyPopup_Unloaded;
        }

        public MessageToast(XamlRoot xamlRoot, string content, TimeSpan showTime) : this(xamlRoot)
        {
            if (m_TextBlockContent == null)
            {
                m_TextBlockContent = "";
            }
            m_TextBlockContent = content;
            m_ShowTime = showTime;
        }
        public MessageToast(XamlRoot xamlRoot, string content, TimeSpan showTime, List<MyUICommand> commands) : this(xamlRoot)
        {
            if (m_TextBlockContent == null)
            {
                m_TextBlockContent = "";
            }
            m_TextBlockContent = content;
            m_ShowTime = showTime;
            foreach (var item in commands)
            {
                HyperlinkButton button = new HyperlinkButton()
                {
                    Margin = new Thickness(8, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = new TextBlock() { Text = item.Label }
                };
                button.Click += new RoutedEventHandler((sender, e) =>
                {
                    item.Invoked?.Invoke(this, item);
                });
                btns.Children.Add(button);
            }
        }


        public void Show()
        {
            m_Popup.IsOpen = true;

        }
        public async void Close()
        {
            await this.Offset(offsetX: 0, offsetY: (float)border.ActualHeight, duration: 200, delay: 0);
            m_Popup.IsOpen = false;
        }
        private async void NotifyPopup_Loaded(object sender, RoutedEventArgs e)
        {
            var window = this.GetCurrentWindow();
            Width = window.Bounds.Width;
            Height = window.Bounds.Height;
            if (m_TextBlockContent == null)
            {
                m_TextBlockContent = "";
            }
            tbNotify.Text = m_TextBlockContent;
            window.SizeChanged += Current_SizeChanged;

            await this.Offset(offsetX: 0, offsetY: -72, duration: 200, delay: 0);
            await this.Offset(offsetX: 0, offsetY: (float)border.ActualHeight, duration: 200, delay: (int)m_ShowTime.TotalMilliseconds);
            m_Popup.IsOpen = false;
        }


        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Width = e.Size.Width;
            Height = e.Size.Height;
        }

        private void NotifyPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = this.GetCurrentWindow();
            window.SizeChanged -= Current_SizeChanged;
        }


    }

    public class MyUICommand
    {
        public MyUICommand(string lable)
        {
            Label = lable;
        }
        public MyUICommand(string lable, EventHandler<MyUICommand> invoked)
        {
            Label = lable;
            Invoked = invoked;
        }
        public object Id { get; set; }
        public EventHandler<MyUICommand> Invoked { get; set; }
        public string Label { get; set; }


    }
}
