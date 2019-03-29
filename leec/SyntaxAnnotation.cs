using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class SyntaxAnnotation : IEquatable<SyntaxAnnotation>
	{
		/// <summary>
		/// A predefined syntax annotation that indicates whether the syntax element has elastic trivia.
		/// </summary>
		public static SyntaxAnnotation ElasticAnnotation { get; } = new SyntaxAnnotation();

		// use a value identity instead of object identity so a deserialized instance matches the original instance.
		private readonly long _id;
		private static long s_nextId;

		// use a value identity instead of object identity so a deserialized instance matches the original instance.
		public string Kind { get; }
		public string Data { get; }

		public SyntaxAnnotation()
		{
			_id = System.Threading.Interlocked.Increment(ref s_nextId);
		}

		public SyntaxAnnotation(string kind)
			: this()
		{
			this.Kind = kind;
		}

		public SyntaxAnnotation(string kind, string data)
			: this(kind)
		{
			this.Data = data;
		}

		private string GetDebuggerDisplay()
		{
			return string.Format("Annotation: Kind='{0}' Data='{1}'", this.Kind ?? "", this.Data ?? "");
		}

		public bool Equals(SyntaxAnnotation other)
		{
			return (object)other != null && _id == other._id;
		}

		public static bool operator ==(SyntaxAnnotation left, SyntaxAnnotation right)
		{
			if ((object)left == (object)right)
			{
				return true;
			}

			if ((object)left == null || (object)right == null)
			{
				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(SyntaxAnnotation left, SyntaxAnnotation right)
		{
			if ((object)left == (object)right)
			{
				return false;
			}

			if ((object)left == null || (object)right == null)
			{
				return true;
			}

			return !left.Equals(right);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as SyntaxAnnotation);
		}

		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}
	}
}
