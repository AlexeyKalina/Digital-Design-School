using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramKiller.Model;
using System.Data.SqlClient;
using NLog;
using InstagramKiller.Log;

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
            if (user.Login.Length > 12)
                throw new ArgumentException("Login is not valid");
            if (user.Password.Length > 12)
                throw new ArgumentException("Password is not valid");
            if (UserWithLoginExist(user.Login))
                throw new ArgumentException("User with this login already exists");

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
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetGuid(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2)
                            };
                        }
                        throw new ArgumentException("User with id not exists");
                    }
                }
            }
        }

        public Post AddPost(Post post)
        {
            if (post.Hashtags.Any(h => h.Length > 12))
                throw new ArgumentException("Hashtags are not valid");

            GetUser(post.UserId);

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

                    AddHashtagsToPost(post);
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
                        if (reader.Read())
                        {
                            Guid id = reader.GetGuid(0);
                            return new Post
                            {
                                Id = id,
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3),
                                Hashtags = GetHashtags(id)
                            };
                        }
                        throw new ArgumentException("Post with id not exists");
                    }
                }
            }
        }

        public Comment AddCommentToPost(Comment comment, Guid postId)
        {
            if (comment.Text.Length > 50)
                throw new ArgumentException("Text is not valid");

            Post post = GetPost(postId);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    comment.Id = Guid.NewGuid();
                    comment.Date = DateTime.Now;
                    comment.PostId = post.Id;

                    command.CommandText = "INSERT INTO comments (id, text, user_id, post_id, date) VALUES (@id, @text, @user_id, @post_id, @date)";
                    command.Parameters.AddWithValue("@id", comment.Id);
                    command.Parameters.AddWithValue("@text", comment.Text);
                    command.Parameters.AddWithValue("@user_id", comment.UserId);
                    command.Parameters.AddWithValue("@post_id", post.Id);
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
                        if (reader.Read())
                        {
                            return new Comment
                            {
                                Id = reader.GetGuid(0),
                                Text = reader.GetString(1),
                                UserId = reader.GetGuid(2),
                                PostId = reader.GetGuid(3),
                                Date = reader.GetDateTime(4)
                            };
                        }
                        throw new ArgumentException("Comment with id not exists");
                    }
                }
            }
        }

        public void DeleteUser(Guid userId)
        {
            GetUser(userId);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"BEGIN TRANSACTION
                                                DELETE FROM comments WHERE user_id = @id;
                                                DELETE FROM likes WHERE user_id = @id;
                                                DELETE FROM users WHERE id = @id
                                            COMMIT";
                    command.Parameters.AddWithValue("@id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePost(Guid postId)
        {
            GetPost(postId);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM posts WHERE id = @id;";
                    command.Parameters.AddWithValue("@id", postId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteComment(Guid commentId)
        {
            GetComment(commentId);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM comments WHERE id = @id";
                    command.Parameters.AddWithValue("@id", commentId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Comment> GetPostComments(Guid postId)
        {
            GetPost(postId);

            var comments = new List<Comment>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, text, user_id, post_id, date FROM comments WHERE post_id = @id";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comments.Add(new Comment
                            {
                                Id = reader.GetGuid(0),
                                Text = reader.GetString(1),
                                UserId = reader.GetGuid(2),
                                PostId = reader.GetGuid(3),
                                Date = reader.GetDateTime(4)
                            });
                        }
                        return comments;
                    }
                }
            }
        }

        public List<Post> GetLatestPosts(int count)
        {
            var posts = new List<Post>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TOP(@count) id, photo, date, user_id FROM posts ORDER BY date DESC";
                    command.Parameters.AddWithValue("@count", count);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            posts.Add(new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3),
                                Hashtags = GetHashtags(reader.GetGuid(0))
                            });
                        }
                        return posts;
                    }
                }
            }
        }

        public List<Post> FindPostsByHashtag(string hashtag)
        {
            var posts = new List<Post>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT posts.id, photo, posts.date, posts.user_id FROM posts JOIN hashtags_posts ON posts.id = hashtags_posts.post_id
                                            JOIN hashtags ON hashtags_posts.hashtag_id = hashtags.id WHERE text = @text";
                    command.Parameters.AddWithValue("@text", hashtag);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            posts.Add(new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3),
                                Hashtags = GetHashtags(reader.GetGuid(0))
                            });
                        }
                        return posts;
                    }
                }
            }
        }

        public void AddLikeToPost(Guid userId, Guid postId)
        {
            GetUser(userId);

            GetPost(postId);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM likes WHERE user_id = @user_id AND post_id = @post_id";
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@post_id", postId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            throw new ArgumentException("This like already exists");
                        }
                    }
                    command.CommandText = "INSERT INTO likes (user_id, post_id) VALUES (@user2_id, @post2_id)";
                    command.Parameters.AddWithValue("@user2_id", userId);
                    command.Parameters.AddWithValue("@post2_id", postId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<User> GetPostLikes(Guid postId)
        {
            GetPost(postId);

            var users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, login, password FROM users JOIN likes ON users.id = likes.user_id WHERE post_id = @id;";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetGuid(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2)
                            });
                        }
                        return users;
                    }
                }
            }
        }
        public void DeleteLikeFromPost(Guid userId, Guid postId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM likes WHERE post_id = @post_id AND user_id = @user_id;";
                    command.Parameters.AddWithValue("@post_id", postId);
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddHashtagsToPost(Post post)
        {
            bool hashtagExist = false;
            Guid id = Guid.NewGuid();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    for (int counter = 0; counter < post.Hashtags.Count; counter++)
                    {
                        command.CommandText = "SELECT id FROM hashtags WHERE text = @hashtag;";
                        command.Parameters.AddWithValue("@hashtag", post.Hashtags[counter]);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hashtagExist = true;
                                id = reader.GetGuid(0);
                            }

                        }
                        if (hashtagExist)
                        {
                            command.CommandText = @"INSERT INTO hashtags_posts (hashtag_id, post_id) VALUES (@hashtag_id, @post_id);";
                            command.Parameters.AddWithValue("@hashtag_id", id);
                            command.Parameters.AddWithValue("@post_id", post.Id);
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            command.CommandText = @"BEGIN TRANSACTION
                                                    INSERT INTO hashtags (id, text) VALUES (@hashtag_id, @text)
                                                    INSERT INTO hashtags_posts (hashtag_id, post_id) VALUES (@hashtag_id, @post_id)
                                                COMMIT";
                            command.Parameters.AddWithValue("@hashtag_id", Guid.NewGuid());
                            command.Parameters.AddWithValue("@text", post.Hashtags[counter]);
                            command.Parameters.AddWithValue("@post_id", post.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private List<string> GetHashtags(Guid postId)
        {
            var hashtags = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT text FROM hashtags JOIN hashtags_posts ON hashtags.id = hashtags_posts.hashtag_id WHERE post_id = @id;";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hashtags.Add(reader.GetString(0));
                        }
                        return hashtags;
                    }
                }
            }
        }

        private bool UserWithLoginExist(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, login, password FROM users WHERE login = @login";
                    command.Parameters.AddWithValue("@login", login);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return true;
                        else
                            return false;
                    }
                }
            }
        }
    }
}
