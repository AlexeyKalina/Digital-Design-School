using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramKiller.Model;
using System.Data.SqlClient;

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
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    user.Id = Guid.NewGuid();
                    command.CommandText = "INSERT INTO users (id, login, password) VALUES (@id, @login, @password)";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@login", user.Login);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.ExecuteNonQuery();
                    return user;
                }
            }
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
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, login, password FROM users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return new User
                        {
                            Id = reader.GetGuid(0),
                            Login = reader.GetString(1),
                            Password = reader.GetString(2)
                        };
                    }
                }
            }
        }
    }
}
