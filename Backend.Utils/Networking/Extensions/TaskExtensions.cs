namespace Backend.Utils.Networking.Extensions
{
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static bool isFinished(this Task task)
        {
            var t = task.Status;
            return t == TaskStatus.Canceled ||
                t == TaskStatus.Faulted ||
                t == TaskStatus.RanToCompletion;
        }
    }
}
