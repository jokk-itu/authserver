using AuthServer.Authorize.Abstract;

namespace AuthServer.Authorize;

internal class UserAccessor : IUserAccessor
{
    public User GetUser()
    {
        throw new NotImplementedException();
    }

    public User? TryGetUser()
    {
        throw new NotImplementedException();
    }

    public void SetUser(User user)
    {
        throw new NotImplementedException();
    }

    public bool TrySetUser(User user)
    {
        throw new NotImplementedException();
    }
}