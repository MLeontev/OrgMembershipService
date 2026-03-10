using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Users.Queries;

public record GetUserByIdentityQuery(string IdentityId) : IRequest<UserByIdentityDto>;

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