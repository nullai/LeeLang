using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct SymbolInfo : IEquatable<SymbolInfo>
	{
		internal static readonly SymbolInfo None = new SymbolInfo(null, new ISymbol[0], CandidateReason.None);

		private ISymbol[] _candidateSymbols;

		/// <summary>
		/// The symbol that was referred to by the syntax node, if any. Returns null if the given
		/// expression did not bind successfully to a single symbol. If null is returned, it may
		/// still be that case that we have one or more "best guesses" as to what symbol was
		/// intended. These best guesses are available via the CandidateSymbols property.
		/// </summary>
		public ISymbol Symbol { get; }

		/// <summary>
		/// If the expression did not successfully resolve to a symbol, but there were one or more
		/// symbols that may have been considered but discarded, this property returns those
		/// symbols. The reason that the symbols did not successfully resolve to a symbol are
		/// available in the CandidateReason property. For example, if the symbol was inaccessible,
		/// ambiguous, or used in the wrong context.
		/// </summary>
		public ISymbol[] CandidateSymbols
		{
			get
			{
				return _candidateSymbols ?? new ISymbol[0];
			}
		}

		internal ISymbol[] GetAllSymbols()
		{
			if (this.Symbol != null)
			{
				return new ISymbol[] { this.Symbol };
			}
			else
			{
				return _candidateSymbols;
			}
		}

		///<summary>
		/// If the expression did not successfully resolve to a symbol, but there were one or more
		/// symbols that may have been considered but discarded, this property describes why those
		/// symbol or symbols were not considered suitable.
		/// </summary>
		public CandidateReason CandidateReason { get; }

		internal SymbolInfo(ISymbol symbol)
			: this(symbol, new ISymbol[0], CandidateReason.None)
		{
		}

		internal SymbolInfo(ISymbol symbol, CandidateReason reason)
			: this(symbol, new ISymbol[0], reason)
		{
		}

		internal SymbolInfo(ISymbol[] candidateSymbols, CandidateReason candidateReason)
			: this(null, candidateSymbols, candidateReason)
		{
		}

		internal SymbolInfo(ISymbol symbol, ISymbol[] candidateSymbols, CandidateReason candidateReason)
			: this()
		{
			this.Symbol = symbol;
			_candidateSymbols = candidateSymbols == null ? new ISymbol[0] : candidateSymbols;

#if DEBUG
			const NamespaceKind NamespaceKindNamespaceGroup = (NamespaceKind)0;
			Debug.Assert((object)symbol == null || symbol.Kind != SymbolKind.Namespace || ((INamespaceSymbol)symbol).NamespaceKind != NamespaceKindNamespaceGroup);
			foreach (var item in _candidateSymbols)
			{
				Debug.Assert(item.Kind != SymbolKind.Namespace || ((INamespaceSymbol)item).NamespaceKind != NamespaceKindNamespaceGroup);
			}
#endif

			this.CandidateReason = candidateReason;
		}

		public override bool Equals(object obj)
		{
			return obj is SymbolInfo && Equals((SymbolInfo)obj);
		}

		public bool Equals(SymbolInfo other)
		{
			return object.Equals(this.Symbol, other.Symbol)
				&& ((_candidateSymbols == null && other._candidateSymbols == null) || _candidateSymbols.SequenceEqual(other._candidateSymbols))
				&& this.CandidateReason == other.CandidateReason;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(this.Symbol, Hash.Combine(Hash.CombineValues(_candidateSymbols, 4), (int)this.CandidateReason));
		}

		internal bool IsEmpty
		{
			get
			{
				return this.Symbol == null
					&& this.CandidateSymbols.Length == 0;
			}
		}
	}
}
