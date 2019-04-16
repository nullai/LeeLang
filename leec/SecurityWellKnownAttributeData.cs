using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public sealed class SecurityWellKnownAttributeData
	{
		// data from Security attributes:
		// Array of decoded security actions corresponding to source security attributes, null if there are no security attributes in source.
		private byte[] _lazySecurityActions;
		// Array of resolved file paths corresponding to source PermissionSet security attributes needing fixup, null if there are no security attributes in source.
		// Fixup involves reading the file contents of the resolved file and emitting it in the permission set.
		private string[] _lazyPathsForPermissionSetFixup;

		public void SetSecurityAttribute(int attributeIndex, DeclarativeSecurityAction action, int totalSourceAttributes)
		{
			Debug.Assert(attributeIndex >= 0 && attributeIndex < totalSourceAttributes);
			Debug.Assert(action != 0);

			if (_lazySecurityActions == null)
			{
				Interlocked.CompareExchange(ref _lazySecurityActions, new byte[totalSourceAttributes], null);
			}

			Debug.Assert(_lazySecurityActions.Length == totalSourceAttributes);
			_lazySecurityActions[attributeIndex] = (byte)action;
		}

		public void SetPathForPermissionSetAttributeFixup(int attributeIndex, string resolvedFilePath, int totalSourceAttributes)
		{
			Debug.Assert(attributeIndex >= 0 && attributeIndex < totalSourceAttributes);
			Debug.Assert(resolvedFilePath != null);

			if (_lazyPathsForPermissionSetFixup == null)
			{
				Interlocked.CompareExchange(ref _lazyPathsForPermissionSetFixup, new string[totalSourceAttributes], null);
			}

			Debug.Assert(_lazyPathsForPermissionSetFixup.Length == totalSourceAttributes);
			_lazyPathsForPermissionSetFixup[attributeIndex] = resolvedFilePath;
		}

		/// <summary>
		/// Used for retrieving applied source security attributes, i.e. attributes derived from well-known SecurityAttribute.
		/// </summary>
		public IEnumerable<SecurityAttribute> GetSecurityAttributes<T>(T[] customAttributes)
			where T : ICustomAttribute
		{
			Debug.Assert(customAttributes != null);
			Debug.Assert(_lazyPathsForPermissionSetFixup == null || _lazySecurityActions != null && _lazyPathsForPermissionSetFixup.Length == _lazySecurityActions.Length);

			if (_lazySecurityActions != null)
			{
				Debug.Assert(_lazySecurityActions != null);
				Debug.Assert(_lazySecurityActions.Length == customAttributes.Length);

				for (int i = 0; i < customAttributes.Length; i++)
				{
					if (_lazySecurityActions[i] != 0)
					{
						var action = (DeclarativeSecurityAction)_lazySecurityActions[i];
						ICustomAttribute attribute = customAttributes[i];

						/*
						if (_lazyPathsForPermissionSetFixup?[i] != null)
						{
							attribute = new PermissionSetAttributeWithFileReference(attribute, _lazyPathsForPermissionSetFixup[i]);
						}

						yield return new ecurityAttribute(action, attribute);*/
						throw new Exception("@!");
					}
				}
			}
			throw new Exception("@!");
		}
	}
}
