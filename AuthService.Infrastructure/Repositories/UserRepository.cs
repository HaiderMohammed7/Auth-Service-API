using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public User? GetByEmail(string email)
          => _context.Users.SingleOrDefault(u => u.Email == email);

        public User? GetByUserName(string userName)
            => _context.Users.SingleOrDefault(u =>u.UserName == userName);

        public User? GetByLoginIdentifier(string Identifier)
            => _context.Users.SingleOrDefault(u => u.Email == Identifier || u.UserName == Identifier);

        public User? GetByEmailOrUserName(string email, string userName)
            => _context.Users.SingleOrDefault(u => u.Email == email || u.UserName == userName);

        public User? GetById(int userId)
            => _context.Users.Find(userId);

        public List<string> GetUserRoles(int userId)
            => _context.UserRoles.Where(ur => ur.UserID == userId)
            .Join(_context.Roles, ur => ur.RoleID, r => r.RoleID, (ur, r) => r.RoleName).ToList();

        public void IncrementFailedAttempts(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.FailedLoginAttempts++;
            _context.SaveChanges();
        }

        public void ResetFailedAttempts(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.FailedLoginAttempts = 0;
            _context.SaveChanges();
        }

        public void LockUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.IsLocked = true;
            _context.SaveChanges();
        }

        public void UpdateLastLogin(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.LastLoginAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void AssignRole(int userId, string roleName)
        {
            var role = _context.Roles.Single(r => r.RoleName == roleName);

            _context.UserRoles.Add(new UserRole
            {
                UserID = userId,
                RoleID = role.RoleID,
            });
            _context.SaveChanges();
        }

        public void UpdatePassword(int userId, byte[] hash, byte[] salt)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }
    }
}