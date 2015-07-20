using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Platform
{
	public class Win32
	{
		/// <summary>
		/// Window handle special value for broadcast.
		/// </summary>
		public const int HWND_BROADCAST = 0xFFFF;

		/// <summary>
		/// Custom window message SHOWME
		/// </summary>
		public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

		/// <summary>
		/// Posts a message to the operating system.
		/// </summary>
		/// <param name="hwnd">Window target of the message</param>
		/// <param name="msg">Message value</param>
		/// <param name="wparam">Extra data</param>
		/// <param name="lparam">Extra data</param>
		/// <returns></returns>
		[DllImport("user32")]
		public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		/// <summary>
		/// Define a custom window message.
		/// </summary>
		/// <param name="message">Name of the message</param>
		/// <returns>Value to use for referencing the message</returns>
		[DllImport("user32")]
		public static extern int RegisterWindowMessage(string message);
	}
}
