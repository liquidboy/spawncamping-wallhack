namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Backend.Utils.Networking.Extensions;
    using System.ComponentModel.Composition;

    [Export(typeof(LobbyServerImpl))]
    public class LobbyServerImpl : IPartImportsSatisfiedNotification
    {
        public LobbyServerImpl() { }

        [Import]
        public LobbyConnector LobbyConnector { get; set; }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() 
        { 
            InitAsync().Wait(); 
        }

        public async Task InitAsync()
        {
            await this.LobbyConnector.EnsureSetupAsync();
        }

        public async Task HandleClient(TcpClient tcpClient, CancellationToken ct)
        {
            var connection = new LobbyConnection(tcpClient);

            await connection.Handlerequest();
        }
    }

   
}
