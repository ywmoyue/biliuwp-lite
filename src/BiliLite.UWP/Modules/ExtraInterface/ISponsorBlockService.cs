using BiliLite.Models.Common.Player;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliLite.Modules.ExtraInterface
{
    public interface ISponsorBlockService
    {
        string DefaultApiUrl { get; }
        Task LoadSponsorBlock(string bvid);
        List<PlayerSkipItem> GetVideoSponsorBlocks(string bvid, string cid, double duration);
        void RemoveSponsorBlockCache(string bvid);
    }
}