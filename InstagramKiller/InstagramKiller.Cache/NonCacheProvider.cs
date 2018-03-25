using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramKiller.Cache
{
	public class NonCacheProvider : ICacheProvider
	{
		public bool TryGet<T>(string key, out T result)
		{
			result = default(T);
			return false;
		}

		public void Add<T>(string key, T value)
		{

		}

		public void Delete(string key)
		{

		}

		public bool TryGetGuidList(string key, int index, out List<Guid> result)
		{
			result = null;
			return false;
		}

		public void AddGuidListAndTrim(string key, int index, params Guid[] list)
		{

		}

		public bool TryGetSet(string key, out List<string> result)
		{
			result = null;
			return false;
		}

		public void AddSet(string key, List<string> result)
		{

		}
	}
}
