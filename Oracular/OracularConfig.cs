﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;

using Oracular.Spec;

namespace Oracular
{
	public class OracularConfig
	{
		public static OracularConfig Deserialize(string source)
		{
			var deserialized = JsonValue.Parse (source);
			var rootObject = deserialized as JsonObject;

			if (rootObject == null)
			{
				throw new OracularException ("unable to deserialize configuration");
			}

			var tables = new List<OracularTable> ();
			var specs = new List<OracularSpec> ();

			if (rootObject.ContainsKey("tables"))
			{
				foreach (var table in rootObject["tables"])
				{
					var tableObject = table as JsonObject;
					if (tableObject == null)
					{
						throw new OracularException ("error deserializing table");
					}

					var fields = new List<FieldConfig> ();
					var parents = new List<ParentConfig> ();

					foreach (var field in tableObject["fields"])
					{
						var fieldObject = field as JsonObject;
						if (fieldObject == null)
						{
							var message = String.Format ("error deserializing field on table {0}", (string)tableObject ["table"]);
							throw new OracularException (message);
						}

						var type = FieldType.String;

						if (fieldObject.ContainsKey("type"))
						{
							switch ((string)fieldObject["type"])
							{
							case "number":
								type = FieldType.Number;
								break;
							case "boolean":
							case "bool": // deprecated
								type = FieldType.Boolean;
								break;
							case "date":
								type = FieldType.Date;
								break;
							}
						}

						fields.Add(new FieldConfig ((string)fieldObject["name"], type));
					}

					if (tableObject.ContainsKey("parents"))
					{
						foreach (var parent in tableObject["parents"])
						{
							var parentObject = parent as JsonObject;
							if (parentObject == null)
							{
								var message = String.Format ("error deserializing parent on table {0}", (string)tableObject ["table"]);
								throw new OracularException (message);
							}

							var parentName = (string)parentObject ["name"];
							var parentTable = parentObject.ContainsKey ("table") ? (string)parentObject ["table"] : parentName;
							var parentId = parentObject.ContainsKey ("id") ? (string)parentObject ["id"] : parentName + "Id";

							parents.Add (new ParentConfig (parentName, parentTable, parentId));
						}
					}

					var id = tableObject.ContainsKey ("id") ? (string)tableObject ["id"] : "Id";

					tables.Add(new OracularTable ((string)tableObject["table"], id, parents, fields));
				}
			}

			if (rootObject.ContainsKey("specs"))
			{
				foreach (var spec in rootObject["specs"])
				{
					var specObject = spec as JsonObject;
					if (specObject == null)
					{
						throw new OracularException ("error deserializing spec");
					}

					specs.Add (new OracularSpec ((string)specObject["name"], (string)specObject["table"], (string)specObject["spec"]));
				}
			}

			return new OracularConfig (tables, specs);
		}

		public static OracularConfig LoadFromFile(string path)
		{
			string source;
			try
			{
				source = File.ReadAllText(path);
			}
			catch (Exception ex)
			{
				var message = String.Format("Unable to read source file: {0}", path);
				throw new OracularException (message);
			}

			return Deserialize(source);
		}

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

			foreach (var spec in this.specs.Values)
			{
				spec.config = this;
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

		public void Check()
		{
			var typechecker = new TypeChecker (this);
			foreach (var spec in specs.Values)
			{
				spec.Spec.Walk (typechecker);
			}

			var refchecker = new RefChecker (this);
			foreach (var spec in specs.Values)
			{
				spec.Spec.Walk (refchecker, new [] { spec.Table });
			}
		}

		public IEnumerable<OracularTable> Tables => tables.Values;
		public IEnumerable<OracularSpec> Specs => specs.Values;
	}
}

