using System;
using System.Collections.Generic;
using System.Web.Mvc;
using LeaveManagement.Models.Entities;

namespace LeaveManagement.ViewModels
{
    public class LeaveRequestSearchViewModel
    {
        public string Search { get; set; }
        public LeaveStatus? Status { get; set; }
        public int? LeaveTypeId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public List<LeaveRequest> Results { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<SelectListItem> LeaveTypes { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0)
                {
                    return 1;
                }

                return (int)Math.Ceiling((double)TotalCount / PageSize);
            }
        }
    }
}