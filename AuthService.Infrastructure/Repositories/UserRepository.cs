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
    }
}