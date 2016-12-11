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
        public User GetUserById(Guid id)
        {
            var result = _client.GetAsync(string.Format("{0}/api/users/{1}", _connectionString, id)).Result;
            return result.Content.ReadAsAsync<User>().Result;
        }
        public List<Post> GetLatestPosts(int count=10)
        {
            var result = _client.GetAsync(string.Format("{0}/api/posts/latest/{1}", _connectionString, count)).Result;
            return result.Content.ReadAsAsync<List<Post>>().Result;
        }
        public List<Post> FindPostsByHashtag(string hashtag)
        {
            var result = _client.GetAsync(string.Format("{0}/api/posts/search/{1}", _connectionString, hashtag)).Result;
            return result.Content.ReadAsAsync<List<Post>>().Result;
        }
    }
}
