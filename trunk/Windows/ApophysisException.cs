﻿using System;
using System.Runtime.Serialization;

namespace Xyrus.Apophysis.Windows
{
	[Serializable]
	public class ApophysisException : Exception
	{
		public ApophysisException() { }
		public ApophysisException(string message) : base(message) { }
		public ApophysisException(string message, Exception innerException) : base(message, innerException) { }

		protected ApophysisException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
