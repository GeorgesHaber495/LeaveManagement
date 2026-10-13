using LeaveManagement.Models;
using LeaveManagement.Services;
using LeaveManagement.ViewModels;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
namespace LeaveManagement.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LeaveReviewService service;

        public ManagerController()
        {
            service = new LeaveReviewService(db);
        }

        public ActionResult Index(bool pendingOnly = false)
        {
            var requests = service.GetAllRequests(pendingOnly);
            ViewBag.PendingOnly = pendingOnly;

            return View(requests);
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

            if (!service.Decide(id, managerId, approve, managerComment))
            {
                return HttpNotFound();
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}