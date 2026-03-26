namespace ERDM.Core.Constants
{
    public static class MongoDbConstants
    {
        // Default Collection Names
        public const string UsersCollection = "users";
        public const string CreditApplicationsCollection = "credit_applications";
        public const string CreditScoresCollection = "credit_scores";
        public const string RiskAssessmentsCollection = "risk_assessments";
        public const string PaymentsCollection = "payments";
        public const string AuditLogsCollection = "audit_logs";

        // Default Values
        public const string DefaultDatabaseName = "credit_management";
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        // Index Types
        public const string IndexAscending = "ascending";
        public const string IndexDescending = "descending";
        public const string IndexUnique = "unique";
        public const string IndexSparse = "sparse";

        // Filter Operators
        public const string FilterEquals = "eq";
        public const string FilterNotEquals = "ne";
        public const string FilterGreaterThan = "gt";
        public const string FilterGreaterThanOrEqual = "gte";
        public const string FilterLessThan = "lt";
        public const string FilterLessThanOrEqual = "lte";
        public const string FilterIn = "in";
        public const string FilterNotIn = "nin";
    }
}