using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramKiller.Model
{
    public interface IDataLayer
    {
        User AddUser(User user);
        User GetUser(Guid id);
        void DeleteUser(Guid userId);
        Post AddPost(Post post);
        Post GetPost(Guid postId);
        void DeletePost(Guid postId);
        Comment AddCommentToPost(Comment comment, Guid postId);
        Comment GetComment(Guid commentId);
        void DeleteComment(Guid commentId);
        List<Comment> GetPostComments(Guid postId);
        List<Post> GetLatestPosts(int count);
        List<Post> FindPostsByHashtag(string hashtag);
        void AddLikeToPost(Guid userId, Guid postId);
        List<User> GetPostLikes(Guid postId);
        void DeleteLikeFromPost(Guid userId, Guid postId);
    }
}
