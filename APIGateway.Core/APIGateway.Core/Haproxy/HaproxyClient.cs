using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace APIGateway.Core.Haproxy
{
    public class HaproxyClient : IHaproxyClient
    {
        private readonly HaproxyClientOption _option;

        public HaproxyClient(IOptions<HaproxyClientOption> option)
        {
            _option = option.Value;
        }

        public async Task<List<ServerStatus>> GetServerStatus()
        {
            var client = new RestClient(_option.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(_option.Username, _option.Password);
            var request = new RestRequest(";csv", Method.GET);
            var csvFile = await client.ExecuteAsync(request);

            if (!csvFile.IsSuccessful)
                throw csvFile.ErrorException;

            var result = new List<ServerStatus>();
            var rows = csvFile.Content.Split('\n');

            var columnHeader = rows[0].Split(',');
            for (var i = 1; i < rows.Count(); i++)
            {
                var columns = rows[i].Split(',');
                if (columns.Length > 17)
                {
                    var message = "";
                    for (var x = 0; x < columns.Length; x++)
                        message += $"{columnHeader[x]}: {columns[x]}, ";

                    result.Add(new ServerStatus
                    {
                        Name = columns[0] + " " + columns[1],
                        Status = columns[17],
                        Message = message
                    });
                }
            }

            return result;
        }
    }

    public class ServerStatus
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}