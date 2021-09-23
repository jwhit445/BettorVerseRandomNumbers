using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Core.Extensions;
using Discord;
using Discord.Commands;
using DiscordRandomNumbers.Models;
using DiscordRandomNumbers.Modules.Preconditions;
using DiscordRandomNumbers.Storage;

namespace DiscordRandomNumbers.Modules {
	public class SetupModule : ModuleBase<SocketCommandContext> {

		private const string SET_ADMIN_ROLES_COMMAND = "setadminroles";

		public Settings _settings { get; }

        public SetupModule(Settings settings) {
			_settings = settings;
		}

		[Command(SET_ADMIN_ROLES_COMMAND)]
		[RequireOwner]
		[Summary("Sets up the roles that are considered admin. Must be performed by the server owner.")]
		public async Task SetAdminRoles(params IRole[] roles) {
			if(roles.IsNullOrEmpty()) {
				await ReplyAsync("Invalid roles.");
				return;
			}

			if(roles.Any(x => x.Id == x.Guild.EveryoneRole.Id)) {
				await ReplyAsync("@everyone role is not allowed.");
				return;
			}

			_settings.ListAdminRoleIDs = roles
				.Select(x => x.Id)
				.ToList();

			await ReplyAsync($"Set the following roles as admins: {string.Join(',',roles.Select(x => x.Name))}");
		}

	}

}
