using GesFer.Admin.Application.DTOs.Company;
using MediatR;

namespace GesFer.Admin.Application.Commands.Company;

public record GetCompanyByIdCommand(Guid Id) : IRequest<CompanyDto?>;
