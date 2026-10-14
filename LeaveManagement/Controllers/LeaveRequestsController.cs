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

        public ActionResult Index(LeaveRequestSearchViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            if (model.PageSize < 1)
            {
                model.PageSize = 10;
            }

            var userId = User.Identity.GetUserId();

            var result = service.SearchRequests(userId, model.Search, model.Status,
                model.LeaveTypeId, model.From, model.To, model.Page, model.PageSize);

            model.Results = result.Items;
            model.TotalCount = result.TotalCount;
            model.LeaveTypes = BuildLeaveTypeList();

            return View(model);
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


            var model = new LeaveRequestDetailsViewModel
            {
                Request = request,
                History = service.GetHistoryForRequest(request.Id)
            };

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
            var userId = User.Identity.GetUserId();

            if (!service.DatesAreValid(model.StartDate, model.EndDate))
            {
                ModelState.AddModelError("EndDate", "The end date cannot be before the start date.");
            }
            else
            {
                if (service.HasOverlap(userId, model.StartDate, model.EndDate))
                {
                    ModelState.AddModelError("StartDate",
                        "You already have a request that covers these dates.");
                }

                int totalDays = LeaveRequestService.CountDays(model.StartDate, model.EndDate);

                if (!service.HasSufficientBalance(userId, model.LeaveTypeId, model.StartDate, totalDays))
                {
                    ModelState.AddModelError("LeaveTypeId",
                        "You do not have enough days left for this leave type.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.LeaveTypes = BuildLeaveTypeList();
                return View(model);
            }

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
        public ActionResult Balances()
        {
            var userId = User.Identity.GetUserId();
            var balances = service.GetBalancesForUser(userId, DateTime.Today.Year);

            ViewBag.Year = DateTime.Today.Year;

            return View(balances);
        }
    }
}