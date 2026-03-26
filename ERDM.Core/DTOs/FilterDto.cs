namespace ERDM.Core.DTOs
{
    public class FilterDto : PaginationDto
    {
        public string SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
    }
}
