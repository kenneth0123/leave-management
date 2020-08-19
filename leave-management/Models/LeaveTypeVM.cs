using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveTypeVM
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(1, 365, ErrorMessage = "Please enter a value from 1 to 365")]
        [Display(Name = "Default Number Of Days")]
        public int Days { get; set; }
        [Display(Name="Date Created")]
        public DateTime? DateCreated { get; set; }
    }

}
