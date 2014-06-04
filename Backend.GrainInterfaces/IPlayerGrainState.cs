namespace Backend.GrainInterfaces
{
    using Frontend.Library.Models;

    public interface IPlayerGrainState : Orleans.IGrainState
    {
        PlayerInfo PlayerInfo { get; set; }

        GameServerStartParams CurrentGame { get; set; }
    }
}
