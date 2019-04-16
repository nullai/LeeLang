using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public class CommonModuleCompilationState
	{
		private bool _frozen;

		internal void Freeze()
		{
			Debug.Assert(!_frozen);

			// make sure that values from all threads are visible:

			_frozen = true;
		}

		internal bool Frozen
		{
			get { return _frozen; }
		}
	}

	public class ModuleCompilationState<TNamedTypeSymbol, TMethodSymbol> : CommonModuleCompilationState
		where TNamedTypeSymbol : class, INamespaceTypeDefinition
		where TMethodSymbol : class, IMethodDefinition
	{
		/// <summary>
		/// Maps an async/iterator method to the synthesized state machine type that implements the method. 
		/// </summary>
		private Dictionary<TMethodSymbol, TNamedTypeSymbol> _lazyStateMachineTypes;

		internal void SetStateMachineType(TMethodSymbol method, TNamedTypeSymbol stateMachineClass)
		{
			Debug.Assert(!Frozen);

			if (_lazyStateMachineTypes == null)
			{
				Interlocked.CompareExchange(ref _lazyStateMachineTypes, new Dictionary<TMethodSymbol, TNamedTypeSymbol>(), null);
			}

			lock (_lazyStateMachineTypes)
			{
				_lazyStateMachineTypes.Add(method, stateMachineClass);
			}
		}

		internal bool TryGetStateMachineType(TMethodSymbol method, out TNamedTypeSymbol stateMachineType)
		{
			Debug.Assert(Frozen);

			stateMachineType = null;
			return _lazyStateMachineTypes != null && _lazyStateMachineTypes.TryGetValue(method, out stateMachineType);
		}
	}
}
