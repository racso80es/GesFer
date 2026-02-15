using GesFer.Admin.Application.DTOs.Company;
using MediatR;

namespace GesFer.Admin.Application.Commands.Company;

/// <summary>
/// Obtiene una empresa por nombre (case-insensitive).
/// Product usa este contrato para login; Admin expone como mucho la empresa que corresponde al contexto.
/// </summary>
public record GetCompanyByNameCommand(string Name) : IRequest<CompanyDto?>;
