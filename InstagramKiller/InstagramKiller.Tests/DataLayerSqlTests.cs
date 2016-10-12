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

        private User _firstUser;
        private Post _firstPost;
        private Comment _firstComment;

        public void FirstData(DataLayer.Sql.DataLayer dataLayer)
        {
            _firstUser = new User
            {
                Login = "firstUser",
                Password = "test"
            };
            _firstUser = dataLayer.AddUser(_firstUser);
            _firstPost = new Post
            {
                UserId = _firstUser.Id,
                Photo = new byte[10],
                Hashtags = new List<string> { "testHashtag" }
            };
            _firstPost = dataLayer.AddPost(_firstPost);
            _firstComment = new Comment
            {
                PostId = _firstPost.Id,
                UserId = _firstUser.Id,
                Text = "test comment"
            };
            _firstComment = dataLayer.AddComment(_firstComment);
        }

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
            FirstData(dataLayer);

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
            FirstData(dataLayer);

            var comment = new Comment
            {
                PostId = _firstPost.Id,
                UserId = _firstUser.Id,
                Text = Guid.NewGuid().ToString()
            };
            //act
            comment = dataLayer.AddComment(comment);
            //asserts
            var resultComment = dataLayer.GetComment(comment.Id);
            Assert.AreEqual(comment.Text, resultComment.Text);
        }

        [TestMethod]
        public void ShouldDeleteUser()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            //act
            dataLayer.DeleteUser(_firstUser);
            //asserts
            var resultUser = dataLayer.GetUser(_firstUser.Id);
            Assert.AreEqual(resultUser, null);
        }

        [TestMethod]
        public void ShouldDeletePost()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            //act
            dataLayer.DeletePost(_firstPost);
            //asserts
            var resultPost = dataLayer.GetPost(_firstPost.Id);
            Assert.AreEqual(resultPost, null);
        }

        [TestMethod]
        public void ShouldDeleteComment()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            //act
            dataLayer.DeleteComment(_firstComment);
            //asserts
            var resultComment = dataLayer.GetComment(_firstComment.Id);
            Assert.AreEqual(resultComment, null);
        }

        [TestMethod]
        public void ShouldGetPostComments()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            //act
            List<Comment> comments = dataLayer.GetPostComments(_firstPost);
            //asserts
            Assert.AreEqual(comments.Any(comment => comment.Id == _firstComment.Id), true);
        }

        [TestMethod]
        public void ShouldGetLatestPosts()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            var post = new Post
            {
                UserId = _firstUser.Id,
                Photo = new byte[10],
                Hashtags = new List<string> { Guid.NewGuid().ToString().Substring(0, 10) }
            };
            dataLayer.AddPost(post);
            //act
            List<Post> posts = dataLayer.GetLatestPosts(5);
            //asserts
            Assert.AreEqual(posts[0].Id, post.Id);
        }

        [TestMethod]
        public void ShouldFindPostsByHashtag()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            string hashtag = _firstPost.Hashtags[0];
            //act
            List<Post> posts = dataLayer.FindPostsByHashtag(hashtag);
            //asserts
            Assert.AreEqual(posts.All(post => post.Hashtags.Contains(hashtag)), true);
        }

        [TestMethod]
        public void ShouldAddLikeToPost()
        {
            //arrange
            var dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
            FirstData(dataLayer);
            //act
            dataLayer.AddLikeToPost(_firstUser, _firstPost);
            //asserts
            List<User> likes = dataLayer.GetPostLikes(_firstPost);
            Assert.AreEqual(likes.Any(user => user.Id == _firstUser.Id), true);
        }
    }
}
