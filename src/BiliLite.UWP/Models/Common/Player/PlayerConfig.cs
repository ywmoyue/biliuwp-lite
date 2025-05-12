namespace BiliLite.Models.Common.Player
{
    public class PlayerConfig
    {
        public bool EnableHw { get; set; }

        public LivePlayerMode PlayMode { get; set; }

        public RealPlayerType PlayerType { get; set; }

        public int SelectedRouteLine { get; set; } = 0;
    }
}
