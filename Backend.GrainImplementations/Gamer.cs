namespace Backend.GrainImplementations
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Frontend.Library.Models;
    using Backend.GameLogic.Models;

    public class Gamer
    {
        private TaskCompletionSource<GameServerStartParams> _tcs = new TaskCompletionSource<GameServerStartParams>();
        private PlayerObserver _playerObserver;
        private IPlayerRegistrationGrain _playerGrain;

        private Gamer() { }

        public static async Task<Gamer> ConstructorAsync(ClientID clientId)
        {
            var gamer = new Gamer();

            gamer._playerGrain = PlayerRegistrationGrainFactory.GetGrain(clientId.ID);
            gamer._playerObserver = await PlayerObserver.CreateAsync(val => gamer._tcs.TrySetResult(val));

            return gamer;
        }

        public async Task<GameServerStartParams> GetAsync()
        {
            await this._playerObserver.Subscribe(this._playerGrain);
            return await _tcs.Task;
        }
    }
}