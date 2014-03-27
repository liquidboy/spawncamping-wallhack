namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Backend.Utils;
    using Messages;

    [Export]
    public class LobbyServerImpl : IPartImportsSatisfiedNotification, ITcpServerHandler, IDisposable
    {
        [Import(typeof(LobbyServiceBackplane), RequiredCreationPolicy = CreationPolicy.NonShared)]
        public LobbyServiceBackplane LobbyConnector { get; set; }

        public LobbyServerImpl() { /* put constructor code into IPartImportsSatisfiedNotification.OnImportsSatisfied */ }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() {  }

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