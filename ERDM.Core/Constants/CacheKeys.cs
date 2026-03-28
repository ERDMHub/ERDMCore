namespace ERDM.Core.Constants
{
    public static class CacheKeys
    {
        private const string Prefix = "ERDM:";

        public static string Customer(string customerId) => $"{Prefix}customer:{customerId}";
        public static string CreditApplication(string applicationId) => $"{Prefix}application:{applicationId}";
        public static string CreditScore(string customerId) => $"{Prefix}creditscore:{customerId}";
        public static string RiskAssessment(string applicationId) => $"{Prefix}risk:{applicationId}";

        public static string AllApplications(int page, int pageSize) => $"{Prefix}applications:all:{page}:{pageSize}";
        public static string ApplicationsByStatus(string status, int page, int pageSize) => $"{Prefix}applications:status:{status}:{page}:{pageSize}";

        public static string UserPermissions(string userId) => $"{Prefix}permissions:{userId}";
        public static string UserRoles(string userId) => $"{Prefix}roles:{userId}";

        public static string LookupData(string type) => $"{Prefix}lookup:{type}";
        public static string Configuration(string key) => $"{Prefix}config:{key}";

        public static string RateLimit(string ipAddress) => $"{Prefix}ratelimit:{ipAddress}";

        public static string Session(string sessionId) => $"{Prefix}session:{sessionId}";
    }
}
