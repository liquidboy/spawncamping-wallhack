namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using Backend.Utils.Networking.Extensions;
    using Backend.GameLogic.Messages;

    public static class GameServerProtocolExtensions
    {


        public async static Task<T> ReadCommandAsync<T>(this Socket socket) where T : GameServerMessage, new()
        {
            int len = await socket.ReadInt32Async();

            var args = new string[len - 1];
            string command = await socket.ReceiveStringAsync();
            for (var i = 0; i < len - 1; i++)
            {
                string arg = await socket.ReceiveStringAsync();
                args[i] = arg;
            }

            T message = Activator.CreateInstance<T>();
            message.Command = command;
            message.Args = args.ToList();
            message.PostRead();

            return message;
        }

        public static async Task WriteCommandAsync(this Socket socket, GameServerMessage message)
        {
            message.PreWrite();

            await socket.WriteAsync(message.Args.Count + 1);
            await socket.SendAsync(message.Command);
            foreach (var arg in message.Args)
            {
                await socket.SendAsync(arg);
            }
        }
    }
}