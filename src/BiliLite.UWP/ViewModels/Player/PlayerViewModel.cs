using System;
using BiliLite.Controls;
using BiliLite.Player.States.PlayStates;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Player
{
    public class PlayerViewModel : BaseViewModel
    {
        private IPlayer m_player;

        public IPlayState PlayState { get; set; }

        public double SourcePosition { get; set; }

        [DependsOn(nameof(SourcePosition))]
        public double Position
        {
            get => SourcePosition;
            set
            {
                if(Math.Abs(SourcePosition - value) < 0.01) return;
                m_player.SetPosition(value);
            }
        }

        public double Duration { get; set; }

        public double SourceVolume { get; set; }

        [DependsOn(nameof(SourceVolume))]
        public double Volume
        {
            get => SourceVolume;
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                if (value < 0)
                {
                    value = 0;
                }
                m_player.SetVolume(value);
            }
        }

        public void SetPlayer(IPlayer player)
        {
            m_player = player;
        }
    }
}
