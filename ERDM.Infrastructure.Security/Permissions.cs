namespace ERDM.Infrastructure.Security
{
    public static class Permissions
    {
        public const string ViewApplications = "applications.view";
        public const string CreateApplications = "applications.create";
        public const string UpdateApplications = "applications.update";
        public const string ApproveApplications = "applications.approve";
        public const string DeclineApplications = "applications.decline";

        public const string ViewCustomers = "customers.view";
        public const string CreateCustomers = "customers.create";
        public const string UpdateCustomers = "customers.update";
        public const string DeleteCustomers = "customers.delete";

        public const string ViewReports = "reports.view";
        public const string ExportReports = "reports.export";
        public const string ViewAuditLogs = "audit.view";
    }
}
