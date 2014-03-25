namespace Backend.GameLogic
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Backend.Utils.Networking.Extensions;
    using Messages;

    public class LobbyClientImpl
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpClient client;

        public Func<string, Task> Logger { private get; set; }

        private async Task LogAsync(string format, params object[] args)
        {
            if (this.Logger != null)
            {
                await Logger(string.Format(format, args));
            }
        }

        public LobbyClientImpl(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            client = new TcpClient();
        }

        public async Task ConnectAsync()
        {
            await client.ConnectAsync(this._ipAddress, this._port);

            await LogAsync("connected to lobby");
        }

        public async Task<GameServerConnectionMessage> JoinLobbyAsync(int clientId)
        {
            var server = client.Client;

            await LogAsync("try to join to lobby");

            await server.WriteCommandAsync(new JoinGameMessage { ClientId = clientId });

            await LogAsync("sent join");


            var gameServerConnectionResponse = await server.ReadCommandAsync<GameServerConnectionMessage>();

            if (gameServerConnectionResponse.IsError) { throw new NotSupportedException("Could not join"); }
            var gameServerConnection = gameServerConnectionResponse.Message;

            return gameServerConnection;
        }
    }
}