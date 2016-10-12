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
        public Post AddPost(Post post)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    post.Id = Guid.NewGuid();
                    post.Date = DateTime.Now;
                    command.CommandText = "INSERT INTO posts (id, photo, date, user_id) VALUES (@id, @photo, @date, @user_id)";
                    command.Parameters.AddWithValue("@id", post.Id);
                    command.Parameters.AddWithValue("@photo", post.Photo);
                    command.Parameters.AddWithValue("@date", post.Date);
                    command.Parameters.AddWithValue("@user_id", post.UserId);
                    command.ExecuteNonQuery();
                    return post;
                }
            }
        }
        public Post GetPost(Guid postId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, photo, date, user_id FROM posts WHERE id = @id";
                    command.Parameters.AddWithValue("@id", postId);
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return new Post
                        {
                            Id = reader.GetGuid(0),
                            Photo = (byte[])reader["photo"],
                            Date = reader.GetDateTime(2),
                            UserId = reader.GetGuid(3)
                        };
                    }
                }
            }
        }
        public Comment AddComment(Comment comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    comment.Id = Guid.NewGuid();
                    comment.Date = DateTime.Now;
                    command.CommandText = "INSERT INTO comments (id, text, user_id, post_id, date) VALUES (@id, @text, @user_id, @post_id, @date)";
                    command.Parameters.AddWithValue("@id", comment.Id);
                    command.Parameters.AddWithValue("@text", comment.Text);
                    command.Parameters.AddWithValue("@user_id", comment.UserId);
                    command.Parameters.AddWithValue("@post_id", comment.PostId);
                    command.Parameters.AddWithValue("@date", comment.Date);
                    command.ExecuteNonQuery();
                    return comment;
                }
            }
        }
        public Comment GetComment(Guid commentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, text, user_id, post_id, date FROM comments WHERE id = @id";
                    command.Parameters.AddWithValue("@id", commentId);
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return new Comment
                        {
                            Id = reader.GetGuid(0),
                            Text = reader.GetString(1),
                            UserId = reader.GetGuid(2),
                            PostId = reader.GetGuid(3),
                            Date = reader.GetDateTime(4)
                        };
                    }
                }
            }
        }
        public bool AddLikeToPost(User user, Post post)
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
        public Post[] GetLatestPosts(int count)
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
    }
}
