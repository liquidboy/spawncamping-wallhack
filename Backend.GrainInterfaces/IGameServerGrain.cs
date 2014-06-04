namespace Backend.GrainInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGameServerGrain : Orleans.IGrain
    {
        Task StartGame(List<IPlayerGrain> playersToLaunch);
    }
}
