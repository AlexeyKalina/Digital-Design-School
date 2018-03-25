using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace InstagramKiller.Cache
{
	public class RedisCacheProvider : ICacheProvider
	{
		private readonly ConnectionMultiplexer _redisConnection;
		public RedisCacheProvider(string connectionString)
		{
			_redisConnection = ConnectionMultiplexer.Connect(connectionString);
		}

		public bool TryGet<T>(string key, out T result)
		{
			result = default(T);
			var db = GetRedisDb();

			if (!db.KeyExists(key))
				return false;

			result = DeserializeFromJson<T>(db.StringGet(key));
			return true;
		}

		public bool TryGetGuidList(string key, int index, out List<Guid> result)
		{
			result = null;
			var db = GetRedisDb();

			if (!db.KeyExists(key))
				return false;

			var redisList = db.ListRange(key, 0, index);

			result = redisList.Select(val => new Guid(val.ToString())).ToList();

			return true;
		}

		public void AddGuidListAndTrim(string key, int index, params Guid[] list)
		{
			var db = GetRedisDb();
			list = list.Reverse().ToArray();
			foreach (var value in list)
			{
				db.ListLeftPush(key, value.ToString());
			}
			db.ListTrim(key, 0, index);
		}

		public bool TryGetSet(string key, out List<string> result)
		{
			var db = GetRedisDb();
			result = null;

			if (!db.KeyExists(key))
				return false;

			var redisValues = db.SetMembers(key);
			result = redisValues.Select(val => val.ToString()).ToList();
			return true;
		}

		public void AddSet(string key, List<string> values)
		{
			var db = GetRedisDb();
			db.SetAdd(key, values.Select(val => (RedisValue)val).ToArray());
		}


		public void Add<T>(string key, T value)
		{
			var db = GetRedisDb();
			var redisValue = SerializeToJson(value);
			db.StringSet(key, redisValue);
		}

		public void Delete(string key)
		{
			var db = GetRedisDb();
			db.KeyDelete(key);
		}
		private IDatabase GetRedisDb()
		{
			return _redisConnection.GetDatabase();
		}

		private string SerializeToJson<T>(T value)
		{
			return JsonConvert.SerializeObject(value, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});
		}

		private T DeserializeFromJson<T>(string str)
		{
			return JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
		}
	}
}
