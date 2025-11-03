
namespace Contracts.Auth;
[Flags]
public enum Roles
{
    None = 0,
    User = 1 << 0,
    Member = 1 << 1,
    Moderator = 1 << 2,
    Admin = 1 << 3
}
