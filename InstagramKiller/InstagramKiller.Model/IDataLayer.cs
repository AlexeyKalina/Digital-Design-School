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
        bool DeleteUser(User user);
        Post AddPost(Post post);
        Post GetPost(Guid postId);
        bool DeletePost(Post post);
        Comment AddComment(Comment comment);
        Comment GetComment(Guid commentId);
        bool DeleteComment(Comment comment);
        Comment[] GetPostComments(Post post);
        Post[] GetLatestPosts(int count);
        Post[] FindPostsByHashtag(string hashtag);
        bool AddLikeToPost(User user, Post post);
        User[] GetPostLikes(Post post);
    }
}
