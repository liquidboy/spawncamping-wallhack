namespace Backend.GrainImplementations
{
    using Backend.GrainInterfaces;
    using Frontend.Library.Models;
    using Orleans;
    using System;
    using System.Threading.Tasks;

    [StorageProvider(ProviderName = "PlayerState")]
    public class PlayerGrain : GrainBase<IPlayerGrainState>, IPlayerGrain
    {
        // The PlayerInGameGrain allows me to management of the Observable to the side. 

        [NonSerialized]
        private ObserverSubscriptionManager<IPlayerObserver> subscribers = 
            new ObserverSubscriptionManager<IPlayerObserver>();

        public override async Task ActivateAsync()
        {
            var playerId = this.GetPrimaryKey().ToString();

            if (this.State.PlayerInfo == null)
            {
                this.State.PlayerInfo = new PlayerInfo { PlayerName = playerId, Strength = 1 };
                await this.State.WriteStateAsync();
            }
        }

        public override Task DeactivateAsync()
        {
            return base.DeactivateAsync();
        }

        Task<PlayerInfo> IPlayerGrain.Subscribe(IPlayerObserver playerObserver)
        {
            var playerId = this.GetPrimaryKey().ToString();

            if (this.subscribers.Count != 0)
            {
                throw new NotSupportedException("Only one subscription allowed");
            }

            this.subscribers.Subscribe(playerObserver);

            return Task.FromResult<PlayerInfo>(this.State.PlayerInfo);
        }

        async Task IPlayerGrain.GameServerStarts(GameServerStartParams gameServerStartParams)
        {
            this.State.CurrentGame = gameServerStartParams;

            await this.State.WriteStateAsync();

            this.subscribers.Notify(_ => _.GameServerStarts(gameServerStartParams));
        }

        async Task IPlayerGrain.GameEnded()
        {
            this.State.CurrentGame = null;

            await this.State.WriteStateAsync();
        }

        Task<GameServerStartParams> IPlayerGrain.GetCurrentGame()
        {
            return Task.FromResult(this.State.CurrentGame);
        }
    }
}
