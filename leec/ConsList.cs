using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public class ConsList<T>
	{
		public static readonly ConsList<T> Empty = new ConsList<T>();

		private readonly T _head;
		private readonly ConsList<T> _tail;
		private ConsList()
		{
			_head = default(T);
			_tail = null;
		}

		public ConsList(T head, ConsList<T> tail)
		{
			Debug.Assert(tail != null);

			_head = head;
			_tail = tail;
		}

		public T Head
		{
			get
			{
				Debug.Assert(this != Empty);
				return _head;
			}
		}

		public ConsList<T> Tail
		{
			get
			{
				Debug.Assert(this != Empty);
				return _tail;
			}
		}

		public bool Any()
		{
			return this != Empty;
		}

		public ConsList<T> Push(T value)
		{
			return new ConsList<T>(value, this);
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder("ConsList[");
			bool any = false;
			for (ConsList<T> list = this; list._tail != null; list = list._tail)
			{
				if (any)
				{
					result.Append(", ");
				}

				result.Append(list._head);
				any = true;
			}

			result.Append("]");
			return result.ToString();
		}
	}
}
