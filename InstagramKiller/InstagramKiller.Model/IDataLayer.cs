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
        void DeleteUser(User user);
        Post AddPost(Post post);
        Post GetPost(Guid postId);
        void DeletePost(Post post);
        Comment AddComment(Comment comment);
        Comment GetComment(Guid commentId);
        void DeleteComment(Comment comment);
        List<Comment> GetPostComments(Post post);
        List<Post> GetLatestPosts(int count);
        List<Post> FindPostsByHashtag(string hashtag);
        void AddLikeToPost(User user, Post post);
        List<User> GetPostLikes(Post post);
    }
}
