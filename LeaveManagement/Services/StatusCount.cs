using LeaveManagement.Models.Entities;

namespace LeaveManagement.Services
{
    public class StatusCount
    {
        public LeaveStatus Status { get; set; }
        public int Count { get; set; }
    }
}