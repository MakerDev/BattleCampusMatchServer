using BattleCampus.Core;
using BattleCampus.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.User.Query
{
    public class PortalLogin
    {
        public const string LOGIN_ADDRESS = "https://ysweb.yonsei.ac.kr/ysbus_main.jsp";

        public class Query : IRequest<bool>
        {
            public string UserId { get; set; }
            public string Password { get; set; }
        }

        public class Hander : IRequestHandler<Query, bool>
        {
            private readonly HttpClient _httpClient;
            public Hander(IHttpClientFactory httpClientFactory)
            {
                _httpClient = httpClientFactory.CreateClient();
            }

            public async Task<bool> Handle(Query request, CancellationToken cancellationToken)
            {
                var data = new Dictionary<string, string>();
                data.Add("userid", request.UserId);
                data.Add("password", request.Password);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, LOGIN_ADDRESS)
                {
                    Content = new FormUrlEncodedContent(data),
                };

                var httpResponse = await _httpClient.SendAsync(httpRequest);

                if (httpResponse.IsSuccessStatusCode == false)
                {
                    return false;
                }

                var responseText = await httpResponse.Content.ReadAsStringAsync();

                if (responseText.Contains("<TITLE>Yonsei Bus Login chk</TITLE>"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

