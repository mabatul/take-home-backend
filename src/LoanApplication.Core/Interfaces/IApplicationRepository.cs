using LoanApplication.Core.Domain;

namespace LoanApplication.Core.Interfaces;

public interface IApplicationRepository
{
    Task<Customer?> GetCustomerBySsnAsync(string ssn);
    Task<Application?> GetApplicationByCustomerIdAsync(Guid customerId);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Application> CreateApplicationAsync(Application application);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<Application> UpdateApplicationAsync(Application application);
}