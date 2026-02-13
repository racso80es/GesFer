using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;

namespace GesFer.Application.Commands.ArticleFamilies;

public record GetAllArticleFamiliesCommand(Guid? CompanyId = null) : ICommand<List<ArticleFamilyDto>>;
