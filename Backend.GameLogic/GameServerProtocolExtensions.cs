namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public static class GameServerProtocolExtensions
    {
        public static async Task SendErrorAsync(this Socket socket, string message)
        {
            await GameServerMessage.Error(message).WriteAsync(socket);
        }

        public static async Task SendCommand(this Socket socket, GameServerMessage gameServerMessage)
        {
            await gameServerMessage.WriteAsync(socket);
        }

        public static async Task<GameServerMessage> ReadCommand(this Socket socket)
        {
            return await GameServerMessage.ReadAsync(socket);
        }
    }
}