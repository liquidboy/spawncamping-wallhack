namespace Backend.Utils
{
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITcpServerHandler
    {
        // This interface is just a replacement for the functional approach
        // 
        //    Func<TcpClient, CancellationToken, Task> serverLogic
        // 
        // 
        Task HandleRequest(TcpClient tcpClient, CancellationToken cancellationToken); 
    }
}