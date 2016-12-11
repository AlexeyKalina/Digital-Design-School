using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using InstagramKiller.Model;

namespace InstagramKiller.WebApi.Controllers
{
    public class PostsController : ApiController
    {
        private const string ConnectionString = @"Data Source = DESKTOP-2VEAELC; Initial Catalog = InstagramKiller; Integrated Security = true";
        private readonly IDataLayer _dataLayer;

        public PostsController()
        {
            _dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
        }

        /// <summary>
        /// Add new post
        /// </summary>
        /// <param name="post">post</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/posts")]
        public Post CreatePost(Post post)
        {
            return _dataLayer.AddPost(post);
        }

        /// <summary>
        /// Get post by id
        /// </summary>
        /// <param name="id">post id</param>
        [HttpGet]
        [Route("api/posts/{id}")]
        public Post GetPost(Guid id)
        {
            return _dataLayer.GetPost(id);
        }

        /// <summary>
        /// Delete post by id
        /// </summary>
        /// <param name="id">post id</param>
        [HttpDelete]
        [Route("api/posts/{id}")]
        public void DeletePost(Guid id)
        {
            _dataLayer.DeletePost(id);
        }

        /// <summary>
        /// Add new comment to post
        /// </summary>
        /// <param name="comment">comment</param>
        /// <param name="postId">post id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/posts/{postId}/comments")]
        public Comment AddCommentToPost(Comment comment, Guid postId)
        {
            return _dataLayer.AddCommentToPost(comment, postId);
        }

        /// <summary>
        /// Get comment by id
        /// </summary>
        /// <param name="id">comment id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/posts/comments/{id}")]
        public Comment GetComment(Guid id)
        {
            return _dataLayer.GetComment(id);
        }

        /// <summary>
        /// Delete comment by id
        /// </summary>
        /// <param name="id">comment id</param>
        [HttpDelete]
        [Route("api/posts/comments/{id}")]
        public void DeleteComment(Guid id)
        {
            _dataLayer.DeleteComment(id);
        }

        /// <summary>
        /// Get post's comments by post id
        /// </summary>
        /// <param name="postId">post id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/posts/{postId}/comments")]
        public List<Comment> GetPostComments(Guid postId)
        {
            return _dataLayer.GetPostComments(postId);
        }

        /// <summary>
        /// Get latest posts
        /// </summary>
        /// <param name="count">count of posts</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/posts/latest/{count}")]
        public List<Post> GetLatestPosts(int count)
        {
            return _dataLayer.GetLatestPosts(count);
        }

        /// <summary>
        /// Find posts by hashtag
        /// </summary>
        /// <param name="hashtag">hashtag</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/posts/search/{hashtag}")]
        public List<Post> FindPostsByHashtag(string hashtag)
        {
            return _dataLayer.FindPostsByHashtag(hashtag);
        }

        /// <summary>
        /// Add like to post
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="postId">post id</param>
        [HttpPost]
        [Route("api/posts/{postId}/likes")]
        public void AddLikeToPost(Guid userId, Guid postId)
        {
            _dataLayer.AddLikeToPost(userId, postId);
        }

        /// <summary>
        /// Get likes from post by id
        /// </summary>
        /// <param name="postId">post id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/posts/{postId}/likes")]
        public List<User> GetPostLikes(Guid postId)
        {
            return _dataLayer.GetPostLikes(postId);
        }

        /// <summary>
        /// Delete like from post
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="postId">post id</param>
        [HttpDelete]
        [Route("api/posts/{postId}/likes")]
        void DeleteLikeFromPost(Guid userId, Guid postId)
        {
            _dataLayer.DeleteLikeFromPost(userId, postId);
        }
    }
}
