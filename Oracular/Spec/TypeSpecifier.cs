using System;

namespace Oracular.Spec
{
	// poor-man's polymorphism
	public enum SpecType
	{
		Any,		// only null literals

		Boolean,
		Number,
		String,
		Date,

		Table		// base type for table specifiers
	}

	public class TypeSpecifier
	{
		public readonly SpecType Type;
		public readonly string Table;

		public TypeSpecifier (SpecType type)
		{
			if (type == SpecType.Table)
			{
				throw new OracularException ("table type specifier requires a table name");
			}

			this.Type = type;
		}

		public TypeSpecifier (string table)
		{
			if (table == null || table.Length == 0)
			{
				throw new OracularException ("table name invalid");
			}

			this.Type = SpecType.Table;
			this.Table = table;
		}

		public override string ToString()
		{
			return Type == SpecType.Table ? Table : Type.ToString ();
		}

		public TypeSpecifier Coalesce(TypeSpecifier other)
		{
			if (Type == SpecType.Any)
			{
				if (other.Type == SpecType.Any)
				{
					// both any, return any
					return this;
				}

				// this is any, defer to other
				return other;
			}

			if (Type != other.Type)
			{
				// distinct types, invalid
				return null;
			}

			if (Type == SpecType.Table)
			{
				// both table types, check table names
				return Table == other.Table ? this : null;
			}

			// same type
			return this;
		}

		public static readonly TypeSpecifier Any     = new TypeSpecifier(SpecType.Any);
		public static readonly TypeSpecifier Boolean = new TypeSpecifier(SpecType.Boolean);
		public static readonly TypeSpecifier Number  = new TypeSpecifier(SpecType.Number);
		public static readonly TypeSpecifier String  = new TypeSpecifier(SpecType.String);
		public static readonly TypeSpecifier Date    = new TypeSpecifier(SpecType.Date);

		public static TypeSpecifier GetTable(string table)
		{
			return new TypeSpecifier (table);
		}
	}
}

