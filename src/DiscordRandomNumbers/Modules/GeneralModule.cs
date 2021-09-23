using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Reflection;
using System.Linq;
using DiscordRandomNumbers.Models;
using Microsoft.Extensions.Options;
using DiscordRandomNumbers.Storage;
using DiscordRandomNumbers.Modules.Preconditions;
using System.Data;
using System.Collections.Generic;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using Core;

namespace DiscordRandomNumbers.Modules {

    public class GeneralModule : ModuleBase<SocketCommandContext> {
		private const string COMMANDS_COMMAND = "commands";
		private const string RAND_MULTI_COMMAND = "randmulti";
		private const string NUM_WINNERS_COMMAND = "numberOfWinners";
		private readonly Random _rand = new();

		private BotSettings _botSettings { get; }
		public Settings _settings { get; }

		public GeneralModule(IOptions<BotSettings> botSettings, Settings settings) {
			_botSettings = botSettings.Value;
			_settings = settings;
		}

		[Command(COMMANDS_COMMAND)]
		[Summary("Displays the list of all available commands that can be used.")]
		public async Task ListCommands() {
			var methods = Assembly.GetEntryAssembly().GetTypes()
					  .SelectMany(t => t.GetMethods())
					  .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0 && m.GetCustomAttributes(typeof(SummaryAttribute), false).Length > 0)
					  .Where(m => (Context.User is IGuildUser guildUser && guildUser.RoleIds.Any(x => _settings.ListAdminRoleIDs.ToHashSet().Contains(x))) || (m.GetCustomAttributes(typeof(RequireAdminAttribute), false).Length == 0 && m.GetCustomAttributes(typeof(RequireOwnerAttribute), false).Length == 0))
					  .Select(x => $"`{_botSettings.Prefix}{((CommandAttribute)x.GetCustomAttribute(typeof(CommandAttribute), false)).Text}{ (x.GetParameters().Length == 0 ? "" : " { " + string.Join(" }, { ",x.GetParameters().Select(x => x.Name)) + " }") }`: {((SummaryAttribute)x.GetCustomAttribute(typeof(SummaryAttribute), false)).Text}")
					  .ToArray();
            await ReplyAsync(string.Join("\n", methods));
		}


		[Command(RAND_MULTI_COMMAND)]
		[RequireAdmin]
		[Summary("Returns random Token IDs in a range for the number of requested winners.")]
		public async Task RandomMulti(int lowerRange, int upperRange, int numberOfRuns) {
			Console.WriteLine("Running RandomMulti...");
			EmbedBuilder embed = new EmbedBuilder();
			embed.Color = Color.Green;
			embed.Title = "Random Token Winners";
			embed.Description = $"The following { numberOfRuns } Token IDs have been selected:\n\n";
			HashSet<int> usedNumbers = new();
			var client = new HttpClient();
			int randNum = 0;
			for (int i = 0; i < numberOfRuns; i++) {
				do {
					randNum = _rand.Next(lowerRange, upperRange + 1);
				}
				while (usedNumbers.Contains(randNum));
				usedNumbers.Add(randNum);
				var username = "";
				await ExUtils.SwallowAnyExceptionAsync(async () => {
					var openseaAssetResponse = await client.GetAsync($"https://api.opensea.io/api/v1/asset/0x378bc723ab7c5445fc2756aa17ff469a544f653c/{ randNum }/");
					var jsonString = await openseaAssetResponse.Content.ReadAsStringAsync();
					dynamic resObj = JsonConvert.DeserializeObject<dynamic>(jsonString);
					// Format the username to work with the hyperlink description. That way if something goes wrong here it will be empty and everything will look normal-ish
					string usr = resObj.owner.user == null ? "" : (resObj.owner.user.username ?? "");
					username = " - " + (string.IsNullOrWhiteSpace(usr) ? ((string)resObj.owner.address).Substring(2, 6) : usr);
				});
				
				embed.Description += $"[#{ randNum }{ username }](https://opensea.io/assets/0x378bc723ab7c5445fc2756aa17ff469a544f653c/{ randNum })\n";
				if(i > 0 && i % 10 == 0) {
					await ReplyAsync(embed: embed.Build());
					embed = new EmbedBuilder();
					embed.Color = Color.Green;
					embed.Description = "";
				}
			}
			await ReplyAsync(embed: embed.Build());
		}

		[Command(NUM_WINNERS_COMMAND)]
		[RequireAdmin]
		[Summary("Returns a random number of winners to be selected for giveaways.")]
		public async Task PickNumberOfWinners(int lowerRange, int upperRange) {
			Console.WriteLine("Running PickNumberOfWinners...");
			int randNum = _rand.Next(lowerRange, upperRange + 1);
			EmbedBuilder embed = new EmbedBuilder();
			embed.Color = Color.Green;
			embed.Title = "Number of Winners";
			embed.Description = $"The number of winners for tonight’s giveaway is { randNum }";
			await ReplyAsync(embed: embed.Build());
		}
	}
}
