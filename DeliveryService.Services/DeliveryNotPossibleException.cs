using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Services;

public class DeliveryNotPossibleException : Exception
{
    public DeliveryNotPossibleException()
        : base("Ни один курьер не может перевезти такой заказ") { }
}
