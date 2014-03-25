namespace Backend.GameLogic
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Messages;
    
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

            var joinMessageResponse = await client.ReadCommandOrErrorAsync<JoinGameMessage>();
            if (joinMessageResponse.IsError) 
            {
                await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, was expecting a {0}", typeof(JoinGameMessage).Name)));
                return;
            }
            var joinMessage = joinMessageResponse.Message;

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
