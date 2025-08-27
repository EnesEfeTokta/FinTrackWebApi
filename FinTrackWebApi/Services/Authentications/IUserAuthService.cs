using FinTrackWebApi.Dtos.AuthDtos;

namespace FinTrackWebApi.Services.Authentications
{
    public interface IUserAuthService
    {
        Task<(bool, string)> InitiateRegistration(UserInitiateRegistrationDto initiateDto);
        Task<(bool, string)> VerifyOtpAndRegister(VerifyOtpRequestDto verifyDto);
        void Login(LoginDto loginDto);
    }
}
