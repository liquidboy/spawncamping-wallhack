namespace Backend.GrainImplementations
{
    using Backend.GrainInterfaces;
    using System;
    using System.Threading.Tasks;
    using Backend.GrainInterfaces;

    public class Gamer
    {
        public Guid Id { get; private set; }
        public IPlayerRegistrationGrain PlayerGrain { get; private set; }
        public PlayerObserver PlayerObserver { get; private set; }

        private Gamer() { }

        public static async Task<Gamer> CreateAsync(Guid gamerId, Action<GameServerStartParams> onGameServerStarted)
        {
            if (onGameServerStarted == null) throw new ArgumentNullException("onGameServerStarted");

            var gamer = new Gamer { Id = gamerId };
            gamer.PlayerGrain = PlayerRegistrationGrainFactory.GetGrain(gamer.Id);
            gamer.PlayerObserver = await PlayerObserver.CreateAsync(onGameServerStarted);

            await gamer.PlayerObserver.Subscribe(gamer.PlayerGrain);

            return gamer;
        }
    }
}