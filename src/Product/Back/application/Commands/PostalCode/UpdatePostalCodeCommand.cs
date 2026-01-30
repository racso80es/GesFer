using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;

namespace GesFer.Application.Commands.PostalCode;

public record UpdatePostalCodeCommand(Guid Id, UpdatePostalCodeDto Dto) : ICommand<PostalCodeDto>;

