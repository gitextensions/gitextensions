// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MiniDumpType.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Enums
{
	public enum MiniDumpType
	{
		/// <summary>
		/// Generate no minidump at all.
		/// </summary>
		None,

		/// <summary>
		/// Generates the smallest possible minidump still with useful information. Dump size is about ~100KB compressed.
		/// </summary>
		Tiny,

		/// <summary>
		/// Generates minidump with private read write memory and data segments. This mode allows retreiving of local values and the stack
		/// variables. Dump size is about ~5MB compressed.
		/// </summary>
		Normal,

		/// <summary>
		/// Generates full application memory dump. This simply dump all memory used by the process. Dump size is about ~100MB compressed.
		/// </summary>
		Full
	}
}