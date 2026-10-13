using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeaveManagement.Services;

namespace LeaveManagement.Tests
{
    [TestClass]
    public class LeaveRequestServiceTests
    {
        [TestMethod]
        public void CountDays_SingleDayRequestIsOneDay()
        {
            var day = new DateTime(2026, 10, 20);

            Assert.AreEqual(1, LeaveRequestService.CountDays(day, day));
        }

        [TestMethod]
        public void CountDays_CountsBothEndDates()
        {
            var start = new DateTime(2026, 10, 22);
            var end = new DateTime(2026, 10, 26);

            Assert.AreEqual(5, LeaveRequestService.CountDays(start, end));
        }

        [TestMethod]
        public void CountDays_CountsAcrossMonthEnd()
        {
            var start = new DateTime(2026, 10, 30);
            var end = new DateTime(2026, 11, 2);

            Assert.AreEqual(4, LeaveRequestService.CountDays(start, end));
        }
    }
}