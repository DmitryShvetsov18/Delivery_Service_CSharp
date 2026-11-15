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
        public CourierStatus Status { get; private set; }
        public decimal BodyLengthCm { get; private set; }
        public decimal BodyHeightCm { get; private set; }
        public decimal BodyWidthCm { get; private set; }
        public decimal BodyVolume => (BodyLengthCm * BodyHeightCm * BodyWidthCm) / 1_000_000m;
        public decimal CarryingCapacityKg { get; private set; }
        public int DeliveriesToday { get; private set; }

        public void StartShift()
        {
            if (Status != CourierStatus.OffShift)
                throw new InvalidOperationException("Смена уже начата");
            Status = CourierStatus.Free;
        }

        public void EndShift()
        {
            if (Status == CourierStatus.Busy)
                throw new InvalidOperationException("Нельзя завершить смену с заказом в доставке");
            Status = CourierStatus.OffShift;
        }

        public void TakeOrder()
        {
            if (Status != CourierStatus.Free)
                throw new InvalidOperationException("Заказ получает только свободный курьер");
            Status = CourierStatus.Busy;
        }

        public void FinishDelivery()
        {
            if (Status != CourierStatus.Busy)
                throw new InvalidOperationException("У курьера нет заказа в доставке");
            Status = CourierStatus.Free;
            DeliveriesToday++;
        }

        public void ReleaseFromOrder()
        {
            if (Status != CourierStatus.Busy)
                throw new InvalidOperationException("У курьера нет заказа в доставке");
            Status = CourierStatus.Free;
        }

        public void UpdateVehicle(decimal lengthCm, decimal widthCm, decimal heightCm, decimal capacityKg)
        {
            if (Status == CourierStatus.Busy)
                throw new InvalidOperationException("Нельзя менять ТС, пока курьер занят");
            if (lengthCm <= 0 || widthCm <= 0 || heightCm <= 0 || capacityKg <= 0)
                throw new ArgumentException("Параметры ТС должны быть больше 0");
            BodyLengthCm = lengthCm;
            BodyWidthCm = widthCm;
            BodyHeightCm = heightCm;
            CarryingCapacityKg = capacityKg;
        }
    }
}
