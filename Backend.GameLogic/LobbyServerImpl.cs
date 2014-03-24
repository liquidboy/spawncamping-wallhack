﻿namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Backend.Utils.Networking.Extensions;

    public class LobbyServerImpl
    {
        public async Task Server(TcpClient serverSocket, CancellationToken ct)
        {
            Socket socket = serverSocket.Client;

            var clientId = await socket.ReadInt32Async();
            
        }
    }
}
