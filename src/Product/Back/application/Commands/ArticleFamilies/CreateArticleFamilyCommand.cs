using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;

namespace GesFer.Application.Commands.ArticleFamilies;

public record CreateArticleFamilyCommand(CreateArticleFamilyDto Dto) : ICommand<ArticleFamilyDto>;
