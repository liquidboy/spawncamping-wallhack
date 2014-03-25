namespace Backend.GameLogic
{
    using Messages;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    
    public class LobbyConnection
    {
        private readonly TcpClient tcpClient;

        public LobbyConnection(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }

        public async Task Handlerequest()
        {
            Socket client = tcpClient.Client;

            var joinMessage = await client.ReadCommandAsync<JoinGameMessage>();

            if (joinMessage.ClientId > 100)
            {
                await client.WriteCommandAsync(new ErrorMessage("Sorry, not permitted"));

                return;
            }

            Console.WriteLine("Connect from client {0}", joinMessage.ClientId);

            await client.WriteCommandAsync(new GameServerConnectionMessage { 
                GameServer = new IPEndPoint(IPAddress.Loopback, 3001),
                Token = new GameServerUserToken { Credential = "supersecret" }});
        }
    }
}
