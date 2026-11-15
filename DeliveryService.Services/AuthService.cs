using DeliveryService.Domain.Models.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services
{
    public class AuthService
    {
        private readonly DeliveryDbContext _db;
        public AuthService(DeliveryDbContext db) => _db = db;

        public async Task<SystemUser?> ValidateAsync(string login, string password)
        {
            var l = login.Trim().ToLower();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == l);
            if (user is null || !user.IsActive) return null;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }
    }
}
