using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Users.Queries;

/// <summary>
/// Запрос пользователя по identityId (sub из access токена Keycloak)
/// </summary>
/// <param name="IdentityId">Внешний идентификатор пользователя в Keycloak (sub из access токена)</param>
public record GetUserByIdentityQuery(string IdentityId) : IRequest<UserByIdentityDto>;

/// <summary>
/// Профиль пользователя для internal интеграций
/// </summary>
/// <param name="Id">Внутренний идентификатор пользователя в сервисе</param>
/// <param name="IdentityId">Внешний идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="Email">Email пользователя</param>
/// <param name="FirstName">Имя</param>
/// <param name="LastName">Фамилия</param>
/// <param name="Patronymic">Отчество (необязательно)</param>
public record UserByIdentityDto(
    Guid Id,
    string IdentityId,
    string Email,
    string FirstName,
    string LastName,
    string? Patronymic);
    
internal class GetUserByIdentityQueryHandler(IDbContext dbContext) : IRequestHandler<GetUserByIdentityQuery, UserByIdentityDto>
{
    public async Task<UserByIdentityDto> Handle(GetUserByIdentityQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.IdentityId == request.IdentityId)
            .Select(x => new UserByIdentityDto(
                    x.Id,
                    x.IdentityId,
                    x.Email,
                    x.FirstName,
                    x.LastName,
                    x.Patronymic))
            .SingleOrDefaultAsync(cancellationToken);

        return user ?? throw new NotFoundException("USER_NOT_FOUND_BY_IDENTITY", "Пользователь не найден");
    }
}
