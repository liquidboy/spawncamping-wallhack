namespace Backend.Utils.Networking.Extensions
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public static class NetworkStreamExtensions
    {
        public static async Task<int> ReadInt(this NetworkStream networkStream)
        {
            var buf = new byte[sizeof(int)];
            var len = await networkStream.ReadAsync(buf, 0, buf.Length);
            if (len != buf.Length) { throw new NotSupportedException("Missing content"); }
            return BitConverter.ToInt32(buf, 0);
        }

        public static async Task WriteInt(this NetworkStream networkStream, int value)
        {
            var buf = BitConverter.GetBytes(value);

            await networkStream.WriteAsync(buf, 0, buf.Length);
        }
        
        //public static async Task<string> ReadString(this NetworkStream networkStream)
        //{
        //    var length = await networkStream.ReadLong();
        //    var buf = new byte[length];
        //    var len = await networkStream.ReadAsync(buf, 0, buf.Length);
        //    if (len != length) { throw new NotSupportedException("Missing content"); }
        //    return Encoding.UTF8.GetString(buf);
        //}

        //public static async Task WriteString(this NetworkStream networkStream, string value)
        //{
        //    var buf = Encoding.UTF8.GetBytes(value);
        //    await networkStream.WriteLong(buf.Length);
        //    await networkStream.WriteAsync(buf, 0, buf.Length);
        //}

        //public static async Task<long> ReadLong(this NetworkStream networkStream)
        //{
        //    var buf = new byte[sizeof(long)];
        //    var len = await networkStream.ReadAsync(buf, 0, buf.Length);
        //    if (len != buf.Length) { throw new NotSupportedException("Missing content"); }
        //    return BitConverter.ToInt64(buf, 0);
        //}

        //public static async Task WriteLong(this NetworkStream networkStream, long value)
        //{
        //    var buf = BitConverter.GetBytes(value);
        //    await networkStream.WriteAsync(buf, 0, buf.Length);
        //}
    }
}
