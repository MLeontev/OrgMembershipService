using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Organizations.Commands;

public record CreateOrganizationOwnerCommand(Guid OrganizationId, Guid OwnerUserId) : IRequest;

internal class CreateOrganizationOwnerCommandHandler(IDbContext dbContext) : IRequestHandler<CreateOrganizationOwnerCommand>
{
    public async Task Handle(CreateOrganizationOwnerCommand request, CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.OwnerUserId, cancellationToken);
        
        if (!userExists)
            throw new NotFoundException("USER_NOT_FOUND", "Пользователь не найден");
        
        var membershipExists = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == request.OrganizationId && x.UserId == request.OwnerUserId,
                cancellationToken);

        if (membershipExists)
            throw new ConflictException("MEMBERSHIP_ALREADY_EXISTS", "Участник уже существует в организации");

        var ownerRole = await dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Code == "ORG_OWNER" && x.OrganizationId == null,
                cancellationToken);
        
        if (ownerRole is null)
            throw new NotFoundException("ORG_OWNER_ROLE_NOT_FOUND", "Роль владельца не найдена");
        
        var membership = Membership.Create(request.OrganizationId, request.OwnerUserId);
        membership.AssignRole(ownerRole.Id, null);
        
        dbContext.Memberships.Add(membership);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}