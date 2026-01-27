using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;

namespace GesFer.Application.Commands.PostalCode;

public record GetAllPostalCodesCommand(Guid? CityId = null, Guid? StateId = null, Guid? CountryId = null) : ICommand<List<PostalCodeDto>>;

