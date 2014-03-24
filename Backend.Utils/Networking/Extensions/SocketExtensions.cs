namespace Backend.Utils.Networking.Extensions
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class SocketExtensions
    {
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
    }
}