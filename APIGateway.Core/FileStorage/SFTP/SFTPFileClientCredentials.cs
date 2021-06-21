using System.Collections.Generic;
using System.Linq;

namespace APIGateway.Core.FileStorage.SFTP
{
    public class SFTPCredentials
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Endpoint { get; set; }
        public int Port { get; set; }
    }

    public class SFTPClientsConfig : List<SFTPCredentials>
    {
        public SFTPCredentials Get(string sftpName)
        {
            return this?.FirstOrDefault(c => c.Name.Equals(sftpName));
        }
    }
}