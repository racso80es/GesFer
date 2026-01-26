using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;

namespace GesFer.Application.Commands.Country;

public record GetCountryByIdCommand(Guid Id) : ICommand<CountryDto?>;

