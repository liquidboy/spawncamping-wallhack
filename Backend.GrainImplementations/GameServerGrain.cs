namespace Backend.GrainImplementations
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Backend.GrainInterfaces;
    using Orleans;
    using Frontend.Library.Models;
    using System;

    [StorageProvider(ProviderName = "GameServerState")]
    public class GameServerGrain : GrainBase<IGameServerGrainState>, IGameServerGrain
    {
        async Task IGameServerGrain.StartGame(List<IPlayerGrain> playersToLaunch)
        {
            Trace.TraceInformation("Launching game server");

            this.State.Players = playersToLaunch.Select(_ => _.GetPrimaryKey().ToString()).ToList();

            await this.State.WriteStateAsync();

            var gameServerParams = await LaunchGameServerProcessAsync();

            var tasks = playersToLaunch
                .Select(_ => _.GameServerStarts(gameServerParams))
                .ToList();

            await Task.WhenAll(tasks);
        }

        async Task<GameServerStartParams> LaunchGameServerProcessAsync()
        {
            // start game server process here
            var gameServerParams = new GameServerStartParams
            {
                GameServerID = new GameServerID { ID = this.GetPrimaryKey() }
            };

            gameServerParams.GameServerID.ID = Guid.Parse("9c02e618-bc51-4c41-ae8d-f6d0d2a913c8");             // TODO DELETE THIS LINE


            Trace.TraceInformation(string.Format("Create game server process for GameID {0}", gameServerParams.GameServerID));
 
            return gameServerParams;
        }
    }
}