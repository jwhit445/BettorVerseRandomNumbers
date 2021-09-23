using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordRandomNumbers.Models {

	public class BotSettings {

		///<summary>The token used to authenticate the bot.</summary>
		public string Token { get; set; }

		///<summary>The prefix used for commands.</summary>
		public char Prefix { get; set; }
		public List<ulong> AdminRoleIDs { get; set; }

	}

}
