using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;

namespace GesFer.Application.Commands.Customer;

public record GetAllCustomersCommand(Guid? CompanyId = null) : ICommand<List<CustomerDto>>;

