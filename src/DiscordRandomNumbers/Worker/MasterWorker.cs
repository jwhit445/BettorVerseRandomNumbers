using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using DiscordRandomNumbers.Services;
using System.Collections.Generic;
using DiscordRandomNumbers.Startup;
using DiscordRandomNumbers.Models;

namespace DiscordRandomNumbers.Worker {
	public class MasterWorker : BackgroundService {
		
		private BotSettings _botSettings { get; }
		private DiscordSocketClient _discordClient { get; }
		private CommandService _commandService { get; }
		private IServiceProvider _serviceProvider { get; }
		private LoggingService _loggingService { get; }
		private IEnumerable<IStartupTask> _startupTasks { get; }

		public MasterWorker(IOptions<BotSettings> botSettings, DiscordSocketClient discordClient, CommandService commandService, IServiceProvider serviceProvider, 
			LoggingService loggingService, IEnumerable<IStartupTask> startupTasks) 
		{
			_botSettings = botSettings.Value;
			_discordClient = discordClient;
			_commandService = commandService;
			_serviceProvider = serviceProvider;
			_loggingService = loggingService;
			_startupTasks = startupTasks;
		}

		public override async Task StartAsync(CancellationToken cancellationToken) {
			await base.StartAsync(cancellationToken);
			_loggingService.Init();
			_commandService.CommandExecuted += CommandExecutedAsync;
			_discordClient.MessageReceived += OnMessageReceived;
			foreach(IStartupTask startupTask in _startupTasks) {
				await startupTask.StartupAsync();
			}

			await _discordClient.LoginAsync(TokenType.Bot, _botSettings.Token);
			await _discordClient.StartAsync();

			await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			await ExUtils.SwallowAnyExceptionAsync(async () => {
				await Task.Delay(Timeout.Infinite, stoppingToken);
			});
		}

		public override async Task StopAsync(CancellationToken cancellationToken) {
			await base.StopAsync(cancellationToken);
		}

		private async Task OnMessageReceived(SocketMessage arg) {
			if(arg is not SocketUserMessage message) {
				return;
			}
			int argPos = 0;
			if(message.Author.IsBot) {
				return;
            }
			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if(message.HasCharPrefix(_botSettings.Prefix, ref argPos)) {
				SocketCommandContext context = new SocketCommandContext(_discordClient, message);
				await _commandService.ExecuteAsync(context: context, argPos: argPos, services: _serviceProvider);
				return;
			}
		}

		public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
			if(!command.IsSpecified || result.IsSuccess) {
				return;
			}

			//he command failed, let's notify the user that something happened.
			await context.Channel.SendMessageAsync(result.ErrorReason);
		}
	}
}
