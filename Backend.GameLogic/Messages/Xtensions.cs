namespace Backend.GameLogic.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using Backend.Utils.Networking.Extensions;

    public static class Xtensions
    {
        public async static Task<T> ReadExpectedCommandAsync<T>(this Socket socket) where T : GameServerMessageBase, new()
        {
            var x = await socket.ReadCommandOrErrorAsync<T>();
            if (x.IsError) { throw new Exception(x.ErrorMessage.Message); }
            return x.Message;
        }


        public async static Task<GameServerMessage<T>> ReadCommandOrErrorAsync<T>(this Socket socket) where T : GameServerMessageBase, new()
        {
            string command;
            string[] args;

            try
            {
                int len = await socket.ReadInt32Async();
                command = await socket.ReadStringAsync();
                args = new string[len - 1];
                for (var i = 0; i < len - 1; i++)
                {
                    string arg = await socket.ReadStringAsync();
                    args[i] = arg;
                }
                if (command == typeof(ErrorMessage).Name)
                {
                    return new GameServerMessage<T>(new ErrorMessage { Command = command, Args = args, Message = args[0] });
                }

                T message = Activator.CreateInstance<T>();
                message.Command = command;
                message.Args = args.ToList();
                message.PostRead();

                return new GameServerMessage<T>(message);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not read {0} command", typeof(T).Name), ex );
            }
        }

        public static async Task WriteCommandAsync(this Socket socket, GameServerMessageBase message)
        {
            message.PreWrite();

            await socket.WriteAsync(message.Args.Count + 1);
            await socket.WriteAsync(message.Command);
            foreach (var arg in message.Args)
            {
                await socket.WriteAsync(arg);
            }
        }
    }
}