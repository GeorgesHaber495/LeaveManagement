using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.Models.Entities
{
    public class LeaveBalance
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public int LeaveTypeId { get; set; }

        public int Year { get; set; }

        public int AllocatedDays { get; set; }

        public int UsedDays { get; set; }

        [NotMapped]
        public int RemainingDays
        {
            get { return AllocatedDays - UsedDays; }
        }

        public virtual ApplicationUser User { get; set; }
        public virtual LeaveType LeaveType { get; set; }
    }
}