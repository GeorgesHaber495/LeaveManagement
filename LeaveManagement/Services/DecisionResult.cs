namespace LeaveManagement.Services
{
    public enum DecisionResult
    {
        Success = 1,
        NotFound = 2,
        NotPending = 3,
        InsufficientBalance = 4
    }
}