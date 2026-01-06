using Application.Abstractions.Persistence;

namespace Application.UseCases.Admin.Users;

public sealed record ListUsersQuery(int Limit, int Offset);
public sealed record ListUsersResult(IReadOnlyList<AdminUserListItem> Users);
