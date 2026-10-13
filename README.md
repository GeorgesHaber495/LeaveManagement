# Employee Leave Management

An ASP.NET MVC application for submitting and approving employee leave requests.
Employees request leave against a yearly balance; managers approve or reject.

## Built with

- ASP.NET MVC 5 on .NET Framework 4.8
- Entity Framework 6, Code First with migrations
- ASP.NET Identity for authentication and roles
- SQL Server LocalDB
- MSTest for unit tests

## Running it locally

You need Visual Studio 2022 with the ASP.NET and web development workload.
SQL Server LocalDB comes with it, so there is nothing extra to install.

1. Open `LeaveManagement.sln`.
2. Restore the NuGet packages (Tools > NuGet Package Manager > Restore, or just build).
3. Open the Package Manager Console and run `Update-Database`.
   This creates the database and fills it with the demo data below.
4. Press F5.

The connection string is in `LeaveManagement/Web.config` under the name
`DefaultConnection`. It points at `(localdb)\MSSQLLocalDB` and the database is
called `LeaveManagementDb`.

## Demo accounts

All three use the password `Demo@123`.

| Role     | Email |
|----------|-------|
| Manager  | manager@demo.local |
| Employee | employee@demo.local |
| Employee | employee2@demo.local |

The seed method checks before inserting, so running `Update-Database` more than
once does not duplicate anything.

## How the project is organised

- `Models/Entities` - the domain classes: LeaveRequest, LeaveType,
  LeaveBalance, LeaveStatusHistory, and the LeaveStatus enum
- `Models/IdentityModels.cs` - ApplicationUser and the DbContext, including
  the relationship configuration
- `Migrations` - EF migrations and the seed data
- `Controllers`, `Views` - the MVC layer
- `LeaveManagement.Tests` - unit tests

## Assumptions I made

- Leave is counted in whole days. Half-days are not supported.
- A reason is required on every request.
- Unpaid leave is given an allowance of 30 days so it fits the same balance
  model as the other types, rather than being treated as a special case.
- Leave types are retired by setting `IsActive` to false, never deleted, so
  that old requests keep pointing at something valid.
- Deleting a user does not delete their leave history. The relationships are
  configured with cascade delete turned off for that reason.
- Status changes are recorded in a separate history table so there is an audit
  trail of who changed what and when.

## Notes

The first attempt to run this on my machine was blocked by Windows Smart App
Control, which refused to load the compiled DLL from the bin folder. Running
the project under a different Windows account solved it.

## Business rules

- **Overlap.** A new request is refused if it overlaps an existing Pending or
  Approved request from the same employee. Rejected and Cancelled requests do
  not block anything. Two ranges overlap when the new start is on or before the
  existing end and the new end is on or after the existing start — this covers
  a request that starts inside an existing one, ends inside one, sits entirely
  within one, or completely contains one.
- **Balance at submission.** A request is refused if the employee does not have
  enough remaining days of that type for the year the leave starts in. A missing
  balance row counts as no entitlement, not unlimited.
- **Balance at approval.** The check runs again when the manager approves,
  because days can be consumed by another request in between.
- **Approval is atomic.** Approving changes the request status, writes a history
  row, and increases UsedDays. All three happen inside a database transaction,
  so a failure part way through leaves the data unchanged. Without it, a status
  that saved while the balance did not would be a silent error nobody would see.
- **Rejecting does not touch the balance.**
- **Only Pending requests can change.** Enforced in the services, not only by
  hiding buttons in the views.

## Tests

Run with Test > Run All Tests. Covers the remaining-days calculation and the
inclusive day count, including the single-day case and a range crossing a month
boundary.

## Known limitations

- Public holidays are not handled; a request spanning one counts it as leave.
- Half-days are not supported.
- Any manager can approve any employee's request; there is no team structure.
- Two managers approving the same request at the same moment is not handled.