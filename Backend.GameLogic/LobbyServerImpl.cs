namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using Messages;
    using Backend.Utils;

    [Export]
    public class LobbyServerImpl : IPartImportsSatisfiedNotification, ITcpServerHandler
    {
        public LobbyServerImpl() { /* put constructor code into IPartImportsSatisfiedNotification.OnImportsSatisfied */ }

        [Import(typeof(LobbyServiceBackplane))]
        public LobbyServiceBackplane LobbyConnector { get; set; }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() 
        { 
            InitAsync().Wait(); 
        }

        public async Task InitAsync()
        {
            await this.LobbyConnector.EnsureSetupAsync();
        }

        public async Task ShutDownAsync()
        {
            await this.LobbyConnector.DetachAsync();
        }

        public async Task HandleRequest(TcpClient tcpClient, CancellationToken ct)
        {
            try
            {
                Socket client = tcpClient.Client;

                var joinMessageResponse = await client.ReadCommandOrErrorAsync<JoinGameMessage>();
                if (joinMessageResponse.IsError)
                {
                    await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, was expecting a {0}", typeof(JoinGameMessage).Name)));
                    return;
                }
                var joinMessage = joinMessageResponse.Message;

                await Task.Delay(TimeSpan.FromSeconds(4));

                //if (joinMessage.ClientId > 10000)
                //{
                //    await client.WriteCommandAsync(new ErrorMessage("Sorry, not permitted"));

                //    return;
                //}

                await client.WriteCommandAsync(new GameServerConnectionMessage
                {
                    GameServer = new IPEndPoint(IPAddress.Loopback, 3001),
                    Token = new GameServerUserToken { Credential = "supersecret" }
                });

                // Trace.TraceInformation("Request from " + tcpClient.Client.RemoteEndPoint.ToString());
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
        }
    }
}