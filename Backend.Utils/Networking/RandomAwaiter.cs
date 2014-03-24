namespace Backend.Utils.Networking
{
    using System;
    using System.Threading.Tasks;

    public static class RandomAwaiter
    {
        static readonly Random random = new Random();

        /// <summary>
        /// Randomly delays the the Task within the range of 0 to maxDelay. 
        /// </summary>
        /// <param name="maxDelay"></param>
        /// <returns></returns>
        public static async Task DelayAsync(TimeSpan maxDelay)
        {
            await Task.Delay(random.Next(0,
                (int)maxDelay.TotalMilliseconds));
        }

        private static Func<TimeSpan, Task> CreateAwaiter()
        {
            Func<Func<TimeSpan, Task>> createTimespanGenerator = () =>
            {
                var random = new Random();
                return async (maxDelay) =>
                {
                    await Task.Delay(random.Next(0,
                        (int)maxDelay.TotalMilliseconds));
                };
            };
            return createTimespanGenerator();
        }
    }
}