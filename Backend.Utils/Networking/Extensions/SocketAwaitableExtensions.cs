namespace Backend.Utils.Networking.Extensions
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public static class SocketAwaitableExtensions
    {
        public static SocketAwaitable ReceiveAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ReceiveAsync(awaitable.m_eventArgs))
                awaitable.m_wasCompleted = true;
            return awaitable;
        }

        public static SocketAwaitable SendAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.SendAsync(awaitable.m_eventArgs))
                awaitable.m_wasCompleted = true;
            return awaitable;
        }

        //public static async Task ReadAsync(this Socket s, Func<byte[], int, Task> onReceived)
        //{
        //    // Reusable SocketAsyncEventArgs and awaitable wrapper 
        //    var args = new SocketAsyncEventArgs();
        //    args.SetBuffer(new byte[0x1000], 0, 0x1000);
        //    var awaitable = new SocketAwaitable(args);

        //    // Do processing, continually receiving from the socket 
        //    while (true)
        //    {
        //        await s.ReceiveAsync(awaitable);
        //        int bytesRead = args.BytesTransferred;
        //        if (bytesRead <= 0)
        //        {
        //            break;
        //        }

        //        await onReceived(args.Buffer, args.BytesTransferred);
        //    }
        //}

        public static async Task WriteValueAsync<T>(this Socket socket, T t, Func<T, byte[]> converter)
        {
            var buf = converter(t);

            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buf, 0, buf.Length);
            var awaitable = new SocketAwaitable(args);

            await socket.SendAsync(awaitable);
        }

        //public static async Task WriteAsync(this Socket socket, Int16 value)
        //{
        //    await WriteValueAsync(socket, value, BitConverter.GetBytes);
        //}

        public static async Task WriteAsync(this Socket socket, Int32 value)
        {
            await WriteValueAsync(socket, value, BitConverter.GetBytes);
        }
        public static async Task WriteAsync(this Socket socket, byte value)
        {
            await WriteValueAsync(socket, value, (x) => new byte[1] { x });
        }

        public static async Task<T> ReadValueAsync<T>(this Socket socket, int len, Func<byte[], T> converter)
        {
            var awaitable = SocketAwaitable.NewInstance(len);
            await socket.ReceiveAsync(awaitable);
            if (len == awaitable.m_eventArgs.BytesTransferred)
            {
                return converter(awaitable.m_eventArgs.Buffer);
            }
            throw new NotSupportedException(string.Format("Could not get {0}", typeof(T).Name));
        }

        //public static async Task<Int16> ReadInt16Async(this Socket socket)
        //{
        //    return await ReadValueAsync<Int16>(socket, sizeof(Int16), buf => BitConverter.ToInt16(buf, 0));
        //}

        public static async Task<Int32> ReadInt32Async(this Socket socket)
        {
            return await ReadValueAsync<Int32>(socket, sizeof(Int32), buf => BitConverter.ToInt32(buf, 0));
        }


        public static async Task<byte> ReadByteAsync(this Socket socket)
        {
            return await ReadValueAsync<byte>(socket, 1, buf => buf[0]);
        }
        //public static async Task<T> ReadAsync<T>(this Socket socket)
        //{
        //    if (typeof(T) == typeof(Int32))
        //    {
        //        var x = await ReadValueAsync<Int32>(socket, sizeof(Int32), buf => BitConverter.ToInt32(buf, 0));
        //        return x as T;
        //    }
        //    return default(T);
        //}
    }
}
