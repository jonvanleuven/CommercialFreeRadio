using System;
using System.Collections.Concurrent;
using System.Linq;

namespace CommercialFreeRadio.Impl
{
    public class TimeSpanCache
    {
        private const string DefaultKey = "default";
        private readonly ConcurrentDictionary<object, CachedData> data = new ConcurrentDictionary<object, CachedData>();
        private readonly TimeSpan duration;

        public TimeSpanCache(TimeSpan duration)
        {
            this.duration = duration;
        }

        public T ReadCached<T>(Func<T> readCallback)
        {
            return ReadCachedInternal(DefaultKey, readCallback);
        }

        public T ReadCached<T>(string key, Func<T> readCallback)
        {
            return ReadCachedInternal(key, readCallback);
        }

        public T ReadCached<T, T1>(Tuple<T1> key, Func<T> readCallback)
        {
            return ReadCachedInternal(key, readCallback);
        }

        public T ReadCached<T, T1, T2>(Tuple<T1, T2> key, Func<T> readCallback)
        {
            return ReadCachedInternal(key, readCallback);
        }

        public T ReadCached<T, T1, T2, T3>(Tuple<T1, T2, T3> key, Func<T> readCallback)
        {
            return ReadCachedInternal(key, readCallback);
        }

        public T ReadCached<T, T1, T2, T3, T4>(Tuple<T1, T2, T3, T4> key, Func<T> readCallback)
        {
            return ReadCachedInternal(key, readCallback);
        }

        public T ReadCached<T, T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5> key, Func<T> readCallback)
        {
            return ReadCachedInternal(key, readCallback);
        }

        public int Count
        {
            get { return data.Count; }
        }

        public void Clear()
        {
            data.Clear();
        }

        public void SetExpireTime(DateTime time)
        {
            foreach (var entry in data)
            {
                entry.Value.ExpireTimeTicks = time.Ticks;
            }
        }

        private T ReadCachedInternal<T>(object key, Func<T> readCallback)
        {
            var result = GetAddOrUpdate<T>(key, readCallback);
            foreach (var expired in data.Where(e => IsExpired(e.Value)))
            {
                CachedData removed;
                data.TryRemove(expired.Key, out removed);
            }
            return (T)result.Value;
        }

        private CachedData GetAddOrUpdate<T>(object key, Func<T> call)
        {
            Func<object, CachedData> factory = _ => new CachedData { ExpireTimeTicks = (DateTime.Now + duration).Ticks, Value = call() };
            var entry = data.GetOrAdd(key, factory);
            return IsExpired(entry)
                ? data.AddOrUpdate(key, factory, (a, b) => factory(a))
                : entry;
        }

        private static bool IsExpired(CachedData entry)
        {
            return DateTime.Now.Ticks >= entry.ExpireTimeTicks;
        }

        private class CachedData
        {
            public long ExpireTimeTicks { get; set; }
            public object Value { get; set; }
        }
    }

    public static class TimeSpanCacheExtensions
    {
        public static Func<T1, TResult> Cache<T1, TResult>(this Func<T1, TResult> func, TimeSpanCache cache)
        {
            return arg => cache.ReadCached(Tuple.Create(arg), () => func(arg));
        }

        public static Func<T1, T2, TResult> Cache<T1, T2, TResult>(this Func<T1, T2, TResult> func, TimeSpanCache cache)
        {
            return (arg1, arg2) => cache.ReadCached(Tuple.Create(arg1, arg2), () => func(arg1, arg2));
        }

        public static Func<T1, T2, T3, TResult> Cache<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, TimeSpanCache cache)
        {
            return (arg1, arg2, arg3) => cache.ReadCached(Tuple.Create(arg1, arg2, arg3), () => func(arg1, arg2, arg3));
        }

        public static Func<T1, T2, T3, T4, TResult> Cache<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, TimeSpanCache cache)
        {
            return (arg1, arg2, arg3, arg4) => cache.ReadCached(Tuple.Create(arg1, arg2, arg3, arg4), () => func(arg1, arg2, arg3, arg4));
        }

        public static Func<T1, T2, T3, T4, T5, TResult> Cache<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> func, TimeSpanCache cache)
        {
            return (arg1, arg2, arg3, arg4, arg5) => cache.ReadCached(Tuple.Create(arg1, arg2, arg3, arg4, arg5), () => func(arg1, arg2, arg3, arg4, arg5));
        }
    }
}
