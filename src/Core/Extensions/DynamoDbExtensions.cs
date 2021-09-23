using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace Core.Extensions {
    public static class DynamoDbExtensions {

		///<summary>Returns the enums description if there is one. Otherwise, returns enum.ToString().</summary>
		public static async Task<List<T>> GetAll<T>(this IDynamoDBContext context, string pk) {
			List<T> retVal = new List<T>();
			var search = context.QueryAsync<T>(pk);
			do {
				retVal.AddRange(await search.GetNextSetAsync());
			}
			while (!search.IsDone);
			return retVal;
		}
	}
}
