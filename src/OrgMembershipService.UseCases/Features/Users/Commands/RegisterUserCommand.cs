using FluentValidation;
using MediatR;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Features.Users.Commands;

public record RegisterUserCommand(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName, 
    string? Patronymic) : IRequest<RegisterUserDto>;

public record RegisterUserDto(Guid UserId, string IdentityId);

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
    IDbContext dbContext, 
    IIdentityProviderService identityProviderService) : IRequestHandler<RegisterUserCommand, RegisterUserDto>
{
    public async Task<RegisterUserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var identityId = await identityProviderService.RegisterUserAsync(
            new UserModel(request.Email, request.Password, request.FirstName, request.LastName),
            cancellationToken);

        var user = User.Create(request.FirstName, request.LastName, request.Patronymic, request.Email, identityId);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return new RegisterUserDto(user.Id, identityId);
    }
}
