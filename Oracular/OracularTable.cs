using System;
using System.Collections.Generic;
using System.Linq;

namespace Oracular
{
	public enum FieldType
	{
		Boolean,
		Number,
		String,
		Date
	}

	public class ParentConfig
	{
		public readonly string Name;
		public readonly string Table;
		public readonly string Id;

		public ParentConfig(string name, string table, string id)
		{
			if (name == null)
			{
				throw new OracularException ("parent config requires name");
			}
			this.Name = name;

			this.Table = table ?? name;
			this.Id = id  ?? Name + "Id";
		}
	}

	public class FieldConfig
	{
		public readonly string Name;
		public readonly FieldType Type;

		public FieldConfig(string name, FieldType? type)
		{
			if (name == null)
			{
				throw new OracularException ("field config requires name");
			}
			this.Name = name;

			this.Type = type.HasValue ? type.Value : FieldType.String;
		}
	}

	public class OracularTable
	{
		private readonly IEnumerable<ParentConfig> parents;
		private readonly IEnumerable<FieldConfig> fields;

		public OracularTable (string table, string id, IEnumerable<ParentConfig> parents, IEnumerable<FieldConfig> fields)
		{
			if (table == null)
			{
				throw new OracularException("table config requires name");
			}
			this.Table = table;

			this.Id = id ?? "Id";

			if (fields == null)
			{
				throw new OracularException ("table config requires at least one field");
			}
			this.fields = fields;

			var fieldsByName = new Dictionary<string, FieldConfig> ();
			foreach (var field in this.fields)
			{
				if (fieldsByName.ContainsKey (field.Name))
				{
					throw new OracularException ("cannot add duplicate fields");
				}
				fieldsByName [field.Name] = field;
			}

			this.parents = parents ?? new List<ParentConfig> ();

			if (!fieldsByName.ContainsKey(this.Id))
			{
				throw new OracularException ("table id field not found");
			}

			var parentsWithoutId = this.parents.Where (p => !fieldsByName.ContainsKey (p.Id));
			if (parentsWithoutId.Count() != 0)
			{
				throw new OracularException ("table parent id not found: " + String.Join(", ", parentsWithoutId.Select(p => p.Id)));
			}

			var parentsCollidingNames = this.parents.Where (p => fieldsByName.ContainsKey (p.Name));
			if (parentsCollidingNames.Count() != 0)
			{
				throw new OracularException ("table parent relationship collides with field: " + String.Join(", ", parentsCollidingNames.Select(p => p.Name)));
			}
		}

		public FieldConfig GetField(string fieldName)
		{
			var field = fields.FirstOrDefault (f => f.Name == fieldName);
			return field;
		}

		public ParentConfig GetParent(string parentName)
		{
			var parent = parents.FirstOrDefault(p => p.Name == parentName);
			return parent;
		}

		public ParentConfig GetRelationshipTo(string parentTable)
		{
			var parent = parents.FirstOrDefault (p => p.Table == parentTable);
			return parent;
		}

		public readonly string Table;
		public readonly string Id;

		public IEnumerable<ParentConfig> Parents => new List<ParentConfig>(parents);
		public IEnumerable<FieldConfig> Fields => new List<FieldConfig>(fields);
	}
}

