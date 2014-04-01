namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Net;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceBus.Messaging;
    using Backend.Utils;
    using Messages;
    using System.Collections.Concurrent;

    [Export]
    public class LobbyServerImpl : IPartImportsSatisfiedNotification, ITcpServerHandler, IDisposable
    {
        [Import(typeof(LobbyServiceBackplane), RequiredCreationPolicy = CreationPolicy.Shared)]
        public LobbyServiceBackplane LobbyConnector { get; set; }

        [Import(typeof(ILobbyServerDatabase))]
        public ILobbyServerDatabase LobbyServerDatabase { get; set; }

        public LobbyServerImpl() { /* put constructor code into IPartImportsSatisfiedNotification.OnImportsSatisfied */ }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() 
        {
            // Read current state from azure storage table 
            this.LobbyServerDatabase.LoadAsync().Wait();

            this.LobbyConnector.ObservableBackPlane.Subscribe(msg => 
            {
                    Trace.TraceInformation(string.Format("Received msg: {0}", msg.Content));
            });
            this.LobbyConnector.ObservableBackPlane.Subscribe(msg =>
            {
                var clientId = (int) msg["clientId"];

                this.CurrentPlayers.Add(clientId);
            });
        }

        public readonly ConcurrentBag<int> CurrentPlayers = new ConcurrentBag<int>();

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

                Func<JoinGameMessage, BrokeredMessage> createJoinNotification = _ =>
                {
                    var joinMessageUpdate = new BrokeredMessage(string.Format("Connect from {0} on instance {1}",
                        _.ClientID.ID, this.LobbyConnector.Settings.LobbyServiceInstanceId));
                    joinMessageUpdate.Properties.Add("clientId", _.ClientID.ID);
                    return joinMessageUpdate;
                };

                await this.LobbyConnector.BroadcastLobbyMessageAsync(createJoinNotification(joinMessage));

                var msgFromFour = await this.LobbyConnector.ObservableBackPlane.FirstAsync(msg => 
                { 
                    var clientIdO = msg["clientId"];
                    return ((int)clientIdO) == 1; 
                });

                Trace.TraceInformation("Received msg from clientId {0}", msgFromFour["clientId"]);

                await Task.Delay(TimeSpan.FromSeconds(4));

                //if (joinMessage.ClientId > 10000)
                //{
                //    await client.WriteCommandAsync(new ErrorMessage("Sorry, not permitted"));

                //    return;
                //}

                await client.WriteCommandAsync(new GameServerConnectionMessage
                {
                    GameServer = new IPEndPoint(IPAddress.Loopback, 4000),
                    Token = new GameServerUserToken { Credential = "supersecret" }
                });

                // Trace.TraceInformation("Request from " + tcpClient.Client.RemoteEndPoint.ToString());
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
                this.LobbyConnector.Dispose();
            }

            // Free any unmanaged objects here. 
            //
            m_disposed = true;
        }

        #endregion
    }
}