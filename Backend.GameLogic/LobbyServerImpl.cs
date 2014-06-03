namespace Backend.GameLogic
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Backend.Utils;
    using Messages;
    using Security;

    [Export(typeof(LobbyServerImpl))]
    public class LobbyServerImpl : ITcpServerHandler, IPartImportsSatisfiedNotification, IDisposable
    {
        [Import(typeof(ILobbyServerDatabase))]
        public ILobbyServerDatabase LobbyServerDatabase { get; set; }

        [Import(typeof(SymmetricKeyGenerator))]
        public SymmetricKeyGenerator GameAuthenticationHandler { get; set; }

        private PlayerAuthenticator _playerAuthenticator;

        public LobbyServerImpl() { /* put constructor code into IPartImportsSatisfiedNotification.OnImportsSatisfied */ }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() 
        {
            // Read current state from azure storage table 
            this.LobbyServerDatabase.LoadAsync().Wait();

            _playerAuthenticator = this.GameAuthenticationHandler.CreateAuthenticator();
        }

        public readonly ConcurrentBag<int> CurrentPlayers = new ConcurrentBag<int>();

        public async Task HandleRequest(TcpClient tcpClient, CancellationToken ct)
        {
            try
            {
                Socket client = tcpClient.Client;

                var loginToLobbyRequestOrError = await client.ReadCommandOrErrorAsync<LoginToLobbyRequestMessage>();
                if (loginToLobbyRequestOrError.IsError)
                {
                    await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, was expecting a {0}", typeof(LoginToLobbyRequestMessage).Name)));
                    return;
                }
                var loginToLobbyRequest = loginToLobbyRequestOrError.Message;
                if (!CompletelyInsecureLobbyAuthentication.AuthenticatePlayer(loginToLobbyRequest.ClientID, loginToLobbyRequest.Password))
                {
                    await client.WriteCommandAsync(new ErrorMessage("Unauthenticated"));
                    return;
                }
                var clientId = loginToLobbyRequest.ClientID;



                //Func<LoginToLobbyRequestMessage, BrokeredMessage> createJoinNotification = _ =>
                //{
                //    var joinMessageUpdate = new BrokeredMessage(string.Format("Connect from {0} on instance {1}",
                //        _.ClientID.ID, this.LobbyConnector.BackplaneSettings.InstanceId));
                //    joinMessageUpdate.Properties.Add("clientId", _.ClientID.ID);
                //    return joinMessageUpdate;
                //};

                //await this.LobbyConnector.BroadcastLobbyMessageAsync(createJoinNotification(loginToLobbyRequest));

                //var msgFromFour = await this.LobbyConnector.ObservableBackPlane.FirstAsync(msg => 
                //{ 
                //    var clientIdO = msg["clientId"];
                //    return ((int)clientIdO) == 1; 
                //});

                //Trace.TraceInformation("Received msg from clientId {0}", msgFromFour["clientId"]);

                var gameserverId = "gameserver123";
                var innerGameServerPort = 4002;
                var usertoken = this._playerAuthenticator.CreatePlayerToken(clientId, gameserverId);

                await client.WriteCommandAsync(new LoginToLobbyResponseMessage(
                    new IPEndPoint(IPAddress.Loopback, 4000), innerGameServerPort, usertoken));
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
        }

        #region IDisposable

        // Flag: Has Dispose already been called? 
        bool m_disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (disposing)
            {
                // this.LobbyConnector.Dispose();
            }

            // Free any unmanaged objects here. 
            //
            m_disposed = true;
        }

        #endregion
    }
}