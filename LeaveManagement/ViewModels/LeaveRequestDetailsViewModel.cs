using System.Collections.Generic;
using LeaveManagement.Models.Entities;

namespace LeaveManagement.ViewModels
{
    public class LeaveRequestDetailsViewModel
    {
        public LeaveRequest Request { get; set; }
        public List<LeaveStatusHistory> History { get; set; }
    }
}