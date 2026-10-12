namespace LeaveManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueLeaveBalance : DbMigration
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
            DropIndex("dbo.LeaveBalances", "IX_UserLeaveTypeYear");
        }

    }
}
