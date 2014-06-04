namespace Backend.GrainImplementations
{
    using Backend.GrainInterfaces;
    using Orleans;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Frontend.Library.Models;

    [Reentrant]
    public class LobbyGrain : 
        GrainBase<ILobbyGrainState> /* Make it to appear stateful, so that there's only one #badidea?*/, 
        ILobbyGrain
    {
        private readonly int MAX_PLAYERS = 3;
        private List<Tuple<IPlayerGrain, PlayerInfo>> playersInLobby = new List<Tuple<IPlayerGrain, PlayerInfo>>();
        private readonly object _lock = new object();

        async Task ILobbyGrain.RegisterPlayer(IPlayerGrain player, PlayerInfo playerInfo)
        {
            List<Tuple<IPlayerGrain, PlayerInfo>> playerSet = null;

            lock (_lock) // do the collection mgmt operations in a locked section, do awaiting the game server spawn outside
            {
                this.playersInLobby.Add(new Tuple<IPlayerGrain, PlayerInfo>(player, playerInfo));
                if (this.playersInLobby.Count == MAX_PLAYERS)
                {
                    playerSet = this.playersInLobby;
                    this.playersInLobby = new List<Tuple<IPlayerGrain, PlayerInfo>>();
                }
            }

            if (playerSet != null)
            {
                var gameServer = GameServerGrainFactory.GetGrain(Guid.NewGuid());

                await gameServer.StartGame(playerSet.Select(_ => _.Item1).ToList());
            }
        }

        private async Task NonReentrantRegisterPlayer(IPlayerGrain player, PlayerInfo playerInfo)
        {
            this.playersInLobby.Add(new Tuple<IPlayerGrain, PlayerInfo>(player, playerInfo));
            if (this.playersInLobby.Count == MAX_PLAYERS)
            {
                var gameServer = GameServerGrainFactory.GetGrain(Guid.NewGuid());

                await gameServer.StartGame(this.playersInLobby.Select(_ => _.Item1).ToList()); 
                this.playersInLobby.Clear();
            }
        }
    }
}
