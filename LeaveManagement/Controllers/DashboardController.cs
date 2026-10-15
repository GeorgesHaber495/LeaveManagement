using System;
using System.Web.Mvc;
using LeaveManagement.Models;
using LeaveManagement.Services;
using LeaveManagement.ViewModels;
using Microsoft.AspNet.Identity;

namespace LeaveManagement.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LeaveRequestService service;

        public DashboardController()
        {
            service = new LeaveRequestService(db);
        }

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            bool isManager = User.IsInRole("Manager");

            var model = new DashboardViewModel();

            model.IsManager = isManager;
            model.StatusCounts = service.GetStatusCounts(isManager ? null : userId);
            model.Balances = service.GetBalancesForUser(userId, DateTime.Today.Year);
            model.Upcoming = service.GetUpcomingApproved(isManager ? null : userId,
                DateTime.Today, DateTime.Today.AddDays(30));

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}