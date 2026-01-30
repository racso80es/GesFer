using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Supplier;

namespace GesFer.Application.Commands.Supplier;

public record CreateSupplierCommand(CreateSupplierDto Dto) : ICommand<SupplierDto>;

