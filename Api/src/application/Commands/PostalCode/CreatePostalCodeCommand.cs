using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;

namespace GesFer.Application.Commands.PostalCode;

public record CreatePostalCodeCommand(CreatePostalCodeDto Dto) : ICommand<PostalCodeDto>;

