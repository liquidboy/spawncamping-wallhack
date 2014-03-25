namespace Backend.Utils.Networking.Extensions
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public static class SocketExtensions
    {
        #region byte[] buffer

        public static Task<int> ReceiveAsync(
            this Socket socket, byte[] buffer, int offset, int size,
            SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginReceive(buffer, offset, size, socketFlags, iar =>
            {
                var t = (TaskCompletionSource<int>)iar.AsyncState;
                var s = (Socket)t.Task.AsyncState;
                try { t.TrySetResult(s.EndReceive(iar)); }
                catch (Exception exc) { t.TrySetException(exc); }
            }, tcs);
            return tcs.Task;
        }
    
        public static Task<int> SendAsync(
            this Socket socket, byte[] buffer, int offset, int size,
            SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginSend(buffer, offset, size, socketFlags, iar =>
            {
                var t = (TaskCompletionSource<int>)iar.AsyncState;
                var s = (Socket)t.Task.AsyncState;
                try 
                {
                    t.TrySetResult(s.EndReceive(iar)); 
                }
                catch (Exception exc) 
                { 
                    t.TrySetException(exc);
                }
            }, tcs);
            return tcs.Task;
        }

        #endregion

        private static readonly Encoding DEFAULT_ENCODING = Encoding.UTF8;

        #region string

        public static async Task<string> ReceiveStringAsync(this Socket socket)
        {
            var length = await socket.ReadInt32Async();

            return await socket.ReadValueAsync<string>(length, DEFAULT_ENCODING.GetString);
        }

        public static async Task SendAsync(this Socket socket, string value)
        {
            var bytes = DEFAULT_ENCODING.GetBytes(value);

            await socket.WriteAsync(bytes.Length);

            var bytesWritten = await socket.SendAsync(bytes, 0, bytes.Length, SocketFlags.None);
        }

        #endregion
    }
}