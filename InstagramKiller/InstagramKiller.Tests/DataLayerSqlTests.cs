using System;
using System.Linq;
using InstagramKiller.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace InstagramKiller.Tests
{
    [TestClass]
    public class DataLayerSqlTests
    {
        private const string ConnectionString = @"Data Source = DESKTOP-2VEAELC; Initial Catalog = InstagramKiller; Integrated Security = true";

        private User _firstUser = new User
        {
            Id = Guid.Empty,
            Login = "firstUser",
            Password = "test"
        };
        private Post _firstPost = new Post
        {
            Id = Guid.Empty,
            UserId = Guid.Empty,
            Photo = new byte[10],
            Hashtags = new List<string> { "testHashtag" }
        };
        private Comment _firstComment = new Comment
        {
            Id = Guid.Empty,
            PostId = Guid.Empty,
            UserId = Guid.Empty,
            Text = "test comment"
        };

        [TestMethod]
        public void ShouldAddUser()
        {
            //arrange
            var user = new User
            {
                Login = Guid.NewGuid().ToString().Substring(0, 10),
                Password = Guid.NewGuid().ToString().Substring(0, 10)
            };
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            //act
            user = dataLayer.AddUser(user);

            //asserts
            var resultUser = dataLayer.GetUser(user.Id);
            Assert.AreEqual(user.Login, resultUser.Login);
        }

        [TestMethod]
        public void ShouldAddPost()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var post = new Post
            {
                UserId = _firstUser.Id,
                Photo = new byte[10],
                Hashtags = new List<string> { Guid.NewGuid().ToString().Substring(0, 10) }
            };

            //act
            post = dataLayer.AddPost(post);

            //asserts
            var resultPost = dataLayer.GetPost(post.Id);
            Assert.AreEqual(post.UserId, resultPost.UserId);
        }

        [TestMethod]
        public void ShouldAddComment()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var comment = new Comment
            {
                UserId = _firstUser.Id,
                Text = Guid.NewGuid().ToString()
            };

            //act
            comment = dataLayer.AddCommentToPost(comment, _firstPost.Id);

            //asserts
            var resultComment = dataLayer.GetComment(comment.Id);
            Assert.AreEqual(comment.Text, resultComment.Text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "User with id not exists")]
        public void ShouldDeleteUser()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var user = new User
            {
                Login = "delete user",
                Password = "test"
            };
            user = dataLayer.AddUser(user);

            //act
            dataLayer.DeleteUser(user.Id);

            //asserts
            var resultUser = dataLayer.GetUser(user.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Post with id not exists")]
        public void ShouldDeletePost()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var post = new Post
            {
                UserId = _firstUser.Id,
                Photo = new byte[10],
                Hashtags = new List<string> { "delete test" }
            };
            post = dataLayer.AddPost(post);

            //act
            dataLayer.DeletePost(post.Id);

            //asserts
            var resultPost = dataLayer.GetPost(post.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Comment with id not exists")]
        public void ShouldDeleteComment()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var comment = new Comment
            {
                UserId = _firstUser.Id,
                Text = "delete test"
            };
            comment = dataLayer.AddCommentToPost(comment, _firstPost.Id);

            //act
            dataLayer.DeleteComment(comment.Id);
            //asserts
            var resultComment = dataLayer.GetComment(comment.Id);
        }

        [TestMethod]
        public void ShouldGetPostComments()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            //act
            var comments = dataLayer.GetPostComments(_firstPost.Id);

            //asserts
            Assert.AreEqual(comments.Any(comment => comment.Id == _firstComment.Id), true);
        }

        [TestMethod]
        public void ShouldGetLatestPosts()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var post = new Post
            {
                UserId = _firstUser.Id,
                Photo = new byte[10],
                Hashtags = new List<string> { Guid.NewGuid().ToString().Substring(0, 10) }
            };
            dataLayer.AddPost(post);

            //act
            var posts = dataLayer.GetLatestPosts(5);

            //asserts
            Assert.AreEqual(posts[0].Id, post.Id);
        }

        [TestMethod]
        public void ShouldFindPostsByHashtag()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            string hashtag = _firstPost.Hashtags[0];

            //act
            var posts = dataLayer.FindPostsByHashtag(hashtag);

            //asserts
            Assert.AreEqual(posts.All(post => post.Hashtags.Contains(hashtag)), true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "This like already exists")]
        public void ShouldAddLikeToPost()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            //act
            dataLayer.AddLikeToPost(_firstUser.Id, _firstPost.Id);

            //asserts
            var likes = dataLayer.GetPostLikes(_firstPost.Id);
            Assert.AreEqual(likes.Any(user => user.Id == _firstUser.Id), true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "This like already exists")]
        public void ShouldDeleteLikeFromPost()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            dataLayer.AddLikeToPost(_firstUser.Id, _firstPost.Id);

            //act
            dataLayer.DeleteLikeFromPost(_firstUser.Id, _firstPost.Id);

            //asserts
            var likes = dataLayer.GetPostLikes(_firstPost.Id);
            Assert.AreEqual(likes.Any(user => user.Id == _firstUser.Id), false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "User with this login already exists")]
        public void ShouldAddUserWithExistingLoginNegative()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);

            var user = new User()
            {
                Id = Guid.NewGuid(),
                Login = "firstUser",
                Password = "jdnadnadn"
            };

            //act
            dataLayer.AddUser(user);

            //asserts
        }
    }
}
