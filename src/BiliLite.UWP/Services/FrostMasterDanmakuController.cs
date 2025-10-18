using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Atelier39;
using AutoMapper;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Video;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace BiliLite.Services
{
    public class FrostMasterDanmakuController : IDanmakuController
    {
        private readonly IMapper m_mapper;
        private CanvasAnimatedControl m_danmakuCanvas;
        private DanmakuFrostMaster m_danmakuMaster;
        private List<DanmakuItem> m_danmakuItems;

        public FrostMasterDanmakuController(IMapper mapper)
        {
            m_mapper = mapper;
            DanmakuViewModel = new DanmakuViewModel()
            {
                ShowAreaControl = true,
                ShowBoldControl = true,
                ShowBoldStyleControl = false,
                IsHide = false,
            };
        }

        public bool IsShow => DanmakuViewModel.IsHide;

        public override void Init(UserControl danmakuElement)
        {
            m_danmakuCanvas = danmakuElement as CanvasAnimatedControl;
            m_danmakuMaster = new DanmakuFrostMaster(m_danmakuCanvas);
            m_danmakuMaster.SetAutoControlDensity(false);
            m_danmakuMaster.SetRollingDensity(-1);
            m_danmakuMaster.SetIsTextBold(true);
            m_danmakuMaster.SetBorderColor(Colors.Blue);
            if (SettingService.GetValue(SettingConstants.VideoDanmaku.DANMAKU_DEBUG_MODE, false))
            {
                m_danmakuMaster.DebugMode = true;
            }
        }

        public override void Clear()
        {
            m_danmakuMaster.Clear();
        }

        public override void Hide()
        {
            base.Hide();
            m_danmakuCanvas.Visibility = Visibility.Collapsed;
            DanmakuViewModel.IsHide = true;
        }

        public override void Show()
        {
            base.Show();
            m_danmakuCanvas.Visibility = Visibility.Visible;
            DanmakuViewModel.IsHide = false;
        }

        public override void HideTop()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.TopLayerId, false);
        }

        public override void HideBottom()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.BottomLayerId, false);
        }

        public override void HideScroll()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.RollingLayerId, false);
        }

        public override void ShowTop()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.TopLayerId, true);
        }

        public override void ShowBottom()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.BottomLayerId, true);
        }

        public override void ShowScroll()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.RollingLayerId, true);
        }

        public override void SetFont(string fontName)
        {
            m_danmakuMaster.SetFontFamilyName(fontName);
        }

        public override void SetFontZoom(double fontZoom)
        {
            base.SetFontZoom(fontZoom);
            var fontLevel = fontZoom * 3;
            m_danmakuMaster.SetDanmakuFontSizeOffset((int)fontLevel);
        }

        public override void SetSpeed(int speed)
        {
            base.SetSpeed(speed);
            speed /= 2;
            m_danmakuMaster.SetRollingSpeed(speed);
        }

        public override void SetTopMargin(double topMargin)
        {
            base.SetTopMargin(topMargin);
            m_danmakuCanvas.Margin = new Thickness(0, topMargin, 0, 0);
        }

        public override void SetOpacity(double opacity)
        {
            base.SetOpacity(opacity);
            m_danmakuMaster.SetOpacity(opacity);
        }

        public override void SetBold(bool bold)
        {
            base.SetBold(bold);
            m_danmakuMaster.SetIsTextBold(bold);
        }

        public override void SetArea(double area)
        {
            base.SetArea(area);
            m_danmakuMaster.SetRollingAreaRatio((int)(area*10));
        }

        //public override void SetDensity(int density)
        //{
        //    base.SetDensity(density);
        //    //m_danmakuMaster.SetRollingDensity(density-1);
        //}

        public override void Load(IEnumerable danmakuList)
        {
            var realDanmakuList = (danmakuList as List<DanmakuItem>).ToList();
            m_danmakuItems = realDanmakuList;
            m_danmakuMaster.SetDanmakuList(realDanmakuList);
        }

        public override void Add(BiliDanmakuItem danmakuItem, bool owner)
        {
            var realDanmakuItem = m_mapper.Map<DanmakuItem>(danmakuItem);
            if (owner) realDanmakuItem.HasBorder = true;
            m_danmakuMaster.AddRealtimeDanmaku(realDanmakuItem, false);
        }

        public override void AddLiveDanmaku(string text, bool owner, Color color)
        {
            Add(new BiliDanmakuItem()
            {
                Color = color,
                Text = text,
                Size = 25,
            }, owner);
        }

        public override void Pause()
        {
            m_danmakuMaster.Pause();
        }

        public override void Resume()
        {
            m_danmakuMaster.Resume();
        }

        public override void UpdateTime(long position)
        {
            base.UpdateTime(position);
            m_danmakuMaster.UpdateTime((uint)position * 1000);
        }

        public override List<DanmakuSimpleItem> FindDanmakus(int? progress = null)
        {
            var query = m_danmakuItems.AsEnumerable();
            if (progress != null)
            {
                query = query.Where(x => x.StartMs / 1000 > progress - 10 && x.StartMs / 1000 < progress + 10);
            }

            return m_mapper.Map<List<DanmakuSimpleItem>>(query);
        }
    }
}
