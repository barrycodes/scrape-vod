using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Platform
{
	/// <summary>
	/// Contains information about the placement of a window on the screen.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct WindowPlacement
	{
		/// <summary>
		/// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
		/// <para>
		/// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
		/// </para>
		/// </summary>
		public int Length;

		/// <summary>
		/// Specifies flags that control the position of the minimized window and the method by which the window is restored.
		/// </summary>
		public uint Flags;

		/// <summary>
		/// The current show state of the window.
		/// </summary>
		public ShowWindowCommands ShowCmd;

		/// <summary>
		/// The coordinates of the window's upper-left corner when the window is minimized.
		/// </summary>
		public Point MinPosition;

		/// <summary>
		/// The coordinates of the window's upper-left corner when the window is maximized.
		/// </summary>
		public Point MaxPosition;

		/// <summary>
		/// The window's coordinates when the window is in the restored position.
		/// </summary>
		public Rect NormalPosition;

		/// <summary>
		/// Gets the default (empty) value.
		/// </summary>
		public static WindowPlacement Default
		{
			get
			{
				WindowPlacement result = new WindowPlacement();
				result.Length = Marshal.SizeOf(result);
				return result;
			}
		}
	}
}
