using BiliLite.Models.Common.Player;
using System;
using System.Collections.Generic;

namespace BiliLite.Modules.ExtraInterface
{
    public interface IPlayerSponsorBlockControl
    {
        List<PlayerSkipItem> SegmentSkipItems { get; }
        event EventHandler<double> UpdatePosition;
        void LoadSponsorBlock(string bvid, string cid, double duration);
    }
}