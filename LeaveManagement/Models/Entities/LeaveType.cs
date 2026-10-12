using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Models.Entities
{
    public class LeaveType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int DefaultAnnualAllowance { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; }
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; }
    }
}