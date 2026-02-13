using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;

namespace GesFer.Application.Commands.ArticleFamilies;

public record UpdateArticleFamilyCommand(Guid Id, UpdateArticleFamilyDto Dto) : ICommand<ArticleFamilyDto>;
