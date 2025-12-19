using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;

namespace GesFer.Application.Commands.City;

public record GetAllCitiesCommand(Guid? StateId = null, Guid? CountryId = null) : ICommand<List<CityDto>>;

