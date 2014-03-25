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

    public class GameServerMessage
    {
        public string Command { get; set; }

        public IList<string> Args { get; set; }

        public GameServerMessage(string command, params string[] args)
        {
            this.Command = command;
            this.Args = new List<string>(args);
        }

        public static GameServerMessage Error(string message)
        {
            return new GameServerMessage("Error", message);
        }

        public async static Task<GameServerMessage> ReadAsync(Socket socket)
        {
            int len = await socket.ReadInt32Async();

            var args = new string[len-1];
            string command = await socket.ReceiveStringAsync();
            for (var i = 0; i < len - 1; i++)
            {
                string arg = await socket.ReceiveStringAsync();
                args[i] = arg;
            }

            return new GameServerMessage(command, args);
        }

        public async Task WriteAsync(Socket socket)
        {
            await socket.WriteAsync(this.Args.Count + 1);
            await socket.SendAsync(this.Command);
            foreach (var arg in Args)
            {
                await socket.SendAsync(arg);
            }
        }
    }
}
