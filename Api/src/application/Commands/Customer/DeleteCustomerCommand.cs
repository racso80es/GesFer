using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.Customer;

public record DeleteCustomerCommand(Guid Id) : ICommand;

