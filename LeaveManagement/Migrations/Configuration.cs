namespace LeaveManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using LeaveManagement.Models;
    using LeaveManagement.Models.Entities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<LeaveManagement.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            SeedRoles(context);
            SeedUsers(context);
            SeedLeaveTypes(context);
            SeedBalances(context);
            SeedRequests(context);
        }

        private void SeedRoles(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists("Manager"))
            {
                roleManager.Create(new IdentityRole("Manager"));
            }

            if (!roleManager.RoleExists("Employee"))
            {
                roleManager.Create(new IdentityRole("Employee"));
            }
        }

        private void SeedUsers(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            CreateUser(userManager, "manager@demo.local", "Rana", "Khoury", "Manager");
            CreateUser(userManager, "employee@demo.local", "Karim", "Nasr", "Employee");
            CreateUser(userManager, "employee2@demo.local", "Lara", "Daou", "Employee");
            CreateUser(userManager, "employee3@demo.local", "Georges", "Haber", "Employee");
            CreateUser(userManager,"employee4@demo.local","elie","hsh","Employee");
        }

        private void CreateUser(UserManager<ApplicationUser> userManager,
                                string email, string firstName, string lastName, string role)
        {
            if (userManager.FindByEmail(email) != null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = userManager.Create(user, "Demo@123");

            if (result.Succeeded)
            {
                userManager.AddToRole(user.Id, role);
            }
        }
       

        private void SeedLeaveTypes(ApplicationDbContext context)
        {
            context.LeaveTypes.AddOrUpdate(t => t.Name,
                new LeaveType { Name = "Annual", DefaultAnnualAllowance = 15, IsActive = true },
                new LeaveType { Name = "Sick", DefaultAnnualAllowance = 10, IsActive = true },
                new LeaveType { Name = "Unpaid", DefaultAnnualAllowance = 30, IsActive = true });

            context.SaveChanges();
        }

        private void SeedBalances(ApplicationDbContext context)
        {
            int year = DateTime.Now.Year;

            var users = context.Users.ToList();
            var types = context.LeaveTypes.ToList();

            foreach (var user in users)
            {
                foreach (var type in types)
                {
                    bool exists = context.LeaveBalances.Any(b =>
                        b.UserId == user.Id &&
                        b.LeaveTypeId == type.Id &&
                        b.Year == year);

                    if (exists)
                    {
                        continue;
                    }

                    context.LeaveBalances.Add(new LeaveBalance
                    {
                        UserId = user.Id,
                        LeaveTypeId = type.Id,
                        Year = year,
                        AllocatedDays = type.DefaultAnnualAllowance,
                        UsedDays = 0
                    });
                }
            }

            context.SaveChanges();
        }

        private void SeedRequests(ApplicationDbContext context)
        {
            if (context.LeaveRequests.Any())
            {
                return;
            }

            var employee = context.Users.First(u => u.Email == "employee@demo.local");
            var employee2 = context.Users.First(u => u.Email == "employee2@demo.local");
            var manager = context.Users.First(u => u.Email == "manager@demo.local");

            var annual = context.LeaveTypes.First(t => t.Name == "Annual");
            var sick = context.LeaveTypes.First(t => t.Name == "Sick");

            var pending = new LeaveRequest
            {
                UserId = employee.Id,
                LeaveTypeId = annual.Id,
                StartDate = DateTime.Today.AddDays(10),
                EndDate = DateTime.Today.AddDays(14),
                TotalDays = 5,
                Reason = "Family trip",
                Status = LeaveStatus.Pending,
                CreatedDate = DateTime.Now.AddDays(-2)
            };

            var approved = new LeaveRequest
            {
                UserId = employee2.Id,
                LeaveTypeId = sick.Id,
                StartDate = DateTime.Today.AddDays(-20),
                EndDate = DateTime.Today.AddDays(-19),
                TotalDays = 2,
                Reason = "Medical appointment",
                Status = LeaveStatus.Approved,
                ManagerComment = "Approved, get well soon",
                ReviewedByUserId = manager.Id,
                ReviewedDate = DateTime.Now.AddDays(-21),
                CreatedDate = DateTime.Now.AddDays(-22)
            };

            context.LeaveRequests.Add(pending);
            context.LeaveRequests.Add(approved);
            context.SaveChanges();

            context.LeaveStatusHistories.Add(new LeaveStatusHistory
            {
                LeaveRequestId = pending.Id,
                OldStatus = null,
                NewStatus = LeaveStatus.Pending,
                ChangedByUserId = employee.Id,
                ChangedDate = pending.CreatedDate
            });

            context.LeaveStatusHistories.Add(new LeaveStatusHistory
            {
                LeaveRequestId = approved.Id,
                OldStatus = null,
                NewStatus = LeaveStatus.Pending,
                ChangedByUserId = employee2.Id,
                ChangedDate = approved.CreatedDate
            });

            context.LeaveStatusHistories.Add(new LeaveStatusHistory
            {
                LeaveRequestId = approved.Id,
                OldStatus = LeaveStatus.Pending,
                NewStatus = LeaveStatus.Approved,
                ChangedByUserId = manager.Id,
                ChangedDate = approved.ReviewedDate.Value,
                Comment = "Approved, get well soon"
            });

            context.SaveChanges();

            var usedBalance = context.LeaveBalances.First(b =>
                b.UserId == employee2.Id &&
                b.LeaveTypeId == sick.Id &&
                b.Year == DateTime.Now.Year);

            usedBalance.UsedDays = 2;
            context.SaveChanges();
        }
    }
}
