using System;
using System.Collections.Generic;

namespace DiscordRandomNumbers.Storage {
	public class Settings {

		public List<ulong> ListAdminRoleIDs { get; set; } = new();

		public int SummonCost { get; set; }

		public int SalvageAmount { get; set; }
		public int MessageShardRewardAmount { get; set; }

	}
}
