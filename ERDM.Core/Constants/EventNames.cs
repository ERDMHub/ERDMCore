namespace ERDM.Core.Constants
{
    public static class EventNames
    {
        // Credit Application Events
        public const string CreditApplicationCreated = "credit.application.created";
        public const string CreditApplicationSubmitted = "credit.application.submitted";
        public const string CreditApplicationUnderReview = "credit.application.under.review";
        public const string CreditApplicationApproved = "credit.application.approved";
        public const string CreditApplicationDeclined = "credit.application.declined";
        public const string CreditApplicationReferred = "credit.application.referred";

        // Customer Events
        public const string CustomerCreated = "customer.created";
        public const string CustomerVerified = "customer.verified";
        public const string CustomerUpdated = "customer.updated";

        // Risk Events
        public const string RiskAssessmentCompleted = "risk.assessment.completed";
        public const string FraudAlertTriggered = "fraud.alert.triggered";

        // Document Events
        public const string DocumentUploaded = "document.uploaded";
        public const string DocumentVerified = "document.verified";

        // Payment Events
        public const string PaymentReceived = "payment.received";
        public const string PaymentFailed = "payment.failed";
        public const string PaymentDueReminder = "payment.due.reminder";
    }
}
