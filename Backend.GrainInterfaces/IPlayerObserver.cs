namespace Backend.GrainInterfaces
{
    using Frontend.Library.Models;

    public interface IPlayerObserver : Orleans.IGrainObserver
    {
        void GameServerStarts(GameServerStartParams gameServerStartParams);
    }
}
