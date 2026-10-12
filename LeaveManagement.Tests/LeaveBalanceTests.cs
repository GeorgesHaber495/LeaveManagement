using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeaveManagement.Models.Entities;

namespace LeaveManagement.Tests
{
    [TestClass]
    public class LeaveBalanceTests
    {
        [TestMethod]
        public void RemainingDays_SubtractsUsedFromAllocated()
        {
            var balance = new LeaveBalance();
            balance.AllocatedDays = 15;
            balance.UsedDays = 4;

            Assert.AreEqual(11, balance.RemainingDays);
        }

        [TestMethod]
        public void RemainingDays_IsZeroWhenEverythingIsUsed()
        {
            var balance = new LeaveBalance();
            balance.AllocatedDays = 15;
            balance.UsedDays = 15;

            Assert.AreEqual(0, balance.RemainingDays);
        }
    }
}