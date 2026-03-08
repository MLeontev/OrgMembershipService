using FluentValidation;
using MediatR;
using OrgMembershipService.Application.Database;
using OrgMembershipService.Application.Users.Services;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Users.Commands;

public record RegisterUserCommand(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName, 
    string? Patronymic) : IRequest<Guid>;

internal class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль обязателен");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Имя обязательно");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Фамилия обязательна");
    }
}

internal class RegisterUserCommandHandler(
    IValidator<RegisterUserCommand> validator,
    IDbContext dbContext, 
    IIdentityProviderService identityProviderService) : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var identityId = await identityProviderService.RegisterUserAsync(
            new UserModel(request.Email, request.Password, request.FirstName, request.LastName),
            cancellationToken);

        var user = User.Create(request.FirstName, request.LastName, request.Patronymic, request.Email, identityId);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return user.Id;
    }
}