using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Platform
{
	[StructLayout(LayoutKind.Sequential)]
	public struct WindowInfo
	{
		public uint cbSize;
		public Rect rcWindow;
		public Rect rcClient;
		public uint dwStyle;
		public uint dwExStyle;
		public uint dwWindowStatus;
		public uint cxWindowBorders;
		public uint cyWindowBorders;
		public ushort atomWindowType;
		public ushort wCreatorVersion;

		public WindowInfo(Boolean? filler)
			: this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
		{
			cbSize = (UInt32)(Marshal.SizeOf(typeof(WindowInfo)));
		}

	}
}
