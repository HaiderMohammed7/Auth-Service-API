using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetByUserName(string userName);
        User? GetByEmailOrUserName(string email, string userName);
        User? GetByLoginIdentifier(string Identifier);
        User? GetById(int userId);

        void IncrementFailedAttempts(int userID);
        void ResetFailedAttempts(int userID);
        void LockUser(int userID);
        void UpdateLastLogin(int userID);
        List<string> GetUserRoles(int userID);
        void Add(User user);
        void Delete(int userId);
        void AssignRole(int userID, string roleName);
        void UpdatePassword(int userId, byte[] hash, byte[] salt);
        void Save();

        void AddResetToken(PasswordResetToken token);
        PasswordResetToken? GetValidResetToken(byte[] tokenHash);
        void MarkResetTokenUsed(int Id);

        Task<List<User>> GetUsersByIdsAsync(IEnumerable<int> userIds);

        Task<User?> GetByIdAsync(int userId);
    }
}