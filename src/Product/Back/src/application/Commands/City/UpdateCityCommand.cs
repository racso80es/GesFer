using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;

namespace GesFer.Application.Commands.City;

public record UpdateCityCommand(Guid Id, UpdateCityDto Dto) : ICommand<CityDto>;

