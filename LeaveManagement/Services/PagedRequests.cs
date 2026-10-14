using System.Collections.Generic;
using LeaveManagement.Models.Entities;

namespace LeaveManagement.Services
{
    public class PagedRequests
    {
        public List<LeaveRequest> Items { get; set; }
        public int TotalCount { get; set; }
    }
}