using System.Threading.Tasks;

namespace DiscordRandomNumbers.Startup {
	public interface IStartupTask {
		Task StartupAsync();
	}
}
