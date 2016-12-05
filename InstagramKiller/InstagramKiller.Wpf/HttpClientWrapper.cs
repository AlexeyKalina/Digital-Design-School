using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using InstagramKiller.Model;
using Newtonsoft.Json;

namespace InstagramKiller.Wpf
{
    public class HttpClientWrapper
    {
        private readonly string _connectionString;
        private readonly HttpClient _client;

        public HttpClientWrapper(string connectionString)
        {
            _connectionString = connectionString;
            _client = new HttpClient
            {
                BaseAddress = new Uri(connectionString)
            };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void AddPost(Post post)
        {
            _client.PostAsJsonAsync(string.Format("{0}/api/posts", _connectionString), post);
        }
    }
}
