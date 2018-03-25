using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramKiller.Cache
{
	public static class Scheme
	{
		public static string Users(Guid id) => $"users:{id}";
		public static string Posts(Guid id) => $"posts:{id}";
		public static string Comments(Guid id) => $"comments:{id}";
		public static string Hashtags(Guid postId) => $"posts:{postId}:hashtags";
		public static string LatestPosts() => "posts:latest";
	}
}
