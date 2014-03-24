namespace Frontend.GameLogic
{
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

    public class LobbyClientImpl
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpClient client;

        public LobbyClientImpl(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            client = new TcpClient();
        }

        public async Task ConnectAsync()
        {
            await client.ConnectAsync(this._ipAddress, this._port);
        }
    }
}
