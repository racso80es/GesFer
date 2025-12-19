using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.Supplier;

public record DeleteSupplierCommand(Guid Id) : ICommand;

