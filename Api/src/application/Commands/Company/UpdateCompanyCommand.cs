using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;

namespace GesFer.Application.Commands.Company;

public record UpdateCompanyCommand(Guid Id, UpdateCompanyDto Dto) : ICommand<CompanyDto>;

