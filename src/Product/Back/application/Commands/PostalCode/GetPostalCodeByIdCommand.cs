using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;

namespace GesFer.Application.Commands.PostalCode;

public record GetPostalCodeByIdCommand(Guid Id) : ICommand<PostalCodeDto?>;

