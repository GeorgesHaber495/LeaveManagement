using LeaveManagement.Models;
using LeaveManagement.Services;
using LeaveManagement.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
namespace LeaveManagement.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LeaveReviewService service;
        private LeaveRequestService requestService;

        public ManagerController()
        {
            service = new LeaveReviewService(db);
            requestService = new LeaveRequestService(db);
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

            var result = requestService.SearchRequests(null, model.Search, model.Status,
                model.LeaveTypeId, model.From, model.To, model.Page, model.PageSize);

            model.Results = result.Items;
            model.TotalCount = result.TotalCount;
            model.LeaveTypes = BuildLeaveTypeList();

            return View(model);
        }

        private IEnumerable<SelectListItem> BuildLeaveTypeList()
        {
            return requestService.GetActiveLeaveTypes()
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var request = service.GetRequest(id.Value);

            if (request == null)
            {
                return HttpNotFound();
            }

            var model = new ReviewLeaveRequestViewModel
            {
                Request = request,
                Balance = service.GetBalance(request.UserId, request.LeaveTypeId, request.StartDate.Year),
                History = service.GetHistory(request.Id)
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Decide(int id, bool approve, string managerComment)
        {
            var managerId = User.Identity.GetUserId();
            var result = service.Decide(id, managerId, approve, managerComment);

            if (result == DecisionResult.NotFound)
            {
                return HttpNotFound();
            }

            if (result == DecisionResult.NotPending)
            {
                TempData["Message"] = "This request has already been decided.";
            }
            else if (result == DecisionResult.InsufficientBalance)
            {
                TempData["Message"] = "The employee does not have enough days left for this leave type.";
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}