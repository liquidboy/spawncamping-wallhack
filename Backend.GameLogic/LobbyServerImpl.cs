namespace Backend.GameLogic
{
    using System.ComponentModel.Composition;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    [Export]
    public class LobbyServerImpl : IPartImportsSatisfiedNotification
    {
        public LobbyServerImpl() { }

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

        public async Task HandleClient(TcpClient tcpClient, CancellationToken ct)
        {
            var connection = new LobbyConnection(tcpClient);

            await connection.Handlerequest();
        }
    }
}