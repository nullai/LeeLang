﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface INoPiaObjectCreationOperation : IOperation
	{
		/// <summary>
		/// Object or collection initializer, if any.
		/// </summary>
		IObjectOrCollectionInitializerOperation Initializer { get; }
	}
}
