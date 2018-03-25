using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramKiller.Cache
{
	public interface ICacheProvider
	{
		bool TryGet<T>(string key, out T result);

		void Add<T>(string key, T value);

		void Delete(string key);

		bool TryGetGuidList(string key, int index, out List<Guid> result);

		void AddGuidListAndTrim(string key, int index, params Guid[] list);

		bool TryGetSet(string key, out List<string> result);

		void AddSet(string key, List<string> result);
	}
}
