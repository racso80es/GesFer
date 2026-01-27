using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;

namespace GesFer.Application.Commands.Country;

public record UpdateCountryCommand(Guid Id, UpdateCountryDto Dto) : ICommand<CountryDto>;

