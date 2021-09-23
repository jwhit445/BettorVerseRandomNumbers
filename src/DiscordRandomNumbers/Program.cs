using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DiscordRandomNumbers.Modules;
using DiscordRandomNumbers.Services;
using DiscordRandomNumbers.Worker;
using DiscordRandomNumbers.Models;
using DiscordRandomNumbers.Startup;
using DiscordRandomNumbers.Storage;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace DiscordRandomNumbers {
    public class Program {

        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host
				.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    IConfiguration config = hostContext.Configuration;

                    services
                        .Configure<BotSettings>(config.GetSection(nameof(BotSettings)));

                    services
                        .AddSingleton<IAmazonDynamoDB>(sp => {
                            return new AmazonDynamoDBClient(new AmazonDynamoDBConfig() {
                                MaxErrorRetry = 20,
                                ThrottleRetries = false
                            });
                        })
                        .AddSingleton<IDynamoDBContext>(sp => {
                            var client = sp.GetRequiredService<IAmazonDynamoDB>();
                            return new DynamoDBContext(client, new DynamoDBContextConfig {
                                Conversion = DynamoDBEntryConversion.V2,

                            });
                        })
                        .AddSingleton<SetupModule>()
                        .AddSingleton<LoggingService>()
                        .AddSingleton<Settings>()
                        .AddSingleton<IStartupTask, ConfigStartupTask>()
                        .AddSingleton(serviceProvider => {
                            return new CommandService(new CommandServiceConfig {
                                LogLevel = LogSeverity.Info,
                                CaseSensitiveCommands = false,
                            });
                        })
                        .AddSingleton(sp => new DiscordSocketClient(new DiscordSocketConfig {
                            AlwaysDownloadUsers = true
                        }))
                        .AddSingleton<BaseSocketClient, DiscordSocketClient>(sp => {
                            return sp.GetRequiredService<DiscordSocketClient>();
                        })
                        .AddSingleton(_ => new CommandService(new CommandServiceConfig() {
                            DefaultRunMode = RunMode.Async,
                        }))
                        .AddHostedService<MasterWorker>();
                });
        }

    }
}
