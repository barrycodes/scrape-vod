using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Platform
{
	public static class PlatformApi
	{
		/// <summary>
		/// Window handle special value for broadcast.
		/// </summary>
		public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

		/// <summary>
		/// Custom window message SHOWME
		/// </summary>
		public static readonly uint WM_SHOWME = User32.RegisterWindowMessage("WM_SHOWME");

		public class WindowInfo
		{
			public string Title { get; set; }
			public IntPtr Handle { get; set; }
			public bool Visible { get; set; }
			public System.Drawing.Rectangle Bounds { get; set; }
			public ShowWindowCommands State { get; set; }
			public System.Drawing.Point MinimizedPosition { get; set; }
			public System.Drawing.Point MaximizedPosition { get; set; }
			public System.Drawing.Rectangle NormalBounds { get; set; }
			public uint PlacementFlags { get; set; }
			public int ZOrder { get; set; }
		}

		public static string GetWindowText(IntPtr hwnd)
		{
			string result = string.Empty;
			int size = Platform.User32.GetWindowTextLength(hwnd);
			if (size++ > 0)
			{
				StringBuilder sb = new StringBuilder(size);
				Platform.User32.GetWindowText(hwnd, sb, size);
				result = sb.ToString();
			}
			return result;
		}

		public static int GetZOrder(IntPtr hWnd)
		{
			var z = 0;
			for (IntPtr h = hWnd; h != IntPtr.Zero; h = User32.GetWindow(h, GetWindowCommand.GW_HWNDPREV)) z++;
			return z;
		}
		
		public static WindowInfo GetWindowInfo(IntPtr hwnd)
		{
			WindowInfo result = null;

			Platform.WindowInfo windowInfo = new Platform.WindowInfo();

			Platform.User32.GetWindowInfo(hwnd, ref windowInfo);
			Rectangle bounds = windowInfo.rcWindow.ToRectangle();

			string title = GetWindowText(hwnd);

			bool isVisible = Platform.User32.IsWindowVisible(hwnd);

			WindowPlacement placement = WindowPlacement.Default;
			User32.GetWindowPlacement(hwnd, ref placement);
			ShowWindowCommands windowCommands = placement.ShowCmd;
			Rectangle normalBounds = placement.NormalPosition.ToRectangle();
			int zOrder = GetZOrder(hwnd);

			//result = new WindowInfo(hwnd, title, isVisible, bounds);
			result =
				new WindowInfo {
					Handle=hwnd,
					Title=title,
					Visible=isVisible,
					Bounds=bounds,
					State=windowCommands,
					NormalBounds=normalBounds,
					MinimizedPosition=placement.MinPosition.ToPoint(),
					MaximizedPosition=placement.MaxPosition.ToPoint(),
					PlacementFlags=placement.Flags,
					ZOrder=zOrder,
				};

			return result;
		}

		public static WindowInfo[] GetAllWindowsInfo(bool includeInvisible)
		{
			List<WindowInfo> windows = new List<WindowInfo>();

			Platform.User32.EnumWindows(
				(hwnd, lparam) =>
				{
					if (includeInvisible || Platform.User32.IsWindowVisible(hwnd))
						windows.Add(GetWindowInfo(hwnd));

					return true;
				},
				IntPtr.Zero);

			windows = windows.OrderByDescending(w => w.ZOrder).ToList();

			return windows.ToArray();
		}

		///// Potential callback function to use with EnumWindows
		///// </summary>
		///// <param name="hWnd"></param>
		///// <param name="lParam"></param>
		///// <returns></returns>
		//protected static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
		//{
		//    int size = GetWindowTextLength(hWnd);
		//    if (size++ > 0 && IsWindowVisible(hWnd))
		//    {
		//        StringBuilder sb = new StringBuilder(size);
		//        GetWindowText(hWnd, sb, size);
		//        Console.WriteLine(sb.ToString());
		//    }
		//    return true;
		//}
	}
}
