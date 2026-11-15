using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace DeliveryService.Domain.Models.Roles
{
    public class Customer : SystemUser
    {
        private Customer () { }
        public Customer(string login, string passwordHash,
            string companyName, string address, string phoneNumber) : base(login, passwordHash, UserRole.Customer)
        {
            Name = companyName;
            PickupAddress = address;
            ContactPhone = phoneNumber;
        }
        public string Name { get; private set; } = null!;
        public string ContactPhone { get; private set; }
        public string PickupAddress { get; private set; }
        public string? ApiKeyHash { get; private set; }
        public bool IsKeyIssued => ApiKeyHash != null;

        public void SetApiKeyHash(string hash) => ApiKeyHash = hash;
    }
}
