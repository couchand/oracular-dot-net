using System;
using System.Collections.Generic;
using System.Linq;

namespace Oracular
{
	public class OracularConfig
	{
		private Dictionary<string, OracularTable> tables;
		private Dictionary<string, OracularSpec> specs;

		public OracularConfig (IEnumerable<OracularTable> tables, IEnumerable<OracularSpec> specs)
		{
			try
			{
				this.tables = tables.ToDictionary (t => t.Table);
			}
			catch (ArgumentException ex)
			{
				throw new OracularException ("duplicate table " + ex.ParamName);
			}

			try
			{
				this.specs = specs.ToDictionary (s => s.Name);
			}
			catch (ArgumentException ex)
			{
				throw new OracularException ("duplicate spec " + ex.ParamName);
			}
		}

		public OracularTable GetTable(string name)
		{
			if (!tables.ContainsKey(name))
			{
				return null;
			}

			return tables [name];
		}

		public OracularSpec GetSpec(string name)
		{
			if (!specs.ContainsKey(name))
			{
				return null;
			}

			return specs [name];
		}

		public IEnumerable<OracularTable> Tables => tables.Values;
		public IEnumerable<OracularSpec> Specs => specs.Values;
	}
}

