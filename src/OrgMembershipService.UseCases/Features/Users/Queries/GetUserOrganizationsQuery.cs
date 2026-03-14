using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Features.Users.Queries;

/// <summary>
/// Запрос списка организаций пользователя
/// </summary>
/// <param name="IdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="Status">Фильтр по статусу членства (Active, Deactivated, Removed)</param>
public record GetUserOrganizationsQuery(string IdentityId, string? Status) : IRequest<UserOrganizationsDto>;

/// <summary>
/// Список организаций пользователя
/// </summary>
/// <param name="Organizations">Организации, в которых состоит пользователь</param>
public record UserOrganizationsDto(IReadOnlyCollection<UserOrganizationDto> Organizations);

/// <summary>
/// Данные организации пользователя
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="Status">Статус членства строкой (Active, Deactivated, Removed)</param>
/// <param name="JoinedAt">Дата и время вступления в организацию</param>
/// <param name="RemovedAt">Дата и время удаления из организации</param>
public record UserOrganizationDto(
    Guid OrganizationId,
    Guid MembershipId,
    string Status,
    DateTimeOffset? JoinedAt,
    DateTimeOffset? RemovedAt);

internal class GetUserOrganizationsQueryValidator : AbstractValidator<GetUserOrganizationsQuery>
{
    public GetUserOrganizationsQueryValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty()
            .WithMessage("Идентификатор пользователя обязателен");

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Некорректный статус членства");
    }

    private static bool BeValidStatus(string? status) =>
        Enum.TryParse<MembershipStatus>(status, ignoreCase: true, out _);
}

internal class GetUserOrganizationsQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<GetUserOrganizationsQuery, UserOrganizationsDto>
{
    public async Task<UserOrganizationsDto> Handle(GetUserOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);

        MembershipStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
            statusFilter = Enum.Parse<MembershipStatus>(request.Status, ignoreCase: true);

        var query = dbContext.Memberships
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (statusFilter.HasValue)
            query = query.Where(x => x.Status == statusFilter.Value);

        var organizationsData = await query
            .OrderBy(x => x.OrganizationId)
            .Select(x => new
            {
                x.OrganizationId,
                x.Id,
                x.Status,
                x.JoinedAt,
                x.RemovedAt
            })
            .ToListAsync(cancellationToken);

        var organizations = organizationsData
            .Select(x => new UserOrganizationDto(
                x.OrganizationId,
                x.Id,
                x.Status.ToString(),
                x.JoinedAt,
                x.RemovedAt))
            .ToList();

        return new UserOrganizationsDto(organizations);
    }
}
