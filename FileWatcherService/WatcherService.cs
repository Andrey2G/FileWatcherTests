using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FileWatcherService
{

    public interface IWatcherService
    {
        void Watch(Action<string> notifier, string folder, string file);
    }
    public class WatcherService: IWatcherService
    {
        private readonly ILogger<WatcherService> _logger;
        public WatcherService(ILogger<WatcherService> logger)
        {
            this._logger = logger;
        }

        public void Watch(Action<string> notifier, string folder, string file)
        {
            var physicalFileProvider = new PhysicalFileProvider(folder);
            ChangeToken.OnChange(()=> { return physicalFileProvider.Watch(file); }, ()=>notifier($"{file} changed in {folder}"));
        }
    }
}