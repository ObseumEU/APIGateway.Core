using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIGateway.Core.Haproxy
{
    public interface IHaproxyClient
    {
        Task<List<ServerStatus>> GetServerStatus();
    }
}