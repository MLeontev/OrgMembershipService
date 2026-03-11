using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Queries;

/// <summary>
/// Запрос членства пользователя в организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="IdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
public record GetUserMembershipQuery(Guid OrganizationId, string IdentityId) : IRequest<UserMembershipDto>;

/// <summary>
/// Данные членства пользователя в организации
/// </summary>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="Status">Статус членства строкой (Active, Deactivated, Removed)</param>
/// <param name="JoinedAt">Дата и время вступления в организацию</param>
/// <param name="RemovedAt">Дата и время удаления из организации</param>
/// <param name="Department">Подразделение пользователя</param>
/// <param name="Title">Должность пользователя</param>
/// <param name="Roles">Список кодов ролей пользователя</param>
public record UserMembershipDto(
    Guid MembershipId,
    string Status,
    DateTimeOffset? JoinedAt,
    DateTimeOffset? RemovedAt,
    string? Department,
    string? Title,
    IReadOnlyCollection<string> Roles);

internal class GetUserMembershipQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<GetUserMembershipQuery, UserMembershipDto>
{
    public async Task<UserMembershipDto> Handle(GetUserMembershipQuery request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);
        
        var membership = await dbContext.Memberships
            .AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId && x.UserId == userId)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.JoinedAt,
                x.RemovedAt,
                x.Department,
                x.Title
            })
            .SingleOrDefaultAsync(cancellationToken);
        
        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");
        
        var roles = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x => x.MembershipId == membership.Id)
            .Select(x => x.Role.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
        
        return new UserMembershipDto(
            membership.Id,
            membership.Status.ToString(),
            membership.JoinedAt,
            membership.RemovedAt,
            membership.Department,
            membership.Title,
            roles);
    }
}
