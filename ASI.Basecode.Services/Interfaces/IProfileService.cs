using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IProfileService
    {
        ProfileViewModel GetProfile(string userId);
        void UpdateProfile(ProfileViewModel profileModel);
        bool IsProfileComplete(string userId);
        int GetProfileCompletionPercentage(string userId);
    }
}