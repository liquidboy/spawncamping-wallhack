namespace Backend.GrainImplementations
{
    using Backend.GrainInterfaces;
    using Frontend.Library.Models;
    using System;
    using System.Threading.Tasks;

    public class PlayerObserver : IPlayerObserver
    {
        private IPlayerObserver reference; 

        private Action<GameServerStartParams> OnGameServerStarted;

        public static async Task<PlayerObserver> CreateAsync(Action<GameServerStartParams> onGameServerStarted)
        {
            if (onGameServerStarted == null) throw new ArgumentNullException("onGameServerStarted");

            var result = new PlayerObserver
            { 
                OnGameServerStarted = onGameServerStarted 
            };

            result.reference = await PlayerObserverFactory.CreateObjectReference(result);

            return result;
        }

        void IPlayerObserver.GameServerStarts(GameServerStartParams gameServerStartParams)
        {
            this.OnGameServerStarted(gameServerStartParams);
        }

        public Task Subscribe(IPlayerRegistrationGrain grain)
        {
            return grain.Register(this.reference);
        }
    }
}