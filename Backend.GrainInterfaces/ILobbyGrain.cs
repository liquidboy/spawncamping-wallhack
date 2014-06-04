namespace Backend.GrainInterfaces
{
    using Frontend.Library.Models;
    using System.Threading.Tasks;

    public interface ILobbyGrain : Orleans.IGrain
    {
        /// <summary>
        /// Gets called by IPlayerGrain.
        /// </summary>
        /// <param name="gameServerStartParams"></param>
        /// <returns></returns>
        Task RegisterPlayer(IPlayerGrain player, PlayerInfo playerInfo);
    }
}
