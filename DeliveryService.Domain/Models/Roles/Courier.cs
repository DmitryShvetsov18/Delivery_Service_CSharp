using DeliveryService.Domain.Models.Statuses;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace DeliveryService.Domain.Models.Roles
{
    public class Courier : SystemUser
    {
        private Courier () { }
        public Courier(string login, string passwordHash,
            string firstName, string lastName, string? middleName, string contactPhone, decimal bodyLengthCm, decimal bodyWidthCm, decimal bodyHeightCm, decimal carryingCapacityKg) : base(login, passwordHash, UserRole.Courier)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            ContactPhone = contactPhone;
            Status = CourierStatus.OffShift;
            BodyLengthCm = bodyLengthCm;
            BodyWidthCm = bodyWidthCm;
            BodyHeightCm = bodyHeightCm;
            CarryingCapacityKg = carryingCapacityKg;
        }

        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string? MiddleName { get; private set; }
        public string ContactPhone { get; set; } = null!;
        public CourierStatus Status { get; set; }
        public decimal BodyLengthCm { get; set; }
        public decimal BodyHeightCm { get; set; }
        public decimal BodyWidthCm { get; set; }
        public decimal BodyVolume => (BodyLengthCm * BodyHeightCm * BodyWidthCm) / 1_000_000m;
        public decimal CarryingCapacityKg { get; set; }
        public int DeliveriesToday { get; set; }
    }
}
