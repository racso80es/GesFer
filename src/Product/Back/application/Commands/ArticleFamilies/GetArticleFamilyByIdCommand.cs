using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;

namespace GesFer.Application.Commands.ArticleFamilies;

public record GetArticleFamilyByIdCommand(Guid Id, Guid? CompanyId = null) : ICommand<ArticleFamilyDto?>;
