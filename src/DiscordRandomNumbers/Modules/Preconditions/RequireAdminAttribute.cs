using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;
using Discord;
using Discord.Commands;
using DiscordRandomNumbers.Storage;

namespace DiscordRandomNumbers.Modules.Preconditions {

	public class RequireAdminAttribute : RequireContextAttribute {


		public RequireAdminAttribute() : base(ContextType.Guild) {

		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			Settings settings = (Settings)services.GetService(typeof(Settings)) 
				?? throw new InvalidOperationException($"Missing required dependency type {nameof(Settings)}");
			HashSet<ulong> setAdminRoleIDs = settings.ListAdminRoleIDs.ToHashSet();
			if(setAdminRoleIDs.IsNullOrEmpty()) {
				return Task.FromResult(PreconditionResult.FromError("Administrative roles have not been set up by the server owner."));
			}

			if(context.User is not IGuildUser guildUser) {
				return Task.FromResult(PreconditionResult.FromError($"User is not an {nameof(IGuildUser)}"));
			}

			if(!guildUser.RoleIds.Any(x => setAdminRoleIDs.Contains(x))) {
				return Task.FromResult(PreconditionResult.FromError($"You do not have an administrative role."));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
