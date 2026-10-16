using LeaveManagement.Models;
using LeaveManagement.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Services.Description;
using System;

namespace LeaveManagement.Services
{
    public class LeaveRequestService
    {
        private ApplicationDbContext db;

        public LeaveRequestService(ApplicationDbContext context)
        {
            db = context;
        }

        public List<LeaveRequest> GetRequestsForUser(string userId)
        {
            return db.LeaveRequests
                .Include(r => r.LeaveType)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();
        }
        public LeaveRequest GetRequestForUser(int id, string userId)
        {
            return db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.ReviewedByUser)
                .FirstOrDefault(r => r.Id == id && r.UserId == userId);
        }
        public List<LeaveType> GetActiveLeaveTypes()
        {
            return db.LeaveTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToList();
        }

        public bool DatesAreValid(DateTime startDate, DateTime endDate)
        {
            return endDate >= startDate;
        }
        public LeaveRequest CreateRequest(string userId, int leaveTypeId,
    DateTime startDate, DateTime endDate, string reason)
        {
            var request = new LeaveRequest
            {
                UserId = userId,
                LeaveTypeId = leaveTypeId,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = CountDays(startDate, endDate),
                Reason = reason,
                Status = LeaveStatus.Pending,
                CreatedDate = DateTime.Now
            };

            db.LeaveRequests.Add(request);
            db.SaveChanges();

            db.LeaveStatusHistories.Add(new LeaveStatusHistory
            {
                LeaveRequestId = request.Id,
                OldStatus = null,
                NewStatus = LeaveStatus.Pending,
                ChangedByUserId = userId,
                ChangedDate = request.CreatedDate,
                Comment = "Request submitted"
            });

            db.SaveChanges();

            return request;
        }


        public bool CancelRequest(int id, string userId)
        {
            var request = db.LeaveRequests
                .FirstOrDefault(r => r.Id == id && r.UserId == userId);

            if (request == null || request.Status != LeaveStatus.Pending)
            {
                return false;
            }

            request.Status = LeaveStatus.Cancelled;
            request.UpdatedDate = DateTime.Now;

            db.LeaveStatusHistories.Add(new LeaveStatusHistory
            {
                LeaveRequestId = request.Id,
                OldStatus = LeaveStatus.Pending,
                NewStatus = LeaveStatus.Cancelled,
                ChangedByUserId = userId,
                ChangedDate = request.UpdatedDate.Value,
                Comment = "Cancelled by employee"
            });

            db.SaveChanges();

            return true;
        }
        public bool HasOverlap(string userId, DateTime startDate, DateTime endDate)
        {
            return db.LeaveRequests.Any(r =>
                r.UserId == userId &&
                (r.Status == LeaveStatus.Pending || r.Status == LeaveStatus.Approved) &&
                startDate <= r.EndDate &&
                endDate >= r.StartDate);
        }
        public static int CountDays(DateTime startDate, DateTime endDate)
        {
            return (endDate - startDate).Days + 1;
        }

        public LeaveBalance GetBalance(string userId, int leaveTypeId, int year)
        {
            return db.LeaveBalances
                .FirstOrDefault(b => b.UserId == userId
                    && b.LeaveTypeId == leaveTypeId
                    && b.Year == year);
        }

        public bool HasSufficientBalance(string userId, int leaveTypeId, DateTime startDate, int totalDays)
        {
            var balance = GetBalance(userId, leaveTypeId, startDate.Year);

            if (balance == null)
            {
                return false;
            }

            return balance.RemainingDays >= totalDays;
        }
        public List<LeaveBalance> GetBalancesForUser(string userId, int year)
        {
            return db.LeaveBalances
                .Include(b => b.LeaveType)
                .Where(b => b.UserId == userId && b.Year == year)
                .OrderBy(b => b.LeaveType.Name)
                .ToList();
        }
        public void CreateInitialBalances(string userId, int year)
        {
            var types = db.LeaveTypes.Where(t => t.IsActive).ToList();

            foreach (var type in types)
            {
                bool exists = db.LeaveBalances.Any(b =>
                    b.UserId == userId && b.LeaveTypeId == type.Id && b.Year == year);

                if (!exists)
                {
                    db.LeaveBalances.Add(new LeaveBalance
                    {
                        UserId = userId,
                        LeaveTypeId = type.Id,
                        Year = year,
                        AllocatedDays = type.DefaultAnnualAllowance,
                        UsedDays = 0
                    });
                }
            }

            db.SaveChanges();
        }
        public List<LeaveStatusHistory> GetHistoryForRequest(int leaveRequestId)
        {
            return db.LeaveStatusHistories
                .Include(h => h.ChangedByUser)
                .Where(h => h.LeaveRequestId == leaveRequestId)
                .OrderBy(h => h.ChangedDate)
                .ToList();
        }
        public PagedRequests SearchRequests(string userId, string search, LeaveStatus? status,
    int? leaveTypeId, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.User)
                .AsQueryable();

            if (userId != null)
            {
                query = query.Where(r => r.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.User.FirstName.Contains(search) ||
                    r.User.LastName.Contains(search) ||
                    r.Reason.Contains(search));
            }

            if (status != null)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (leaveTypeId != null)
            {
                query = query.Where(r => r.LeaveTypeId == leaveTypeId.Value);
            }

            if (from != null)
            {
                query = query.Where(r => r.EndDate >= from.Value);
            }

            if (to != null)
            {
                query = query.Where(r => r.StartDate <= to.Value);
            }

            int total = query.Count();

            var items = query
                .OrderByDescending(r => r.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedRequests { Items = items, TotalCount = total };
        }
        public List<StatusCount> GetStatusCounts(string userId)
        {
            var query = db.LeaveRequests.AsQueryable();

            if (userId != null)
            {
                query = query.Where(r => r.UserId == userId);
            }

            return query
                .GroupBy(r => r.Status)
                .Select(g => new StatusCount
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }
        

        public List<LeaveRequest> GetUpcomingApproved(string userId, DateTime fromDate, DateTime toDate)
        {
            var query = db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.User)
                .Where(r => r.Status == LeaveStatus.Approved
                    && r.StartDate >= fromDate
                    && r.StartDate <= toDate);

            if (userId != null)
            {
                query = query.Where(r => r.UserId == userId);
            }

            return query
                .OrderBy(r => r.StartDate)
                .ToList();
        }
    }
}