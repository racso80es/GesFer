using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;

namespace GesFer.Application.Commands.City;

public record GetCityByIdCommand(Guid Id) : ICommand<CityDto?>;

