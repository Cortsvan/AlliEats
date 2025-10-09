using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models
{
    public partial class MenuItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}