namespace Backend.GrainInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Orleans;

    public class GameServerStartParams
    {
        public string GameServerID { get; set; }
    }

    public interface IPlayerObserver : IGrainObserver
    {
        void GameServerStarts(GameServerStartParams gameServerStartParams);
    }

    public class PlayerInfo
    {
        public string PlayerName { get; set; }

        public int Strength { get; set; }
    }

    public interface IPlayerRegistrationGrain : IGrain
    {
        Task Register(IPlayerObserver playerObserver);
    }

    public interface IPlayerGrain : IGrain
    {
        Task<PlayerInfo> Subscribe(IPlayerObserver playerObserver);

        Task GameServerStarts(GameServerStartParams gameServerStartParams);

        Task GameEnded();

        Task<GameServerStartParams> GetCurrentGame();
    }

    public interface IPlayerGrainState : IGrainState
    {
        PlayerInfo PlayerInfo { get; set; }

        GameServerStartParams CurrentGame { get; set; }
    }

    public interface ILobbyGrain : IGrain
    {
        /// <summary>
        /// Gets called by IPlayerGrain.
        /// </summary>
        /// <param name="gameServerStartParams"></param>
        /// <returns></returns>
        Task RegisterPlayer(IPlayerGrain player, PlayerInfo playerInfo);
    }

    public interface ILobbyGrainState : IGrainState
    {
        string SomeCrap { get; set; }
    }

    public interface IGameServerGrain : IGrain
    {
        Task StartGame(List<IPlayerGrain> playersToLaunch);
    }

    public interface IGameServerGrainState : IGrainState
    {
        List<string> Players { get; set; }
    }
}