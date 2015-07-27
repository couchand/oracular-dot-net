using System;
using System.Collections.Generic;

namespace Oracular
{
	public static class Builtins
	{
		private static readonly Dictionary<string, string> builtins = new Dictionary<string, string>
		{
			{ "any", "any" }
		};

		public static bool Contains(string name)
		{
			return builtins.ContainsKey (name.ToLower());
		}
	}
}

