using System;
using System.Linq;
using LeaveManagement.Models.Entities;
using LeaveManagement.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeaveManagement.Tests
{
    [TestClass]
    public class ServiceIntegrationTests
    {
        private TestDb data;
        private LeaveRequestService service;
        private LeaveReviewService reviewService;
        private int year;
        private DateTime start;

        [TestInitialize]
        public void Setup()
        {
            data = new TestDb();
            service = new LeaveRequestService(data.Db);
            reviewService = new LeaveReviewService(data.Db);

            year = DateTime.Today.Year;
            start = new DateTime(year, 6, 1);
        }

        [TestCleanup]
        public void Cleanup()
        {
            data.Dispose();
        }

        [TestMethod]
        public void CreateRequest_SavesRequestAsPendingAndWritesHistory()
        {
            var request = service.CreateRequest(data.EmployeeId, data.AnnualTypeId,
                start, start.AddDays(2), "Family trip");

            Assert.IsTrue(request.Id > 0);
            Assert.AreEqual(LeaveStatus.Pending, request.Status);
            Assert.AreEqual(3, request.TotalDays);

            var history = service.GetHistoryForRequest(request.Id);
            Assert.AreEqual(1, history.Count);
            Assert.AreEqual(LeaveStatus.Pending, history[0].NewStatus);
            Assert.IsNull(history[0].OldStatus);
        }

        [TestMethod]
        public void DatesAreValid_ReturnsFalseWhenEndIsBeforeStart()
        {
            Assert.IsFalse(service.DatesAreValid(start, start.AddDays(-1)));
            Assert.IsTrue(service.DatesAreValid(start, start));
        }

        [TestMethod]
        public void HasOverlap_ReturnsTrueWhenDatesTouchAnExistingRequest()
        {
            service.CreateRequest(data.EmployeeId, data.AnnualTypeId,
                start, start.AddDays(4), "First request");

            Assert.IsTrue(service.HasOverlap(data.EmployeeId, start.AddDays(2), start.AddDays(6)));
            Assert.IsFalse(service.HasOverlap(data.EmployeeId, start.AddDays(10), start.AddDays(12)));
        }

        [TestMethod]
        public void Decide_ReturnsInsufficientBalanceWhenEmployeeLacksDays()
        {
            var request = service.CreateRequest(data.EmployeeId, data.SickTypeId,
                start, start.AddDays(4), "Five sick days");

            var result = reviewService.Decide(request.Id, data.ManagerId, true, "Approved");

            Assert.AreEqual(DecisionResult.InsufficientBalance, result);

            var stored = data.Db.LeaveRequests.First(r => r.Id == request.Id);
            Assert.AreEqual(LeaveStatus.Pending, stored.Status);

            var balance = service.GetBalance(data.EmployeeId, data.SickTypeId, year);
            Assert.AreEqual(0, balance.UsedDays);
        }

        [TestMethod]
        public void Decide_ApprovingUpdatesBalanceAndWritesHistory()
        {
            var request = service.CreateRequest(data.EmployeeId, data.AnnualTypeId,
                start, start.AddDays(2), "Family trip");

            var result = reviewService.Decide(request.Id, data.ManagerId, true, "Enjoy");

            Assert.AreEqual(DecisionResult.Success, result);

            var stored = data.Db.LeaveRequests.First(r => r.Id == request.Id);
            Assert.AreEqual(LeaveStatus.Approved, stored.Status);
            Assert.AreEqual(data.ManagerId, stored.ReviewedByUserId);

            var balance = service.GetBalance(data.EmployeeId, data.AnnualTypeId, year);
            Assert.AreEqual(3, balance.UsedDays);
            Assert.AreEqual(17, balance.RemainingDays);

            var history = service.GetHistoryForRequest(request.Id);
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(LeaveStatus.Approved, history[1].NewStatus);
        }

        [TestMethod]
        public void Decide_RefusesWhenManagerIsTheRequester()
        {
            var request = service.CreateRequest(data.ManagerId, data.AnnualTypeId,
                start, start.AddDays(1), "My own leave");

            var result = reviewService.Decide(request.Id, data.ManagerId, true, "Approving myself");

            Assert.AreEqual(DecisionResult.CannotReviewOwnRequest, result);

            var stored = data.Db.LeaveRequests.First(r => r.Id == request.Id);
            Assert.AreEqual(LeaveStatus.Pending, stored.Status);
        }

        [TestMethod]
        public void GetRequestForUser_ReturnsNullForAnotherEmployeesRequest()
        {
            var request = service.CreateRequest(data.EmployeeId, data.AnnualTypeId,
                start, start.AddDays(1), "Private");

            Assert.IsNotNull(service.GetRequestForUser(request.Id, data.EmployeeId));
            Assert.IsNull(service.GetRequestForUser(request.Id, data.OtherEmployeeId));
        }
    }
}