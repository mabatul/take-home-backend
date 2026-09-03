using LoanApplication.Core.Domain;
using LoanApplication.Core.Interfaces;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly LoanDbContext _context;

    public ApplicationRepository(LoanDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetCustomerBySsnAsync(string ssn)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Ssn == ssn);
    }

    public async Task<Application?> GetApplicationByCustomerIdAsync(Guid customerId)
    {
        return await _context.Applications.FirstOrDefaultAsync(a => a.CustomerId == customerId);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        customer.Id = Guid.NewGuid();
        customer.CreatedAt = DateTime.UtcNow;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Application> CreateApplicationAsync(Application application)
    {
        application.Id = Guid.NewGuid();
        application.CreatedAt = DateTime.UtcNow;
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Application> UpdateApplicationAsync(Application application)
    {
        application.UpdatedAt = DateTime.UtcNow;
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }
}