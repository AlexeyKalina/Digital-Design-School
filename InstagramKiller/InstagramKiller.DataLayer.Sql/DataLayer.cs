using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramKiller.Model;

namespace InstagramKiller.DataLayer.Sql
{
    public class DataLayer : IDataLayer
    {
        private readonly string _connectionString;
        public DataLayer(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
        }
        public Comment AddComment(Comment comment)
        {
            throw new NotImplementedException();
        }

        public bool AddLikeToPost(User user, Post post)
        {
            throw new NotImplementedException();
        }

        public Post AddPost(Post post)
        {
            throw new NotImplementedException();
        }

        public User AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public bool DeleteComment(Comment comment)
        {
            throw new NotImplementedException();
        }

        public bool DeletePost(Post post)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Post[] FindPostsByHashtag(string hashtag)
        {
            throw new NotImplementedException();
        }

        public Comment GetComment(Guid commentId)
        {
            throw new NotImplementedException();
        }

        public Post[] GetLatestPosts(int count)
        {
            throw new NotImplementedException();
        }

        public Post GetPost(Guid postId)
        {
            throw new NotImplementedException();
        }

        public Comment[] GetPostComments(Post post)
        {
            throw new NotImplementedException();
        }

        public User[] GetPostLikes(Post post)
        {
            throw new NotImplementedException();
        }

        public User GetUser(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
