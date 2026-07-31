using EventReservationAPI.Entities;

namespace EventReservationAPI.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterUserAsync(UserRegisterDto dto);
        Task<string?> LoginUserAsync(UserLoginDto dto);
    }
}
