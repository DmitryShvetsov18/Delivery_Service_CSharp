using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Domain.Services;

namespace DeliveryService.Tests.Services;

[TestFixture]
public class OrderStateRulesTests
{
    [TestCase(OrderStatus.Created, true, false, true, true, true, false, false)]
    [TestCase(OrderStatus.InDelivery, false, true, false, true, true, true, false)]
    [TestCase(OrderStatus.Delivered, false, false, false, false, false, false, true)]
    [TestCase(OrderStatus.Cancelled, false, false, false, false, false, false, true)]
    public void Rules_MatchPlan(OrderStatus s, bool assign, bool complete, bool custCancel,
        bool supCancel, bool change, bool back, bool final)
    {
        Assert.Multiple(() =>
        {
            Assert.That(OrderStateRules.CanAssign(s), Is.EqualTo(assign));
            Assert.That(OrderStateRules.CanComplete(s), Is.EqualTo(complete));
            Assert.That(OrderStateRules.CanCancelByCustomer(s), Is.EqualTo(custCancel));
            Assert.That(OrderStateRules.CanCancelBySupport(s), Is.EqualTo(supCancel));
            Assert.That(OrderStateRules.CanChangeDeliveryData(s), Is.EqualTo(change));
            Assert.That(OrderStateRules.CanReturnToQueue(s), Is.EqualTo(back));
            Assert.That(OrderStateRules.IsFinal(s), Is.EqualTo(final));
        });
    }
}