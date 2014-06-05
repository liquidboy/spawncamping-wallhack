namespace Backend.GrainImplementations
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Frontend.Library.Models;
    using Backend.GameLogic.Models;
    using System;

    public class Gamer
    {
        private TaskCompletionSource<GameServerStartParams> _tcs = new TaskCompletionSource<GameServerStartParams>();
        private PlayerObserver _playerObserver;
        private IPlayerRegistrationGrain _playerGrain;

        private Gamer() { }

        //public static async Task<Gamer> ConstructorAsync(ClientID clientId)
        //{
        //    var gamer = new Gamer();
        //    gamer._playerGrain = PlayerRegistrationGrainFactory.GetGrain(clientId.ID);
        //    gamer._playerObserver = await PlayerObserver.CreateAsync(val => gamer._tcs.TrySetResult(val));
        //    return gamer;
        //}
        //public async Task<GameServerStartParams> GetAsync()
        //{
        //    await this._playerObserver.Subscribe(this._playerGrain);
        //    return await _tcs.Task;
        //}


        //private static async Task<GameServerStartParams> GetGameServer_DOES_NOT_WORK_Async(ClientID clientId)
        //{
        //    // This function does not work:
        //    //
        //    // Object associated with Grain ID *cliObj/d2a24dbb has been garbage collected. 
        //    // Deleting object reference and unregistering it. Backend.GrainInterfaces.IPlayerObserver:GameServerStarts()	
        //    var taskCompletionSource = new TaskCompletionSource<GameServerStartParams>();
        //    var grain = PlayerRegistrationGrainFactory.GetGrain(clientId.ID);
        //    var observer = await PlayerObserver.CreateAsync(val => taskCompletionSource.TrySetResult(val));
        //    await observer.Subscribe(grain);
        //    return await taskCompletionSource.Task;
        //}


        private static async Task<Gamer> ConstructorAsync(ClientID clientId, Action<GameServerStartParams> onGameServerStarted)
        {
            if (onGameServerStarted == null) throw new ArgumentNullException("onGameServerStarted");

            var gamer = new Gamer();
            gamer._playerGrain = PlayerRegistrationGrainFactory.GetGrain(clientId.ID);
            gamer._playerObserver = await PlayerObserver.CreateAsync(onGameServerStarted);

            await gamer._playerObserver.Subscribe(gamer._playerGrain);

            return gamer;
        }

        public static async Task<GameServerStartParams> GetAsync(ClientID clientId)
        {
            GameServerStartParams startParams = null;
            var gamer = await Gamer.ConstructorAsync(clientId, server => { startParams = server; });

            while (startParams == null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            return startParams;
        }
    }
}