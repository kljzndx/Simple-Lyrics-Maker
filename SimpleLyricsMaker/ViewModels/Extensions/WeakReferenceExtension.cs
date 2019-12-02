using System;
using System.Threading.Tasks;

namespace SimpleLyricsMaker.ViewModels.Extensions
{
    public static class WeakReferenceExtension
    {
        public static async Task<T> GetTarget<T>(this WeakReference<T> wr, Func<Task<T>> objectFactory) where T : class
        {
            T obj = default(T);
            if (wr.TryGetTarget(out obj))
                return obj;
            else
            {
                obj = await objectFactory();
                wr.SetTarget(obj);
                return obj;
            }
        }
    }
}