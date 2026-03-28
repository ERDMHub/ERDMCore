namespace ERDM.Infrastructure.Configuration
{
    public class HangfireSettings
    {
        public string ConnectionString { get; set; }
        public int WorkerCount { get; set; } = 5;
        public bool EnableDashboard { get; set; } = true;
        public string DashboardPath { get; set; } = "/hangfire";
    }
}
