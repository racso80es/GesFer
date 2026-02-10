using GesFer.Admin.Application.DTOs.Company;
using MediatR;

namespace GesFer.Admin.Application.Commands.Company;

public record UpdateCompanyCommand(Guid Id, UpdateCompanyDto Dto) : IRequest<CompanyDto>;
