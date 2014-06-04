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

    using Utils;
    using Messages;
    using Security;
    using GrainImplementations;
    using Backend.GrainInterfaces;
    using Frontend.Library.Models;

    [Export(typeof(LobbyServerImpl))]
    public class LobbyServerImpl : ITcpServerHandler, IPartImportsSatisfiedNotification
    {
        [Import(typeof(SymmetricKeyGenerator))]
        public SymmetricKeyGenerator GameAuthenticationHandler { get; set; }

        private PlayerAuthenticator _playerAuthenticator;

        public LobbyServerImpl() { /* put constructor code into IPartImportsSatisfiedNotification.OnImportsSatisfied */ }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() 
        {
            _playerAuthenticator = this.GameAuthenticationHandler.CreateAuthenticator();
        }

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

                GameServerStartParams startParams = null;
                var gamer = await Gamer.CreateAsync(clientId, server => { startParams = server; });

                while (startParams == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }

                var gameserverId = startParams.GameServerID;
                var innerGameServerPort = 4002;
                var usertoken = this._playerAuthenticator.CreatePlayerToken(clientId, startParams.GameServerID);

                await client.WriteCommandAsync(new LoginToLobbyResponseMessage(
                    new IPEndPoint(IPAddress.Loopback, 4000), innerGameServerPort, usertoken, startParams.GameServerID));
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
        }
    }
}