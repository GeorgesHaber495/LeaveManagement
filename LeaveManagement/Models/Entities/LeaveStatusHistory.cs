using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Models.Entities
{
    public class LeaveStatusHistory
    {
        public int Id { get; set; }

        public int LeaveRequestId { get; set; }

        public LeaveStatus? OldStatus { get; set; }

        public LeaveStatus NewStatus { get; set; }

        [Required]
        public string ChangedByUserId { get; set; }

        public DateTime ChangedDate { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }

        public virtual LeaveRequest LeaveRequest { get; set; }
        public virtual ApplicationUser ChangedByUser { get; set; }
    }
}