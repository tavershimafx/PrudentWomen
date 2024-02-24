namespace Monochrome.Module.Core.Models
{
    public class SyncLogs: BaseModel
    {
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public int NumberOfRecords { get; set; }

        public SynchronizationStatus Status { get; set; }
    }
    public enum SynchronizationStatus
    {
        Pending = 0,
        
        Running = 1,

        Failed = 2,

        Completed
    }
}
