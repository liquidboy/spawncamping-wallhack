namespace Backend.GrainImplementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Orleans;
    using Backend.GrainInterfaces;

    public class PlayerRegistrationGrain : GrainBase, IPlayerRegistrationGrain
    {
        ILobbyGrain _lobby = LobbyGrainFactory.GetGrain(0);

        async Task IPlayerRegistrationGrain.Register(IPlayerObserver playerObserver)
        {
            var playerId = this.GetPrimaryKey();

            // first have someone else to wait for future notifications from the GameServerGrain
            IPlayerGrain playerInGame = PlayerGrainFactory.GetGrain(playerId);

            // Forward subscription to the corresponding PlayerInGameGrain and trigger lobby service. 
            var playerInfo = await playerInGame.Subscribe(playerObserver);

            // *then* ask to start a GameServerGrain via lobby
            await _lobby.RegisterPlayer(playerInGame, playerInfo);
        }
    }
}