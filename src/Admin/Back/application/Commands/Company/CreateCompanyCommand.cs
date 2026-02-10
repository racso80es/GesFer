using GesFer.Admin.Application.DTOs.Company;
using MediatR;

namespace GesFer.Admin.Application.Commands.Company;

public record CreateCompanyCommand(CreateCompanyDto Dto) : IRequest<CompanyDto>;
