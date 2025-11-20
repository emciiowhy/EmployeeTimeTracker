namespace EmployeeTimeTracker.Interfaces
{
    // Interface demonstrating ABSTRACTION
    // Any class implementing this must provide these methods
    public interface IPayable
    {
        decimal CalculateMonthlyPay();
        string GeneratePayslip();
    }
}