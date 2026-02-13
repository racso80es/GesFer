using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.ArticleFamilies;

public record DeleteArticleFamilyCommand(Guid Id) : ICommand;
