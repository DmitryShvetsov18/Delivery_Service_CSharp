using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Models.Roles
{
    public class Admin : SupportEmployee
    {
        private Admin() { } 

        public Admin(string login, string passwordHash,
                     string firstName, string lastName, string? middleName)
            : base(login, passwordHash, firstName, lastName, middleName, UserRole.Admin) { }
    }
}
