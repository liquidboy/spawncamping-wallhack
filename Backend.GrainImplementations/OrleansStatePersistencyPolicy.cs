namespace Backend.GrainImplementations
{
    using System;
    using System.Threading.Tasks;

    public class OrleansStatePersistencyPolicy
    {
        public static OrleansStatePersistencyPolicy Every(TimeSpan interval)
        {
            return new OrleansStatePersistencyPolicy(interval);
        }

        public OrleansStatePersistencyPolicy(TimeSpan interval)
        {
            this.Interval = interval;
        }

        private TimeSpan Interval { get; set; }

        private DateTimeOffset Last { get; set; }

        private bool ShouldPersist { get { return DateTimeOffset.UtcNow > this.Last.Add(this.Interval); } }

        public async Task PersistIfNeeded(Func<Task> persist)
        {
            if (ShouldPersist)
            {
                await persist();
                this.Last = DateTimeOffset.UtcNow;
            }
        }
    }
}
