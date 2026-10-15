using System;
using System.Linq;
using System.Web.Http;
using LeaveManagement.Dtos;
using LeaveManagement.Models;
using LeaveManagement.Models.Entities;
using LeaveManagement.Services;

namespace LeaveManagement.Controllers.Api
{
    [Authorize(Roles = "Manager")]
    [RoutePrefix("api/leave-requests")]
    public class LeaveRequestsApiController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LeaveRequestService service;

        public LeaveRequestsApiController()
        {
            service = new LeaveRequestService(db);
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get(DateTime? from = null, DateTime? to = null,
            string status = null, int page = 1, int pageSize = 20)
        {
            if (page < 1)
            {
                return BadRequest("page must be 1 or greater.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("pageSize must be between 1 and 100.");
            }

            if (from != null && to != null && to < from)
            {
                return BadRequest("to cannot be earlier than from.");
            }

            LeaveStatus? parsedStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                LeaveStatus value;

                if (!Enum.TryParse(status, true, out value)
                    || !Enum.IsDefined(typeof(LeaveStatus), value))
                {
                    return BadRequest("status must be Pending, Approved, Rejected or Cancelled.");
                }

                parsedStatus = value;
            }

            var result = service.SearchRequests(null, null, parsedStatus, null,
                from, to, page, pageSize);

            var dto = new PagedResultDto
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                Items = result.Items.Select(r => new LeaveRequestDto
                {
                    Id = r.Id,
                    EmployeeName = r.User.FullName,
                    LeaveType = r.LeaveType.Name,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalDays = r.TotalDays,
                    Reason = r.Reason,
                    Status = r.Status.ToString(),
                    CreatedDate = r.CreatedDate
                }).ToList()
            };

            return Ok(dto);
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