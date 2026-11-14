using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Models.Roles
{
    public class SupportEmployee : SystemUser
    {
        protected SupportEmployee() { } 

        public SupportEmployee(string login, string passwordHash,
                               string firstName, string lastName, string? middleName)
            : this(login, passwordHash, firstName, lastName, middleName, UserRole.Support) { }

        protected SupportEmployee(string login, string passwordHash,
                                  string firstName, string lastName, string? middleName,
                                  UserRole role)
            : base(login, passwordHash, role)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
        }

        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string? MiddleName { get; private set; }
    }
}
