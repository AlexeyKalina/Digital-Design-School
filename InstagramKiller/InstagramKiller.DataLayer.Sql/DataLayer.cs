﻿using System;
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
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetGuid(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2)
                            };
                        }
                        else
                        {
                            return null;
                        }
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
                        if (reader.Read())
                        {
                            return new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3)
                            };
                        }
                        else
                        {
                            return null;
                        }
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
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        public void DeleteUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM comments WHERE user_id = @id OR post_id IN (SELECT id FROM posts WHERE user_id = @id);
                                            DELETE FROM hashtags_posts WHERE post_id IN (SELECT id FROM posts WHERE user_id = @id);
                                            DELETE FROM likes WHERE user_id = @id OR post_id IN (SELECT id FROM posts WHERE user_id = @id);
                                            DELETE FROM posts WHERE user_id = @id;
                                            DELETE FROM users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeletePost(Post post)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM comments WHERE post_id = @id;
                                            DELETE FROM hashtags_posts WHERE post_id = @id;
                                            DELETE FROM likes WHERE post_id = @id;
                                            DELETE FROM posts WHERE id = @id;";
                    command.Parameters.AddWithValue("@id", post.Id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteComment(Comment comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM comments WHERE id = @id";
                    command.Parameters.AddWithValue("@id", comment.Id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<Comment> GetPostComments(Post post)
        {
            List<Comment> comments = new List<Comment>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, text, user_id, post_id, date FROM comments WHERE post_id = @id";
                    command.Parameters.AddWithValue("@id", post.Id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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
            List<Post> posts = new List<Post>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TOP(@count) id, photo, date, user_id FROM posts ORDER BY date DESC";
                    command.Parameters.AddWithValue("@count", count);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            posts.Add(new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3)
                            });
                        }
                        return posts;
                    }
                }
            }
        }
        public List<Post> FindPostsByHashtag(string hashtag)
        {
            throw new NotImplementedException();
        }
        public void AddLikeToPost(User user, Post post)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO likes (user_id, post_id) VALUES (@user_id, @post_id)";
                    command.Parameters.AddWithValue("@user_id", user.Id);
                    command.Parameters.AddWithValue("@post_id", post.Id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<User> GetPostLikes(Post post)
        {
            List<User> users = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, login, password FROM users WHERE id IN (SELECT user_id FROM likes WHERE post_id = @id)";
                    command.Parameters.AddWithValue("@id", post.Id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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
    }
}