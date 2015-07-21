using System;

namespace Oracular
{
	public class OracularException : ApplicationException
	{
		public OracularException (string message)
			: base(message) {}
	}
}

