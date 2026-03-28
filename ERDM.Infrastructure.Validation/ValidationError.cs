namespace ERDM.Infrastructure.Validation
{
    public class ValidationError
    {
        public string Property { get; set; }
        public string Message { get; set; }
        public object AttemptedValue { get; set; }
    }
}
