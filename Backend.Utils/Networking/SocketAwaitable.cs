namespace Backend.Utils.Networking
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class SocketAwaitable : INotifyCompletion
    {
        private readonly static Action SENTINEL = () => { };

        internal bool m_wasCompleted;
        internal Action m_continuation;
        public readonly SocketAsyncEventArgs m_eventArgs;

        public static SocketAwaitable NewInstance(int buflen)
        {
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[buflen], 0, buflen);
            return new SocketAwaitable(args);
        }

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException("eventArgs");
            m_eventArgs = eventArgs;
            eventArgs.Completed += delegate
            {
                Action prev = m_continuation ?? Interlocked.CompareExchange(ref m_continuation, SENTINEL, null);
                if (prev != null)
                {
                    prev();
                }
            };
        }

        internal void Reset()
        {
            m_wasCompleted = false;
            m_continuation = null;
        }

        public SocketAwaitable GetAwaiter() { return this; }

        public bool IsCompleted { get { return m_wasCompleted; } }

        public void OnCompleted(Action continuation)
        {
            if (m_continuation == SENTINEL ||
                Interlocked.CompareExchange(
                    ref m_continuation, continuation, null) == SENTINEL)
            {
                Task.Run(continuation);
            }
        }

        public void GetResult()
        {
            if (m_eventArgs.SocketError != SocketError.Success)
            {
                throw new SocketException((int)m_eventArgs.SocketError);
            }
        }
    }
}
