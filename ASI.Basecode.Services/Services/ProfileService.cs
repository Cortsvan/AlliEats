using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ProfileService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public ProfileViewModel GetProfile(string userId)
        {
            var user = _userRepository.GetUserById(userId); // Use the new method
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            return _mapper.Map<ProfileViewModel>(user);
        }

        public void UpdateProfile(ProfileViewModel profileModel)
        {
            var user = _userRepository.GetUserById(profileModel.UserId); // Use the new method
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Update user with profile data
            user.Name = profileModel.Name;
            user.Phone = profileModel.Phone;
            user.Address = profileModel.Address;
            user.City = profileModel.City;
            user.PostalCode = profileModel.PostalCode;
            user.DateOfBirth = profileModel.DateOfBirth;
            user.ProfilePicture = profileModel.ProfilePicture;
            user.PreferredDeliveryAddress = profileModel.PreferredDeliveryAddress;
            user.DeliveryInstructions = profileModel.DeliveryInstructions;
            user.UpdatedTime = DateTime.Now;
            user.UpdatedBy = profileModel.UserId;

            _userRepository.UpdateUser(user);
        }

        public bool IsProfileComplete(string userId)
        {
            var profile = GetProfile(userId);
            return profile.IsProfileComplete;
        }

        public int GetProfileCompletionPercentage(string userId)
        {
            var profile = GetProfile(userId);
            return profile.ProfileCompletionPercentage;
        }
    }
}