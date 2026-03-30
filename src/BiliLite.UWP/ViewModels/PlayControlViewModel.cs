using BiliLite.Modules.Player;
using BiliLite.ViewModels.Common;
using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Modules;

namespace BiliLite.ViewModels
{
    public class PlayControlViewModel: BaseViewModel
    {
        private List<InteractionEdgeInfoQuestionModel> _questions;
        public List<InteractionEdgeInfoQuestionModel> Questions
        {
            get => _questions;
            set => Set(ref _questions, value);
        }

        public bool ShowVideoBottomVirtualProgressBar =>
            SettingService.GetValue(SettingConstants.Player.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR,
                SettingConstants.Player.DEFAULT_SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR);

        private bool _showViewPointsView;
        public bool ShowViewPointsView
        {
            get => _showViewPointsView;
            set => Set(ref _showViewPointsView, value);
        }

        private bool _showViewPointsBtn;
        public bool ShowViewPointsBtn
        {
            get => _showViewPointsBtn;
            set => Set(ref _showViewPointsBtn, value);
        }

        private bool _showWebPlayerToolbarButton;
        public bool ShowWebPlayerToolbarButton
        {
            get => _showWebPlayerToolbarButton;
            set => Set(ref _showWebPlayerToolbarButton, value);
        }

        private bool _showWebPlayerToolbar;
        public bool ShowWebPlayerToolbar
        {
            get => _showWebPlayerToolbar;
            set => Set(ref _showWebPlayerToolbar, value);
        }

        private List<PlayerInfoViewPoint> _viewPoints;
        public List<PlayerInfoViewPoint> ViewPoints
        {
            get => _viewPoints;
            set => Set(ref _viewPoints, value);
        }

        private double _position;
        public double Position
        {
            get => _position;
            set => Set(ref _position, value);
        }

        private double _duration;
        public double Duration
        {
            get => _duration;
            set => Set(ref _duration, value);
        }
    }
}
