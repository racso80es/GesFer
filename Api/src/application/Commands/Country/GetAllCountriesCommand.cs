using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;

namespace GesFer.Application.Commands.Country;

public record GetAllCountriesCommand() : ICommand<List<CountryDto>>;

