using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LeaveManagement.Models.Entities;

namespace LeaveManagement.ViewModels
{
    public class ReviewLeaveRequestViewModel
    {
        public LeaveRequest Request { get; set; }
        public LeaveBalance Balance { get; set; }
        public List<LeaveStatusHistory> History { get; set; }

        [StringLength(500)]
        [Display(Name = "Comment")]
        public string ManagerComment { get; set; }
    }
}