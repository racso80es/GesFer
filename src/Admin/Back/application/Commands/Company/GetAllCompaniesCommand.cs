using GesFer.Admin.Application.DTOs.Company;
using MediatR;

namespace GesFer.Admin.Application.Commands.Company;

public record GetAllCompaniesCommand() : IRequest<List<CompanyDto>>;
