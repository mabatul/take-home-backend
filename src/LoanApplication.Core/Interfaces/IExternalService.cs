using LoanApplication.Core.Domain;

namespace LoanApplication.Core.Interfaces;

public interface IExternalService
{
    Task SendCustomerAsync(Customer customer, bool isUpdate);
    Task SendApplicationAsync(Application application, bool isUpdate);
}