using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class MenuItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}