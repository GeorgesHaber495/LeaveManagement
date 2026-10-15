using System.Collections.Generic;
using LeaveManagement.Models.Entities;
using LeaveManagement.Services;

namespace LeaveManagement.ViewModels
{
    public class DashboardViewModel
    {
        public bool IsManager { get; set; }
        public List<StatusCount> StatusCounts { get; set; }
        public List<LeaveBalance> Balances { get; set; }
        public List<LeaveRequest> Upcoming { get; set; }

        public int CountFor(LeaveStatus status)
        {
            foreach (var item in StatusCounts)
            {
                if (item.Status == status)
                {
                    return item.Count;
                }
            }

            return 0;
        }
    }
}