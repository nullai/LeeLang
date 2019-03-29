using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public class DiagnosticBag
	{
		// The lazyBag field is populated lazily -- the first time an error is added.
		private Queue<Diagnostic> _lazyBag;

		/// <summary>
		/// Return true if the bag is completely empty - not even containing void diagnostics.
		/// </summary>
		/// <remarks>
		/// This exists for short-circuiting purposes. Use <see cref="System.Linq.Enumerable.Any{T}(IEnumerable{T})"/>
		/// to get a resolved Tuple(Of NamedTypeSymbol, ImmutableArray(Of Diagnostic)) (i.e. empty after eliminating void diagnostics).
		/// </remarks>
		public bool IsEmptyWithoutResolution
		{
			get
			{
				// It should be safe to access this here, since we normally have a collect phase and
				// then a report phase, and we shouldn't be called during the "report" phase. We
				// also never remove diagnostics, so the worst that happens is that we don't return
				// an element that is added a split second after this is called.
				Queue<Diagnostic> bag = _lazyBag;
				return bag == null || bag.Count == 0;
			}
		}

		/// <summary>
		/// Returns true if the bag has any diagnostics with Severity=Error. Does not consider warnings or informationals.
		/// </summary>
		/// <remarks>
		/// Resolves any lazy diagnostics in the bag.
		/// 
		/// Generally, this should only be called by the creator (modulo pooling) of the bag (i.e. don't use bags to communicate -
		/// if you need more info, pass more info).
		/// </remarks>
		public bool HasAnyErrors()
		{
			if (IsEmptyWithoutResolution)
			{
				return false;
			}

			foreach (Diagnostic diagnostic in Bag)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if the bag has any non-lazy diagnostics with Severity=Error.
		/// </summary>
		/// <remarks>
		/// Does not resolve any lazy diagnostics in the bag.
		/// 
		/// Generally, this should only be called by the creator (modulo pooling) of the bag (i.e. don't use bags to communicate -
		/// if you need more info, pass more info).
		/// </remarks>
		public bool HasAnyResolvedErrors()
		{
			if (IsEmptyWithoutResolution)
			{
				return false;
			}

			foreach (Diagnostic diagnostic in Bag)
			{
				if ((diagnostic as DiagnosticWithInfo)?.HasLazyInfo != true && diagnostic.Severity == DiagnosticSeverity.Error)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Add a diagnostic to the bag.
		/// </summary>
		public void Add(Diagnostic diag)
		{
			Queue<Diagnostic> bag = this.Bag;
			bag.Enqueue(diag);
		}

		public LeeDiagnosticInfo Add(ErrorCode code, Location location)
		{
			var info = new LeeDiagnosticInfo(code);
			var diag = new LeeDiagnostic(info, location);
			Add(diag);
			return info;
		}

		public LeeDiagnosticInfo Add(ErrorCode code, Location location, params object[] args)
		{
			var info = new LeeDiagnosticInfo(code, args);
			var diag = new LeeDiagnostic(info, location);
			Add(diag);
			return info;
		}

		/// <summary>
		/// Add multiple diagnostics to the bag.
		/// </summary>
		public void AddRange<T>(T[] diagnostics) where T : Diagnostic
		{
			if (diagnostics.Length != 0)
			{
				Queue<Diagnostic> bag = this.Bag;
				for (int i = 0; i < diagnostics.Length; i++)
				{
					bag.Enqueue(diagnostics[i]);
				}
			}
		}

		/// <summary>
		/// Add multiple diagnostics to the bag.
		/// </summary>
		public void AddRange(IEnumerable<Diagnostic> diagnostics)
		{
			foreach (Diagnostic diagnostic in diagnostics)
			{
				this.Bag.Enqueue(diagnostic);
			}
		}

		/// <summary>
		/// Add another DiagnosticBag to the bag.
		/// </summary>
		public void AddRange(DiagnosticBag bag)
		{
			if (!bag.IsEmptyWithoutResolution)
			{
				AddRange(bag.Bag);
			}
		}

		/// <summary>
		/// Add another DiagnosticBag to the bag and free the argument.
		/// </summary>
		public void AddRangeAndFree(DiagnosticBag bag)
		{
			AddRange(bag);
			bag.Free();
		}

		/// <summary>
		/// Seal the bag so no further errors can be added, while clearing it and returning the old set of errors.
		/// Return the bag to the pool.
		/// </summary>
		public TDiagnostic[] ToReadOnlyAndFree<TDiagnostic>() where TDiagnostic : Diagnostic
		{
			Queue<Diagnostic> oldBag = _lazyBag;
			Free();

			return ToReadOnlyCore<TDiagnostic>(oldBag);
		}

		public Diagnostic[] ToReadOnlyAndFree()
		{
			return ToReadOnlyAndFree<Diagnostic>();
		}

		public TDiagnostic[] ToReadOnly<TDiagnostic>() where TDiagnostic : Diagnostic
		{
			Queue<Diagnostic> oldBag = _lazyBag;
			return ToReadOnlyCore<TDiagnostic>(oldBag);
		}

		public Diagnostic[] ToReadOnly()
		{
			return ToReadOnly<Diagnostic>();
		}

		private static TDiagnostic[] ToReadOnlyCore<TDiagnostic>(Queue<Diagnostic> oldBag) where TDiagnostic : Diagnostic
		{
			if (oldBag == null)
			{
				return new TDiagnostic[0];
			}

			List<TDiagnostic> builder = new List<TDiagnostic>();

			foreach (TDiagnostic diagnostic in oldBag) // Cast should be safe since all diagnostics should be from same language.
			{
				if (diagnostic.Severity != InternalDiagnosticSeverity.Void)
				{
					Debug.Assert(diagnostic.Severity != InternalDiagnosticSeverity.Unknown); //Info access should have forced resolution.
					builder.Add(diagnostic);
				}
			}

			return builder.ToArray();
		}


		/// <remarks>
		/// Generally, this should only be called by the creator (modulo pooling) of the bag (i.e. don't use bags to communicate -
		/// if you need more info, pass more info).
		/// </remarks>
		public IEnumerable<Diagnostic> AsEnumerable()
		{
			Queue<Diagnostic> bag = this.Bag;

			bool foundVoid = false;

			foreach (Diagnostic diagnostic in bag)
			{
				if (diagnostic.Severity == InternalDiagnosticSeverity.Void)
				{
					foundVoid = true;
					break;
				}
			}

			return foundVoid
				? AsEnumerableFiltered()
				: bag;
		}

		/// <remarks>
		/// Using an iterator to avoid copying the list.  If perf is a problem,
		/// create an explicit enumerator type.
		/// </remarks>
		private IEnumerable<Diagnostic> AsEnumerableFiltered()
		{
			foreach (Diagnostic diagnostic in this.Bag)
			{
				if (diagnostic.Severity != InternalDiagnosticSeverity.Void)
				{
					Debug.Assert(diagnostic.Severity != InternalDiagnosticSeverity.Unknown); //Info access should have forced resolution.
					yield return diagnostic;
				}
			}
		}

		public IEnumerable<Diagnostic> AsEnumerableWithoutResolution()
		{
			// PERF: don't make a defensive copy - callers are public and won't modify the bag.
			return (IEnumerable<Diagnostic>)_lazyBag ?? new Diagnostic[0];
		}

		public override string ToString()
		{
			if (this.IsEmptyWithoutResolution)
			{
				// TODO(cyrusn): do we need to localize this?
				return "<no errors>";
			}
			else
			{
				StringBuilder builder = new StringBuilder();
				foreach (Diagnostic diag in Bag) // NOTE: don't force resolution
				{
					builder.AppendLine(diag.ToString());
				}
				return builder.ToString();
			}
		}

		/// <summary>
		/// Get the underlying concurrent storage, creating it on demand if needed.
		/// NOTE: Concurrent Adding to the bag is supported, but concurrent Clearing is not.
		///       If one thread adds to the bug while another clears it, the scenario is 
		///       broken and we cannot do anything about it here.
		/// </summary>
		private Queue<Diagnostic> Bag
		{
			get
			{
				Queue<Diagnostic> bag = _lazyBag;
				if (bag == null)
				{
					bag = new Queue<Diagnostic>();
					_lazyBag = bag;
				}

				return bag;
			}
		}

		// clears the bag.
		/// NOTE: Concurrent Adding to the bag is supported, but concurrent Clearing is not.
		///       If one thread adds to the bug while another clears it, the scenario is 
		///       broken and we cannot do anything about it here.
		public void Clear()
		{
			var bag = _lazyBag;
			if (bag != null)
			{
				_lazyBag = null;
			}
		}

		#region "Poolable"

		public static DiagnosticBag GetInstance()
		{
			DiagnosticBag bag = s_poolInstance.Allocate();
			return bag;
		}

		public void Free()
		{
			Clear();
			s_poolInstance.Free(this);
		}

		private static readonly ObjectPool<DiagnosticBag> s_poolInstance = CreatePool(128);
		private static ObjectPool<DiagnosticBag> CreatePool(int size)
		{
			return new ObjectPool<DiagnosticBag>(() => new DiagnosticBag(), size);
		}

		#endregion

		#region Debugger View

		public sealed class DebuggerProxy
		{
			private readonly DiagnosticBag _bag;

			public DebuggerProxy(DiagnosticBag bag)
			{
				_bag = bag;
			}

			public object[] Diagnostics
			{
				get
				{
					var lazyBag = _bag._lazyBag;
					if (lazyBag != null)
					{
						return lazyBag.ToArray();
					}
					else
					{
						return new object[0];
					}
				}
			}
		}

		private string GetDebuggerDisplay()
		{
			return "Count = " + (_lazyBag?.Count ?? 0);
		}
		#endregion
	}
}
