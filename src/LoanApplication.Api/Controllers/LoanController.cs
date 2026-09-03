using LoanApplication.Core.Dtos;
using LoanApplication.Core.Domain;
using LoanApplication.Core.Interfaces;
using LoanApplication.Core.Rules;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly RuleEngine _ruleEngine;
    private readonly IApplicationRepository _repository;
    private readonly ApplicationEventPublisher _eventPublisher;
    private readonly LoanDbContext _context;

    public LoanController(
        RuleEngine ruleEngine,
        IApplicationRepository repository,
        ApplicationEventPublisher eventPublisher,
        LoanDbContext context)
    {
        _ruleEngine = ruleEngine;
        _repository = repository;
        _eventPublisher = eventPublisher;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitApplication([FromBody] ApplicationDto applicationDto)
    {
        var ruleResult = await _ruleEngine.EvaluateAsync(applicationDto);
        if (ruleResult.IsDenied)
        {
            return Ok(new { Status = "Denied", Reason = ruleResult.Reason });
        }

        Customer customer;
        Application application;
        bool isReturningCustomer;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingCustomer = await _repository.GetCustomerBySsnAsync(applicationDto.Ssn);
            isReturningCustomer = existingCustomer != null;

            if (isReturningCustomer)
            {
                customer = existingCustomer!;
                customer.FirstName = applicationDto.FirstName;
                customer.LastName = applicationDto.LastName;
                customer.Address = applicationDto.Address;
                customer.State = applicationDto.State;
                customer.CompanyName = applicationDto.CompanyName;
                await _repository.UpdateCustomerAsync(customer);

                var existingApplication = await _repository.GetApplicationByCustomerIdAsync(customer.Id);
                if (existingApplication != null)
                {
                    application = existingApplication;
                    application.RequestedAmount = applicationDto.RequestedAmount;
                    await _repository.UpdateApplicationAsync(application);
                }
                else
                {
                    application = new Application
                    {
                        CustomerId = customer.Id,
                        RequestedAmount = applicationDto.RequestedAmount
                    };
                    application = await _repository.CreateApplicationAsync(application);
                }
            }
            else
            {
                customer = new Customer
                {
                    FirstName = applicationDto.FirstName,
                    LastName = applicationDto.LastName,
                    Address = applicationDto.Address,
                    State = applicationDto.State,
                    CompanyName = applicationDto.CompanyName,
                    Ssn = applicationDto.Ssn
                };
                customer = await _repository.CreateCustomerAsync(customer);

                application = new Application
                {
                    CustomerId = customer.Id,
                    RequestedAmount = applicationDto.RequestedAmount
                };
                application = await _repository.CreateApplicationAsync(application);
            }

            await _eventPublisher.PublishAsync(new ApplicationEvent
            {
                Customer = customer,
                Application = application,
                IsReturningCustomer = isReturningCustomer
            });

            await transaction.CommitAsync();

            return Ok(new
            {
                Status = "Approved",
                CustomerId = customer.Id,
                ApplicationId = application.Id,
                IsReturningCustomer = isReturningCustomer
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}