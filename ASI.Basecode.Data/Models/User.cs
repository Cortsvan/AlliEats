using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Email
        public string Name { get; set; } // Full Name
        public string Password { get; set; }
        public string Role { get; set; } = "User";

        // Profile Information (nullable - can be completed later)
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ProfilePicture { get; set; } // File path or URL

        // Delivery Preferences
        public string PreferredDeliveryAddress { get; set; }
        public string DeliveryInstructions { get; set; }

        // Email OTP Verification fields
        public bool IsEmailVerified { get; set; } = false;
        public string EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        // System fields
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}