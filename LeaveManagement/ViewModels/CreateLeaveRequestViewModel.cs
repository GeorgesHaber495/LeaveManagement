using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LeaveManagement.ViewModels
{
    public class CreateLeaveRequestViewModel
    {
        [Required]
        [Display(Name = "Leave type")]
        public int LeaveTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public IEnumerable<SelectListItem> LeaveTypes { get; set; }
    }
}