using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using LeaveManagement.Models.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LeaveManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveStatusHistory> LeaveStatusHistories { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<LeaveRequest>()
                .HasRequired(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

           
            modelBuilder.Entity<LeaveRequest>()
                .HasOptional(r => r.ReviewedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<LeaveRequest>()
                .HasRequired(r => r.LeaveType)
                .WithMany(t => t.LeaveRequests)
                .HasForeignKey(r => r.LeaveTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LeaveBalance>()
                .HasRequired(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LeaveBalance>()
                .HasRequired(b => b.LeaveType)
                .WithMany(t => t.LeaveBalances)
                .HasForeignKey(b => b.LeaveTypeId)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<LeaveStatusHistory>()
                .HasRequired(h => h.LeaveRequest)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(h => h.LeaveRequestId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<LeaveStatusHistory>()
                .HasRequired(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
}