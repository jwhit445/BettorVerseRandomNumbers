using System;
using System.Runtime;
using System.Threading.Tasks;
using DiscordRandomNumbers.Models;
using DiscordRandomNumbers.Storage;
using Microsoft.Extensions.Options;

namespace DiscordRandomNumbers.Startup {
	public class ConfigStartupTask : IStartupTask {

		private BotSettings _botSettings { get; }
        public Settings _settings { get; }

        public ConfigStartupTask(IOptions<BotSettings> botSettings, Settings settings) {
			_botSettings = botSettings.Value;
			_settings = settings;
		}

		public Task StartupAsync() {
			_settings.ListAdminRoleIDs = _botSettings.AdminRoleIDs;
			return Task.CompletedTask;
		}
	}
}
