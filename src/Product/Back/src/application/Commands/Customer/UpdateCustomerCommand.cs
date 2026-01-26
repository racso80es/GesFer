using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;

namespace GesFer.Application.Commands.Customer;

public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Dto) : ICommand<CustomerDto>;

