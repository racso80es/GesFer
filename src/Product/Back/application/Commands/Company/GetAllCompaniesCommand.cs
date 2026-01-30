using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;

namespace GesFer.Application.Commands.Company;

public record GetAllCompaniesCommand() : ICommand<List<CompanyDto>>;

