using System.Collections.Generic;

namespace LeaveManagement.Dtos
{
    public class PagedResultDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<LeaveRequestDto> Items { get; set; }
    }
}