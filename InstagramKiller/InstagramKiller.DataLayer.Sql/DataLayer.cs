using System;
using System.Collections.Generic;
using System.Linq;
using InstagramKiller.Model;
using System.Data.SqlClient;
using InstagramKiller.Utils;

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
            Logs.logger.Debug("Старт метода AddUser");
            Logs.logger.Debug("Проверяем валидность данных добавляемого пользователя");
            if (user.Login.Length > 12)
            {
                Logs.logger.Error("Невалидный логин. Логин {0} имеет длину {1}, которая больше 12", user.Login, user.Login.Length);
                throw new ArgumentException("Login is not valid");
            }
            if (user.Password.Length > 12)
            {
                Logs.logger.Error("Невалидный пароль. Пароль {0} имеет длину {1}, которая больше 12", user.Password, user.Password.Length);
                throw new ArgumentException("Password is not valid");
            }
            if (UserWithLoginExist(user.Login))
            {
                Logs.logger.Error("Невалидный логин. Логин {0} уже существует в системе.", user.Login);
                throw new ArgumentException("User with this login already exists");
            }
            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем запрос на добавление пользователя в базу данных");
                    user.Id = Guid.NewGuid();
                    command.CommandText = "INSERT INTO users (id, login, password) VALUES (@id, @login, @password)";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@login", user.Login);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Пользователь добавлен: id = {0}, login = {1}, password = {2}", user.Id, user.Login, user.Password);
                    Logs.logger.Debug("Выход из метода AddUser");
                    return user;
                }
            }
        }

        public User GetUser(Guid id)
        {
            Logs.logger.Debug("Старт метода GetUser с id = {0}", id);
            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на пользователя в базу данных");
                    command.CommandText = "SELECT id, login, password FROM users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Logs.logger.Info("Пользователь получен: id = {0}, login = {1}, password = {2}", id, reader.GetString(1), reader.GetString(2));
                            Logs.logger.Debug("Выход из метода GetUser");
                            return new User
                            {
                                Id = reader.GetGuid(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2)
                            };
                        }
                        Logs.logger.Error("Пользователь с id = {0} отсутствует в системе", id);
                        throw new ArgumentException("User with id not exists");
                    }
                }
            }
        }

        public Post AddPost(Post post)
        {
            Logs.logger.Debug("Старт метода AddPost");
            Logs.logger.Debug("Проверяем валидность данных добавляемого поста");
            if (post.Hashtags.Any(h => h.Length > 12))
            {
                Logs.logger.Debug("Невалидный список хэштегов. Содержится хэштег {0}, имеющий длину больше 12", post.Hashtags.Find(h => h.Length > 12));
                throw new ArgumentException("Hashtags are not valid");
            }
            Logs.logger.Debug("Проверяем наличие пользователя, к которому добавляем пост");
            GetUser(post.UserId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    post.Id = Guid.NewGuid();
                    post.Date = DateTime.Now;

                    Logs.logger.Debug("Делаем запрос на добавление поста в базу данных");
                    command.CommandText = "INSERT INTO posts (id, photo, date, user_id) VALUES (@id, @photo, @date, @user_id)";
                    command.Parameters.AddWithValue("@id", post.Id);
                    command.Parameters.AddWithValue("@photo", post.Photo);
                    command.Parameters.AddWithValue("@date", post.Date);
                    command.Parameters.AddWithValue("@user_id", post.UserId);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Пост добавлен: id = {0}, date = {1}, user_id = {2}", post.Id, post.Date, post.UserId);

                    AddHashtagsToPost(post);
                    Logs.logger.Info("Хэштеги добавлены к посту с id = {0}", post.Id);
                    Logs.logger.Debug("Выход из метода AddPost");
                    return post;
                }
            }
        }

        public Post GetPost(Guid postId)
        {
            Logs.logger.Debug("Старт метода GetPost c id = {0}", postId);
            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на пост в базу данных");
                    command.CommandText = "SELECT id, photo, date, user_id FROM posts WHERE id = @id";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            List<string> hashtags = GetHashtags(postId);
                            Logs.logger.Info("Пост получен: id = {0}, date = {1}, userId = {2}", postId, reader.GetDateTime(2), reader.GetGuid(3));
                            Logs.logger.Debug("Выход из метода GetPost");
                            return new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3),
                                Hashtags = hashtags
                            };
                        }
                        Logs.logger.Error("Пост с id = {0} отсутствует в системе", postId);
                        throw new ArgumentException("Post with id not exists");
                    }
                }
            }
        }

        public Comment AddCommentToPost(Comment comment, Guid postId)
        {
            Logs.logger.Debug("Старт метода AddCommentToPost с postId = {0}", postId);
            Logs.logger.Debug("Проверяем валидность коммента");
            if (comment.Text.Length > 50)
            {
                Logs.logger.Error("Невалидный текст комментария. Текст \"{0}\" имеет длину {1}, которая больше 50", comment.Text, comment.Text.Length);
                throw new ArgumentException("Text is not valid");
            }
            Logs.logger.Debug("Проверяем наличие поста, к которому добавляем комментарий");
            Post post = GetPost(postId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    comment.Id = Guid.NewGuid();
                    comment.Date = DateTime.Now;
                    comment.PostId = post.Id;

                    Logs.logger.Debug("Делаем запрос на добавление комментария в базу данных");
                    command.CommandText = "INSERT INTO comments (id, text, user_id, post_id, date) VALUES (@id, @text, @user_id, @post_id, @date)";
                    command.Parameters.AddWithValue("@id", comment.Id);
                    command.Parameters.AddWithValue("@text", comment.Text);
                    command.Parameters.AddWithValue("@user_id", comment.UserId);
                    command.Parameters.AddWithValue("@post_id", post.Id);
                    command.Parameters.AddWithValue("@date", comment.Date);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Комментарий добавлен: id = {0}, text = {1}, userId = {2}, postId = {3}, date = {4}", comment.Id, comment.Text, comment.UserId, comment.PostId, comment.Date);
                    Logs.logger.Debug("Выход из метода AddCommentToPost");
                    return comment;
                }
            }
        }

        public Comment GetComment(Guid commentId)
        {
            Logs.logger.Debug("Старт метода GetComment с id = {0}", commentId);
            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на комментарий в базу данных");
                    command.CommandText = "SELECT id, text, user_id, post_id, date FROM comments WHERE id = @id";
                    command.Parameters.AddWithValue("@id", commentId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Logs.logger.Info("Комментарий получен: id = {0}, text = {1}, userId = {2}, postId = {3}, date = {4}", reader.GetGuid(0), reader.GetString(1), reader.GetGuid(2), reader.GetGuid(3), reader.GetDateTime(4));
                            Logs.logger.Debug("Выход из метода GetComment");
                            return new Comment
                            {
                                Id = reader.GetGuid(0),
                                Text = reader.GetString(1),
                                UserId = reader.GetGuid(2),
                                PostId = reader.GetGuid(3),
                                Date = reader.GetDateTime(4)
                            };
                        }
                        Logs.logger.Error("Комментарий с id = {0} отсутствует в системе", commentId);
                        throw new ArgumentException("Comment with id not exists");
                    }
                }
            }
        }

        public void DeleteUser(Guid userId)
        {
            Logs.logger.Debug("Старт метода DeleteUser с id = {0}", userId);
            Logs.logger.Debug("Проверяем наличие пользователя в системе");
            GetUser(userId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Начинаем транзакцию на удаление пользователя с id = {0}, лайков и комментариев, которые он оставил", userId);
                    command.CommandText = @"BEGIN TRANSACTION
                                                DELETE FROM comments WHERE user_id = @id;
                                                DELETE FROM likes WHERE user_id = @id;
                                                DELETE FROM users WHERE id = @id
                                            COMMIT";
                    command.Parameters.AddWithValue("@id", userId);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Пользователь с id = {0} удален из системы", userId);
                    Logs.logger.Debug("Выход из метода DeleteUser");
                }
            }
        }

        public void DeletePost(Guid postId)
        {
            Logs.logger.Debug("Старт метода DeletePost с id = {0}", postId);
            Logs.logger.Debug("Проверяем наличие поста в системе");
            GetPost(postId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем запрос на удаление поста с id = {0} в базу данных", postId);
                    command.CommandText = @"DELETE FROM posts WHERE id = @id;";
                    command.Parameters.AddWithValue("@id", postId);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Пост с id = {0} удален из системы", postId);
                    Logs.logger.Debug("Выход из метода DeletePost");
                }
            }
        }

        public void DeleteComment(Guid commentId)
        {
            Logs.logger.Debug("Старт метода DeleteComment с id = {0}", commentId);
            Logs.logger.Debug("Проверяем наличие комментария в системе");
            GetComment(commentId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем запрос на удаление комментария с id = {0} в базу данных", commentId);
                    command.CommandText = "DELETE FROM comments WHERE id = @id";
                    command.Parameters.AddWithValue("@id", commentId);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Комментарий с id = {0} удален из системы", commentId);
                    Logs.logger.Debug("Выход из метода DeleteComment");
                }
            }
        }

        public List<Comment> GetPostComments(Guid postId)
        {
            Logs.logger.Debug("Старт метода GetPostComments с postId = {0}", postId);
            Logs.logger.Debug("Проверяем наличие поста, комментарии которого получаем");
            GetPost(postId);

            var comments = new List<Comment>();

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на комментарии в базу данных");
                    command.CommandText = "SELECT id, text, user_id, post_id, date FROM comments WHERE post_id = @id";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        Logs.logger.Debug("В цикле считываем комментарии");
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
                            Logs.logger.Info("Комментарий поста с id = {0} получен: id = {1}, text = {2}, userId = {3}, date = {4}", postId, reader.GetGuid(0), reader.GetString(1), reader.GetGuid(2), reader.GetDateTime(4));
                        }
                        Logs.logger.Debug("Выход из метода GetPostComment");
                        return comments;
                    }
                }
            }
        }

        public List<Post> GetLatestPosts(int count = 5)
        {
            Logs.logger.Debug("Старт метода GetLatestPosts с количеством постов = {0}", count);
            var posts = new List<Post>();

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на посты в базу данных");
                    command.CommandText = "SELECT TOP(@count) id, photo, date, user_id FROM posts ORDER BY date DESC";
                    command.Parameters.AddWithValue("@count", count);

                    using (var reader = command.ExecuteReader())
                    {
                        Logs.logger.Debug("В цикле считываем посты");
                        while (reader.Read())
                        {
                            List<string> hashtags = GetHashtags(reader.GetGuid(0));
                            Logs.logger.Info("Пост получен: id = {0}, date = {1}, userId = {2}", reader.GetGuid(0), reader.GetDateTime(2), reader.GetGuid(3));
                            posts.Add(new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3),
                                Hashtags = hashtags
                            });
                        }
                        Logs.logger.Debug("Выход из метода GetLatestPosts");
                        return posts;
                    }
                }
            }
        }

        public List<Post> FindPostsByHashtag(string hashtag)
        {
            Logs.logger.Debug("Старт метода FindPostsByHashtag с hashtag = {0}", hashtag);
            var posts = new List<Post>();

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на посты с хэштегом {0} в базу данных", hashtag);
                    command.CommandText = @"SELECT posts.id, photo, posts.date, posts.user_id FROM posts JOIN hashtags_posts ON posts.id = hashtags_posts.post_id
                                            JOIN hashtags ON hashtags_posts.hashtag_id = hashtags.id WHERE text = @text";
                    command.Parameters.AddWithValue("@text", hashtag);

                    using (var reader = command.ExecuteReader())
                    {
                        Logs.logger.Debug("В цикле считываем посты");
                        while (reader.Read())
                        {
                            List<string> hashtags = GetHashtags(reader.GetGuid(0));
                            Logs.logger.Info("Пост c хэштегом {3} получен: id = {0}, date = {1}, userId = {2}", reader.GetGuid(0), reader.GetDateTime(2), reader.GetGuid(3), hashtag);
                            posts.Add(new Post
                            {
                                Id = reader.GetGuid(0),
                                Photo = (byte[])reader["photo"],
                                Date = reader.GetDateTime(2),
                                UserId = reader.GetGuid(3),
                                Hashtags = hashtags
                            });
                        }
                        Logs.logger.Debug("Выход из метода FindPostsByHashtag");
                        return posts;
                    }
                }
            }
        }

        public void AddLikeToPost(Guid userId, Guid postId)
        {
            Logs.logger.Debug("Старт метода AddLikeToPost с userId = {0} и postId = {1}", userId, postId);
            Logs.logger.Debug("Проверяем наличе пользователя в системе");
            GetUser(userId);
            Logs.logger.Debug("Проверяем наличе поста в системе");
            GetPost(postId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Проверяем нет ли уже такого лайка в системе");
                    Logs.logger.Debug("Делаем SELECT запрос на лайк");
                    command.CommandText = @"SELECT * FROM likes WHERE user_id = @user_id AND post_id = @post_id";
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@post_id", postId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Logs.logger.Error("Лайк от пользователя с id = {0} к посту с id = {1} уже есть в системе", userId, postId);
                            throw new ArgumentException("This like already exists");
                        }
                        Logs.logger.Debug("Такого лайка нет");
                    }
                    Logs.logger.Debug("Делаем запрос на добавление лайка в систему");
                    command.CommandText = "INSERT INTO likes (user_id, post_id) VALUES (@user2_id, @post2_id)";
                    command.Parameters.AddWithValue("@user2_id", userId);
                    command.Parameters.AddWithValue("@post2_id", postId);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Лайк добавлен: userId = {0}, postId = {1}", userId, postId);
                    Logs.logger.Debug("Выход из метода AddLikeToPost");
                }
            }
        }

        public List<User> GetPostLikes(Guid postId)
        {
            Logs.logger.Debug("Старт метода GetPostLikes с postId = {0}", postId);
            Logs.logger.Debug("Проверяем наличе поста в системе");
            GetPost(postId);

            var users = new List<User>();

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на лайки поста с id = {0}", postId);
                    command.CommandText = "SELECT id, login, password FROM users JOIN likes ON users.id = likes.user_id WHERE post_id = @id;";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Logs.logger.Debug("В цикле считываем лайки");
                            Logs.logger.Info("Лайк поста с id = {0} получен: userId = {1}, login = {2}", postId, reader.GetGuid(0), reader.GetString(1));
                            users.Add(new User
                            {
                                Id = reader.GetGuid(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2)
                            });
                        }
                        Logs.logger.Debug("Выход из метода GetPostLikes");
                        return users;
                    }
                }
            }
        }
        public void DeleteLikeFromPost(Guid userId, Guid postId)
        {
            Logs.logger.Debug("Старт метода DeleteLikeFromPost с userId = {0} и postId = {1}", userId, postId);
            Logs.logger.Debug("Проверяем наличе пользователя в системе");
            GetUser(userId);
            Logs.logger.Debug("Проверяем наличе поста в системе");
            GetPost(postId);

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем запрос на удаление лайка пользователя с id = {0} к посту с id = {1} в базу данных", userId, postId);
                    command.CommandText = @"DELETE FROM likes WHERE post_id = @post_id AND user_id = @user_id;";
                    command.Parameters.AddWithValue("@post_id", postId);
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.ExecuteNonQuery();
                    Logs.logger.Info("Лайк от пользователя с id = {0} к посту с id = {1} удален", userId, postId);
                    Logs.logger.Debug("Выход из метода DeleteLikeFromPost");
                }
            }
        }

        private void AddHashtagsToPost(Post post)
        {
            Logs.logger.Debug("Старт метода AddHashtagsToPost");
            bool hashtagExist = false;
            Guid id = Guid.NewGuid();

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("В цикле обрабатываем хэштеги");
                    for (int counter = 0; counter < post.Hashtags.Count; counter++)
                    {
                        Logs.logger.Debug("Отправляем SELECT запрос по хэштегу с текстом {0} в базу данных", post.Hashtags[counter]);
                        command.CommandText = "SELECT id FROM hashtags WHERE text = @hashtag;";
                        command.Parameters.AddWithValue("@hashtag", post.Hashtags[counter]);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Logs.logger.Debug("Хэштег {0} содержится в системе, его id = {1}", post.Hashtags[counter], reader.GetGuid(0));
                                hashtagExist = true;
                                id = reader.GetGuid(0);
                            }
                            Logs.logger.Debug("Хэштега {0} в системе нет", post.Hashtags[counter]);
                        }
                        if (hashtagExist)
                        {
                            Logs.logger.Debug("Отправляем запрос на добавление привязки поста с id = {0} к хэштегу с текстом {1} в базу данных", post.Id, post.Hashtags[counter]);
                            command.CommandText = @"INSERT INTO hashtags_posts (hashtag_id, post_id) VALUES (@hashtag_id, @post_id);";
                            command.Parameters.AddWithValue("@hashtag_id", id);
                            command.Parameters.AddWithValue("@post_id", post.Id);
                            command.ExecuteNonQuery();
                            Logs.logger.Info("Хэштег {0} добавлен к посту с id = {1}", post.Hashtags[counter], post.Id);
                        }
                        else
                        {
                            Guid newId = Guid.NewGuid();
                            Logs.logger.Debug("Начинаем транзакцию на добавление хэштега {0} в систему и привязку его к посту с id = {1} в базе данных", post.Hashtags[counter], post.Id);
                            command.CommandText = @"BEGIN TRANSACTION
                                                    INSERT INTO hashtags (id, text) VALUES (@hashtag_id, @text)
                                                    INSERT INTO hashtags_posts (hashtag_id, post_id) VALUES (@hashtag_id, @post_id)
                                                COMMIT";
                            command.Parameters.AddWithValue("@hashtag_id", newId);
                            command.Parameters.AddWithValue("@text", post.Hashtags[counter]);
                            command.Parameters.AddWithValue("@post_id", post.Id);
                            command.ExecuteNonQuery();
                            Logs.logger.Info("Хэштег добавлен в систему: id = {0}, text = {1}", newId, post.Hashtags[counter]);
                            Logs.logger.Info("Хэштег {0} добавлен к посту с id = {1}", post.Hashtags[counter], post.Id);
                        }
                    }
                }
            }
            Logs.logger.Debug("Выход из метода AddHashtagsToPost");
        }

        private List<string> GetHashtags(Guid postId)
        {
            Logs.logger.Debug("Старт метода GetHashtags с postId = {0}", postId);
            var hashtags = new List<string>();

            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на хэштэги, привязанные к посту с id = {0}", postId);
                    command.CommandText = "SELECT text FROM hashtags JOIN hashtags_posts ON hashtags.id = hashtags_posts.hashtag_id WHERE post_id = @id;";
                    command.Parameters.AddWithValue("@id", postId);

                    using (var reader = command.ExecuteReader())
                    {
                        Logs.logger.Debug("В цикле считываем хэштеги");
                        while (reader.Read())
                        {
                            Logs.logger.Debug("Хэштег {0} получен", reader.GetString(0));
                            hashtags.Add(reader.GetString(0));
                        }
                        Logs.logger.Debug("Выход из метода GetHashtags");
                        return hashtags;
                    }
                }
            }
        }

        private bool UserWithLoginExist(string login)
        {
            Logs.logger.Debug("Старт метода UserWithLoginExist, проверяющего существует ли уже в системе пользователь с логином {0}", login);
            Logs.logger.Debug("Открываем подключение к базе данных");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    Logs.logger.Debug("Делаем SELECT запрос на пользователя в базу данных");
                    command.CommandText = "SELECT id, login, password FROM users WHERE login = @login";
                    command.Parameters.AddWithValue("@login", login);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Logs.logger.Debug("Пользователь с логином {0} существует в системе. Выход из метода UserWithLoginExist", login);
                            return true;
                        }
                        else
                        {
                            Logs.logger.Debug("Пользователя с логином {0} в системе нет. Выход из метода UserWithLoginExist", login);
                            return false;
                        }
                    }
                }
            }
        }
    }
}
