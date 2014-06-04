namespace Backend.GrainImplementations
{
    using GameLogic.Models;
    using GrainInterfaces;
    using Frontend.Library.Models;
    using System;
    using System.Threading.Tasks;

    public class Gamer
    {
        public ClientID Id { get; private set; }
        public IPlayerRegistrationGrain PlayerGrain { get; private set; }
        public PlayerObserver PlayerObserver { get; private set; }

        private Gamer() { }

        private static async Task<Gamer> CreateAsync(ClientID clientId, Action<GameServerStartParams> onGameServerStarted)
        {
            if (onGameServerStarted == null) throw new ArgumentNullException("onGameServerStarted");

            var gamer = new Gamer { Id = clientId };
            gamer.PlayerGrain = PlayerRegistrationGrainFactory.GetGrain(gamer.Id.ID);
            gamer.PlayerObserver = await PlayerObserver.CreateAsync(onGameServerStarted);

            await gamer.PlayerObserver.Subscribe(gamer.PlayerGrain);

            return gamer;
        }

        public static async Task<GameServerStartParams> GetGameServerAsync(ClientID clientId)
        {
            GameServerStartParams startParams = null;
            var gamer = await Gamer.CreateAsync(clientId, server => { startParams = server; });

            while (startParams == null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            return startParams;
        }
    }
}