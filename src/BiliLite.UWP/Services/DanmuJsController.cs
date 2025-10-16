using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Controls.Common;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Video;
using NSDanmaku.Model;
using Atelier39;

namespace BiliLite.Services
{
    public class DanmakuJsController : IDanmakuController
    {
        private DanmakuWeb m_danmakuElement;
        private List<DanmakuModel> m_danmakuList;

        public DanmakuJsController()
        {
            DanmakuViewModel = new DanmakuViewModel()
            {
                ShowAreaControl = true,
                ShowBoldControl = false,
                ShowBoldStyleControl = false,
                IsHide = false,
            };
        }

        public override void Init(object danmakuElement)
        {
            m_danmakuElement = danmakuElement as DanmakuWeb;
            m_danmakuElement.DanmakuLoaded += (s, e) => OnDanmakuLoaded();
        }

        public override void Clear()
        {
            m_danmakuElement?.Clear();
            m_danmakuList?.Clear();
        }

        public override void Hide()
        {
            base.Hide();
            if (m_danmakuElement != null)
            {
                m_danmakuElement.Visibility = Visibility.Collapsed;
                m_danmakuElement.Pause();
            }
        }

        public override void Show()
        {
            base.Show();
            if (m_danmakuElement != null)
            {
                m_danmakuElement.Visibility = Visibility.Visible;
                m_danmakuElement.Resume();
            }
        }

        public override void HideTop()
        {
            m_danmakuElement?.HideMode("top");
        }

        public override void HideBottom()
        {
            m_danmakuElement?.HideMode("bottom");
        }

        public override void HideScroll()
        {
            m_danmakuElement?.HideMode("scroll");
        }

        public override void ShowTop()
        {
            m_danmakuElement?.ShowMode("top");
        }

        public override void ShowBottom()
        {
            m_danmakuElement?.ShowMode("bottom");
        }

        public override void ShowScroll()
        {
            m_danmakuElement?.ShowMode("scroll");
        }

        public override void SetFont(string fontName)
        {
            // danmu.js 字体设置需要通过样式实现
        }
        public override void SetFontZoom(double fontZoom)
        {
            base.SetFontZoom(fontZoom);
            m_danmakuElement?.SetFontSize(20 * fontZoom);
        }

        public override void SetArea(double area)
        {
            base.SetArea(area);
            var start = 0.0;
            var end = area;
            m_danmakuElement?.SetArea(start, end);
        }

        public override void SetOpacity(double opacity)
        {
            base.SetOpacity(opacity);
            m_danmakuElement?.SetOpacity(opacity);
        }

        public override void SetSpeed(int speed)
        {
            base.SetSpeed(speed);
            var rate = speed / 10.0;
            m_danmakuElement?.SetPlayRate("scroll", rate);
        }

        public override void Load(IEnumerable danmakuList)
        {
            var realDanmakuList = (danmakuList as List<DanmakuItem>).ToList();

            var config = new DanmakuConfig
            {
                AreaStart = 0,
                AreaEnd = DanmakuViewModel.Area,
                ChannelSize = 40,
                MouseControl = false,
                MouseControlPause = false,
                DefaultOff = false,
                ChaseEffect = true
            };

            m_danmakuElement?.LoadDanmaku(realDanmakuList, config);
        }

        public override void Add(BiliDanmakuItem danmakuItem, bool owner)
        {
            //if (owner)
            //{
            //    //danmakuItem.HasBorder = true;
            //    danmakuItem.Color = Colors.Orange;
            //}

            //m_danmakuElement?.SendComment(danmakuItem);
        }

        public override void AddLiveDanmaku(string text, bool owner, Color color)
        {
            Add(new BiliDanmakuItem()
            {
                Color = color,
                Text = text,
                Size = 25,
                Location = DanmakuLocation.Scroll,
                Time = 0
            }, owner);
        }

        public override void Pause()
        {
            m_danmakuElement?.Pause();
        }

        public override void Resume()
        {
            m_danmakuElement?.Resume();
        }

        public override void UpdateTime(long position)
        {
            base.UpdateTime(position);
            // danmu.js 会自动处理时间同步，这里不需要额外操作
        }

        private void OnDanmakuLoaded()
        {
            // 弹幕加载完成后的处理
        }
    }
}
