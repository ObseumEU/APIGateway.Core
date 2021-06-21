using System.Collections.Generic;

namespace APIGateway.Core.Auth.BasicAuth
{
    public class BasicAuthConfig
    {
        public List<UserConfig> Users { get; set; }
    }

    public class UserConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string[] Scopes { get; set; }
    }
}