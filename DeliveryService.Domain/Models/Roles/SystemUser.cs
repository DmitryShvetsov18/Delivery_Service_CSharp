using DeliveryService.Domain.Models.Statuses;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Models.Roles
{
    abstract public class SystemUser
    {
        protected SystemUser() { }

        protected SystemUser(string login, string passwordHash, UserRole role)
        {
            Login = login;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
        }

        public long Id { get; private set; }
        public string Login { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}
