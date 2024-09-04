namespace API.DTOs
{
    public class FilterParamsDTO
    {
        public string? TextSearch {  get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public string? Sort {  get; set; }

    }
}
