namespace Backend.GrainInterfaces
{
    using Frontend.Library.Models;
    using System.Threading.Tasks;

    public interface IPlayerGrain : Orleans.IGrain
    {
        Task<PlayerInfo> Subscribe(IPlayerObserver playerObserver);

        Task GameServerStarts(GameServerStartParams gameServerStartParams);

        Task GameEnded();

        Task<GameServerStartParams> GetCurrentGame();
    }
}
