using System.Linq;
using Microsoft.Extensions.Options;

namespace APIGateway.Core.Auth.BasicAuth
{
    internal interface IUserService
    {
        bool IsValidUser(string username, string password, string scope);
    }

    /// <summary>
    ///     Load user name and password from config
    /// </summary>
    public class UserServiceFromConfig : IUserService
    {
        private readonly BasicAuthConfig _config;

        public UserServiceFromConfig(IOptions<BasicAuthConfig> config)
        {
            _config = config.Value;
        }

        public bool IsValidUser(string username, string password, string scope)
        {
            var exist = _config.Users.Any(u =>
                u.Password.Equals(password) && u.Username.Equals(username) && u.Scopes.Contains(scope));

            if (exist)
                return true;
            return false;
        }
    }
}