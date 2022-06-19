using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Runtime.Caching;

namespace FileWatcherService
{

    public interface IWatcherService
    {
        void Watch(Action<string> notifier, string folder, string file);
    }
    public class WatcherService: IWatcherService
    {
        #region avoid raising duplicating events
        //it may possible that when the file changed system raised several events
        //We are using caching logic which prevent raising the same event more time during last 1 second
        private readonly MemoryCache _memCache;
        private readonly CacheItemPolicy _cacheItemPolicy;
        //avoid raising more than one event during 500ms
        private const int CacheTimeMilliseconds = 500;
        #endregion
        private readonly ILogger<WatcherService> _logger;
        public WatcherService(ILogger<WatcherService> logger)
        {
            this._logger = logger;
            _memCache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy()
            {
                RemovedCallback = OnRemovedFromCache
            };
        }

        // Handle cache item expiring 
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            var file = (string)args.CacheItem.Value;
            _logger.LogInformation("Check removing from cache {file} on {FullPath}", file, args.CacheItem.Key);
            if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;
            _logger.LogInformation("Now we can respond to the event {file} on {FullPath}", file, args.CacheItem.Key);
        }

        public void Watch(Action<string> notifier, string folder, string file)
        {
            var physicalFileProvider = new PhysicalFileProvider(folder);
            ChangeToken.OnChange(() => { return physicalFileProvider.Watch(file); }, () => { notifier($"{file} changed in {folder}"); Reload(folder, file); });
        }

        private void Reload(string folder, string file)
        {
            _cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);
            _memCache.AddOrGetExisting(folder, file, _cacheItemPolicy);
        }
    }
}