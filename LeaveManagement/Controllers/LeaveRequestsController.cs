using LeaveManagement.Models;
using LeaveManagement.Services;
using LeaveManagement.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LeaveManagement.Controllers
{
    public class LeaveRequestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LeaveRequestService service;

        public LeaveRequestsController()
        {
            service = new LeaveRequestService(db);
        }

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var requests = service.GetRequestsForUser(userId);
            return View(requests);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var request = service.GetRequestForUser(id.Value, userId);

            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
        public ActionResult Create()
        {
            var model = new CreateLeaveRequestViewModel();
            model.StartDate = DateTime.Today;
            model.EndDate = DateTime.Today;
            model.LeaveTypes = BuildLeaveTypeList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateLeaveRequestViewModel model)
        {
            if (!service.DatesAreValid(model.StartDate, model.EndDate))
            {
                ModelState.AddModelError("EndDate", "The end date cannot be before the start date.");
            }

            if (!ModelState.IsValid)
            {
                model.LeaveTypes = BuildLeaveTypeList();
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            service.CreateRequest(userId, model.LeaveTypeId, model.StartDate,
                model.EndDate, model.Reason);

            return RedirectToAction("Index");
        }

        private IEnumerable<SelectListItem> BuildLeaveTypeList()
        {
            return service.GetActiveLeaveTypes()
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToList();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var userId = User.Identity.GetUserId();

            if (!service.CancelRequest(id, userId))
            {
                return HttpNotFound();
            }

            return RedirectToAction("Index");
        }
    }
}