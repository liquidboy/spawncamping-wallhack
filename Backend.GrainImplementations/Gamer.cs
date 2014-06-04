namespace Backend.GrainImplementations
{
    using Backend.GameLogic.Models;
    using Backend.GrainInterfaces;
    using Frontend.Library.Models;
    using System;
    using System.Threading.Tasks;

    public class Gamer
    {
        public ClientID Id { get; private set; }
        public IPlayerRegistrationGrain PlayerGrain { get; private set; }
        public PlayerObserver PlayerObserver { get; private set; }

        private Gamer() { }

        public static async Task<Gamer> CreateAsync(ClientID clientId, Action<GameServerStartParams> onGameServerStarted)
        {
            if (onGameServerStarted == null) throw new ArgumentNullException("onGameServerStarted");

            var gamer = new Gamer { Id = clientId };
            gamer.PlayerGrain = PlayerRegistrationGrainFactory.GetGrain(gamer.Id.ID);
            gamer.PlayerObserver = await PlayerObserver.CreateAsync(onGameServerStarted);

            await gamer.PlayerObserver.Subscribe(gamer.PlayerGrain);

            return gamer;
        }
    }
}