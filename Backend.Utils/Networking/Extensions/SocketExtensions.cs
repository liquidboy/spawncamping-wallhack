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

        public static Task<int> ReadAsync(
            this Socket socket, byte[] buffer, int offset, int size,
            SocketFlags socketFlags = SocketFlags.None)
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
    
        public static Task<int> WriteAsync(
            this Socket socket, byte[] buffer, int offset, int size,
            SocketFlags socketFlags = SocketFlags.None)
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

        #region string

        private static readonly Encoding DEFAULT_ENCODING = Encoding.UTF8;

        public static async Task<string> ReadStringAsync(this Socket socket)
        {
            var length = await socket.ReadInt32Async();
            return await socket.ReadValueAsync<string>(length, _ => DEFAULT_ENCODING.GetString(_));
        }

        public static async Task WriteAsync(this Socket socket, string value)
        {
            var bytes = DEFAULT_ENCODING.GetBytes(value);

            await socket.WriteAsync(bytes.Length);

            var bytesWritten = await socket.WriteAsync(bytes, 0, bytes.Length);
        }

        #endregion

        #region Int32

        public static async Task<Int32> ReadInt32Async(this Socket socket)
        {
            return await socket.ReadValueAsync<Int32>(sizeof(Int32), _ => BitConverter.ToInt32(_, 0));
        }

        public static async Task WriteAsync(this Socket socket, Int32 value)
        {
            var buf = BitConverter.GetBytes(value);
            await socket.WriteAsync(buf, 0, sizeof(Int32));
        }

        #endregion

        #region byte

        public static async Task<byte> ReadByteAsync(this Socket socket)
        {
            return await socket.ReadValueAsync<byte>(sizeof(byte), _ => _[0]);
        }

        public static async Task WriteAsync(this Socket socket, byte value)
        {
            await socket.WriteAsync(new byte[1]{value} , 0, 1);
        }

        #endregion

        #region utils

        private static async Task<T> ReadValueAsync<T>(this Socket socket, int length, Func<byte[], T> convertAsync)
        {
            return await ReadValueAsync(socket, length, async (_) => convertAsync(_));
        }
        
        private static async Task<T> ReadValueAsync<T>(this Socket socket, int length, Func<byte[], Task<T>> convertAsync)
        {
            byte[] buf = new byte[length];
            var bytesRead = await socket.ReadAsync(buf, 0, buf.Length);
            if (bytesRead != buf.Length) throw new Exception("Missing bytes");
            return await convertAsync(buf);
        }

        #endregion
    }
}