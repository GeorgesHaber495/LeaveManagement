using LeaveManagement.Models;
using LeaveManagement.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LeaveManagement.Services
{
    public class LeaveReviewService
    {
        private ApplicationDbContext db;

        public LeaveReviewService(ApplicationDbContext context)
        {
            db = context;
        }

        public List<LeaveRequest> GetAllRequests(bool pendingOnly)
        {
            var query = db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.User)
                .AsQueryable();

            if (pendingOnly)
            {
                query = query.Where(r => r.Status == LeaveStatus.Pending);
            }

            return query
                .OrderByDescending(r => r.CreatedDate)
                .ToList();
        }
        public LeaveRequest GetRequest(int id)
        {
            return db.LeaveRequests
                .Include(r => r.LeaveType)
                .Include(r => r.User)
                .Include(r => r.ReviewedByUser)
                .FirstOrDefault(r => r.Id == id);
        }

        public LeaveBalance GetBalance(string userId, int leaveTypeId, int year)
        {
            return db.LeaveBalances
                .FirstOrDefault(b => b.UserId == userId
                    && b.LeaveTypeId == leaveTypeId
                    && b.Year == year);
        }

        public List<LeaveStatusHistory> GetHistory(int leaveRequestId)
        {
            return db.LeaveStatusHistories
                .Include(h => h.ChangedByUser)
                .Where(h => h.LeaveRequestId == leaveRequestId)
                .OrderBy(h => h.ChangedDate)
                .ToList();
        }
        public DecisionResult Decide(int id, string managerId, bool approve, string comment)
        {
            var request = db.LeaveRequests.FirstOrDefault(r => r.Id == id);

            if (request == null)
            {
                return DecisionResult.NotFound;
            }

            if (request.Status != LeaveStatus.Pending)
            {
                return DecisionResult.NotPending;
            }

            var newStatus = approve ? LeaveStatus.Approved : LeaveStatus.Rejected;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (approve)
                    {
                        var balance= db.LeaveBalances.FirstOrDefault(b =>
                            b.UserId == request.UserId &&
                            b.LeaveTypeId == request.LeaveTypeId &&
                            b.Year == request.StartDate.Year);

                        if (balance == null || balance.RemainingDays < request.TotalDays)
                        {
                            transaction.Rollback();
                            return DecisionResult.InsufficientBalance;
                        }

                        balance.UsedDays = balance.UsedDays + request.TotalDays;
                    }

                    request.Status = newStatus;
                    request.ManagerComment = comment;
                    request.ReviewedByUserId = managerId;
                    request.ReviewedDate = DateTime.Now;
                    request.UpdatedDate = DateTime.Now;

                    db.LeaveStatusHistories.Add(new LeaveStatusHistory
                    {
                        LeaveRequestId = request.Id,
                        OldStatus = LeaveStatus.Pending,
                        NewStatus = newStatus,
                        ChangedByUserId = managerId,
                        ChangedDate = request.ReviewedDate.Value,
                        Comment = comment
                    });

                    db.SaveChanges();
                    transaction.Commit();

                    return DecisionResult.Success;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}