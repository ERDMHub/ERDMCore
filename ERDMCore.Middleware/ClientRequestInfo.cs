
namespace ERDMCore.Middleware
{
    public class ClientRequestInfo
    {
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
        public int RequestCount { get; set; }
    }
}
