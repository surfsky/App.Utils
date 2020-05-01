using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Caching.Distributed;

namespace App.Utils.Base
{
    /// <summary>
    /// 缓存处理（支持内存、Redis、文件）
    /// </summary>
    public class Cacher
    {
        static MemoryCache Cache = new MemoryCache(new MemoryCacheOptions() { });
        static RedisCache RedisCache;

        /// <summary>使用Redis作为缓存</summary>
        public static void UseRedis(string ip, string password, string instanceName)
        {
            RedisCache = new RedisCache(new RedisCacheOptions()
            {
                Configuration = $"{ip},password={password}",
                InstanceName = $"{instanceName}"
            });
        }

        /// <summary>从缓存中获取数据。若该缓存失效，则自动从创建对象并塞入缓存</summary>
        public static T GetCache<T>(string key, DateTime? expiredTime, Func<T> func) where T : class
        {
            if (RedisCache != null)
                return GetRedisCache(key, expiredTime, func);
            return GetMemoryCache(key, expiredTime, func);
        }


        /// <summary>从本机内存缓存中获取数据。若该缓存失效，则自动从创建对象并塞入缓存</summary>
        public static T GetMemoryCache<T>(string key, DateTime? expiredTime, Func<T> func) where T : class
        {
            if (!Cache.TryGetValue(key, out object o))
            {
                o = func();
                if (expiredTime == null)
                    Cache.Set(key, o);
                else
                    Cache.Set(key, o, expiredTime.Value - DateTime.Now);
            }
            return o as T;
        }

        /// <summary>从Redis获取缓存数据（未测试）</summary>
        /// <remarks>
        /// Redis存储的是byte[]
        /// 现阶段用以下方式转化为对象：bytes -> json -> object
        /// 以后尝试直接用 jsonb 方式： bytes -> object
        /// </remarks>
        public static T GetRedisCache<T>(string key, DateTime? expiredTime, Func<T> func) where T: class
        {
            var bytes = RedisCache.Get(key);
            if (bytes == null || bytes.Length == 0)
            {
                bytes = func().ToJson().ToBytes();
                RedisCache.Set(key, bytes, new DistributedCacheEntryOptions()
                {
                     AbsoluteExpiration = expiredTime
                });
            }
            var o = bytes.ToString(Encoding.UTF8).ToJson().Parse<T>();
            return o;
        }

        /// <summary>从文件中获取缓存，若文件变更，自动刷新缓存（未测试）</summary>
        public static string GetFileCache(string fileName)
        {
            if (Cache.TryGetValue(fileName, out string txt))
            {
                var fileInfo = new FileInfo(fileName);
                txt = File.ReadAllText(fileName);
                var cacheEntityOps = new MemoryCacheEntryOptions();
                cacheEntityOps.AddExpirationToken(new PollingFileChangeToken(fileInfo)); // 监控文件变化
                cacheEntityOps.RegisterPostEvictionCallback((key, value, reason, state) => { Console.WriteLine($"文件 {key} 改动了"); }); // 缓存失效时处理
                Cache.Set(fileInfo.Name, txt, cacheEntityOps);
            }
            return txt;
        }



    }
}
