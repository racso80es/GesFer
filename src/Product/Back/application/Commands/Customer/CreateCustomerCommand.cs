using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;

namespace GesFer.Application.Commands.Customer;

public record CreateCustomerCommand(CreateCustomerDto Dto) : ICommand<CustomerDto>;

