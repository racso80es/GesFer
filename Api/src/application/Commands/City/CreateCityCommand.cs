using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;

namespace GesFer.Application.Commands.City;

public record CreateCityCommand(CreateCityDto Dto) : ICommand<CityDto>;

