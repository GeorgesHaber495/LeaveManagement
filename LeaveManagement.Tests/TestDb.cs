using System;
using System.Data.Entity;
using LeaveManagement.Models;
using LeaveManagement.Models.Entities;

namespace LeaveManagement.Tests
{
    public class TestDb : IDisposable
    {
        public ApplicationDbContext Db;

        public string EmployeeId = "employee-1";
        public string OtherEmployeeId = "employee-2";
        public string ManagerId = "manager-1";

        public int AnnualTypeId;
        public int SickTypeId;

        public TestDb()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<ApplicationDbContext>());

            Db = new ApplicationDbContext();
            Db.Database.Initialize(true);

            Db.Users.Add(new ApplicationUser
            {
                Id = EmployeeId,
                UserName = "employee1@test.local",
                Email = "employee1@test.local",
                FirstName = "Test",
                LastName = "Employee"
            });

            Db.Users.Add(new ApplicationUser
            {
                Id = OtherEmployeeId,
                UserName = "employee2@test.local",
                Email = "employee2@test.local",
                FirstName = "Other",
                LastName = "Employee"
            });

            Db.Users.Add(new ApplicationUser
            {
                Id = ManagerId,
                UserName = "manager1@test.local",
                Email = "manager1@test.local",
                FirstName = "Test",
                LastName = "Manager"
            });

            var annual = new LeaveType { Name = "Annual", DefaultAnnualAllowance = 20, IsActive = true };
            var sick = new LeaveType { Name = "Sick", DefaultAnnualAllowance = 2, IsActive = true };

            Db.LeaveTypes.Add(annual);
            Db.LeaveTypes.Add(sick);

            Db.SaveChanges();

            AnnualTypeId = annual.Id;
            SickTypeId = sick.Id;

            Db.LeaveBalances.Add(new LeaveBalance
            {
                UserId = EmployeeId,
                LeaveTypeId = AnnualTypeId,
                Year = DateTime.Today.Year,
                AllocatedDays = 20,
                UsedDays = 0
            });

            Db.LeaveBalances.Add(new LeaveBalance
            {
                UserId = EmployeeId,
                LeaveTypeId = SickTypeId,
                Year = DateTime.Today.Year,
                AllocatedDays = 2,
                UsedDays = 0
            });

            Db.SaveChanges();
        }

        public void Dispose()
        {
            Db.Dispose();
        }
    }
}