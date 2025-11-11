using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Profile Picture")]
        public string ProfilePicture { get; set; }

        [Display(Name = "Preferred Delivery Address")]
        public string PreferredDeliveryAddress { get; set; }

        [Display(Name = "Delivery Instructions")]
        [StringLength(500, ErrorMessage = "Delivery instructions cannot exceed 500 characters")]
        public string DeliveryInstructions { get; set; }

        // Helper properties
        public bool IsProfileComplete =>
            !string.IsNullOrEmpty(Phone) &&
            !string.IsNullOrEmpty(Address) &&
            !string.IsNullOrEmpty(City);

        public int ProfileCompletionPercentage
        {
            get
            {
                var fields = new[] { Phone, Address, City, PostalCode };
                var completedFields = fields.Count(f => !string.IsNullOrEmpty(f));
                if (DateOfBirth.HasValue) completedFields++;
                return (completedFields * 100) / 5; // 5 total optional fields
            }
        }
    }
}