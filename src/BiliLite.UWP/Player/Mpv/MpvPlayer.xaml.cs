using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Player.Mpv
{
    public sealed partial class MpvPlayer : UserControl
    {
        private MpvClient m_mpvClient;
        private MpvRender m_mpvRender;

        public MpvPlayer()
        {
            this.InitializeComponent();
        }

        public double Duration => m_mpvClient.Duration;

        public event EventHandler MediaLoaded;

        public void Load(string videoUrl,string userAgent,string referer)
        {
            if (m_mpvClient == null)
            {
                m_mpvClient = new MpvClient();
                m_mpvClient.FileLoaded += (_, e) => { MediaLoaded?.Invoke(this, e); };
                m_mpvClient.Initialize();
                m_mpvClient.SetPropertyString("options/http-header-fields", $"User-Agent: {userAgent}");
                m_mpvClient.SetPropertyString("options/http-header-fields", $"Referer: {referer}");
            }

            if (m_mpvRender == null)
            {
                m_mpvRender = new MpvRender(m_mpvClient);

                m_mpvRender.Initialize(SwapChainPanel, videoUrl);
            }
        }

        public void Pause()
        {
            m_mpvClient.Pause();
        }

        public void Resume()
        {
            m_mpvClient.Play();
        }
    }
}
