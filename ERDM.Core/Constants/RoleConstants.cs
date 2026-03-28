namespace ERDM.Core.Constants
{
    public static class RoleConstants
    {
        // System Roles
        public const string Admin = "Admin";
        public const string System = "System";

        // Business Roles
        public const string CreditOfficer = "CreditOfficer";
        public const string Underwriter = "Underwriter";
        public const string FraudAnalyst = "FraudAnalyst";
        public const string ComplianceOfficer = "ComplianceOfficer";

        // Customer Roles
        public const string Customer = "Customer";
        public const string PremiumCustomer = "PremiumCustomer";

        // Support Roles
        public const string SupportAgent = "SupportAgent";
        public const string Auditor = "Auditor";

        public static bool IsAdmin(string role) => role == Admin;
        public static bool IsUnderwriter(string role) => role == Underwriter;
        public static bool IsCreditOfficer(string role) => role == CreditOfficer;

        public static string[] AllRoles => new[]
        {
            Admin, System, CreditOfficer, Underwriter,
            FraudAnalyst, ComplianceOfficer, Customer,
            PremiumCustomer, SupportAgent, Auditor
        };
    }
}
