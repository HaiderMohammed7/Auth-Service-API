using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        void AssignRole(int userID, string roleName);
    }
}