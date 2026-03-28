
namespace ERDM.Core.Constants
{
    public static class ErrorCodes
    {
        public const string InternalServerError = "ERR-1000";
        public const string ValidationError = "ERR-1001";
        public const string NotFound = "ERR-1002";
        public const string Unauthorized = "ERR-1003";
        public const string Forbidden = "ERR-1004";
        public const string Conflict = "ERR-1005";

        // Business Logic Errors (2000-2999)
        public const string InvalidApplicationStatus = "ERR-2000";
        public const string InsufficientCreditScore = "ERR-2001";
        public const string ExceedsCreditLimit = "ERR-2002";
        public const string DuplicateApplication = "ERR-2003";
        public const string ApplicationExpired = "ERR-2004";

        // Database Errors (3000-3999)
        public const string DatabaseConnectionFailed = "ERR-3000";
        public const string DuplicateKey = "ERR-3001";
        public const string ConcurrencyError = "ERR-3002";

        // External Service Errors (4000-4999)
        public const string CreditBureauUnavailable = "ERR-4000";
        public const string FraudCheckFailed = "ERR-4001";
        public const string DocumentVerificationFailed = "ERR-4002";
    }
}
