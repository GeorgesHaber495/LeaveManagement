using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Models.Entities
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public int LeaveTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public LeaveStatus Status { get; set; }

        [StringLength(500)]
        public string ManagerComment { get; set; }

        public string ReviewedByUserId { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationUser ReviewedByUser { get; set; }
        public virtual LeaveType LeaveType { get; set; }
        public virtual ICollection<LeaveStatusHistory> StatusHistory { get; set; }
    }
}