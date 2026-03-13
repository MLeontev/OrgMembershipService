using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;

namespace OrgMembershipService.Application.Features.Memberships.Queries;

/// <summary>
/// Запрос списка участников организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
public record GetOrganizationMembersQuery(Guid OrganizationId) : IRequest<OrganizationMembersDto>;

/// <summary>
/// Список участников организации
/// </summary>
/// <param name="Members">Участники организации</param>
public record OrganizationMembersDto(IReadOnlyCollection<OrganizationMemberDto> Members);

/// <summary>
/// Данные участника организации
/// </summary>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="UserId">Внутренний идентификатор пользователя в сервисе</param>
/// <param name="IdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="Email">Email пользователя</param>
/// <param name="FirstName">Имя</param>
/// <param name="LastName">Фамилия</param>
/// <param name="Patronymic">Отчество (необязательно)</param>
/// <param name="Status">Статус членства строкой (Active, Deactivated, Removed)</param>
/// <param name="Department">Подразделение пользователя</param>
/// <param name="Title">Должность пользователя</param>
/// <param name="JoinedAt">Дата и время вступления в организацию</param>
/// <param name="RemovedAt">Дата и время удаления из организации</param>
/// <param name="Roles">Список кодов ролей пользователя</param>
public record OrganizationMemberDto(
    Guid MembershipId,
    Guid UserId,
    string IdentityId,
    string Email,
    string FirstName,
    string LastName,
    string? Patronymic,
    string Status,
    string? Department,
    string? Title,
    DateTimeOffset? JoinedAt,
    DateTimeOffset? RemovedAt,
    IReadOnlyCollection<string> Roles);

internal class GetOrganizationMembersQueryHandler(IDbContext dbContext) : IRequestHandler<GetOrganizationMembersQuery, OrganizationMembersDto>
{
    public async Task<OrganizationMembersDto> Handle(
        GetOrganizationMembersQuery request,
        CancellationToken cancellationToken)
    {
        var members = await dbContext.Memberships
            .AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId)
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.FirstName)
            .ThenBy(x => x.User.Email)
            .Select(x => new
            {
                MembershipId = x.Id,
                x.UserId,
                x.User.IdentityId,
                x.User.Email,
                x.User.FirstName,
                x.User.LastName,
                x.User.Patronymic,
                x.Status,
                x.Department,
                x.Title,
                x.JoinedAt,
                x.RemovedAt
            })
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
            return new OrganizationMembersDto([]);

        var membershipIds = members.Select(x => x.MembershipId).ToList();

        var roleAssignments = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x => membershipIds.Contains(x.MembershipId))
            .Select(x => new { x.MembershipId, RoleCode = x.Role.Code })
            .ToListAsync(cancellationToken);

        var rolesByMembershipId = roleAssignments
            .GroupBy(x => x.MembershipId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<string>)g
                    .Select(x => x.RoleCode)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList());

        var result = members
            .Select(x =>
            {
                var roles = rolesByMembershipId.TryGetValue(x.MembershipId, out var assignedRoles)
                    ? assignedRoles
                    : [];

                return new OrganizationMemberDto(
                    x.MembershipId,
                    x.UserId,
                    x.IdentityId,
                    x.Email,
                    x.FirstName,
                    x.LastName,
                    x.Patronymic,
                    x.Status.ToString(),
                    x.Department,
                    x.Title,
                    x.JoinedAt,
                    x.RemovedAt,
                    roles);
            })
            .ToList();

        return new OrganizationMembersDto(result);
    }
}
