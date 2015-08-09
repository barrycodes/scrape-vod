using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace MainInterface
{
    public static class Program
    {
		private static class Settings
		{
			/// <summary>
			/// Unique value to name our mutex which has computer-wide scope
			/// </summary>
			public const string MutexGuid = "{ACE938CA-9D40-448B-AECD-948187BB5A35}";
		}

		/// <summary>
		/// Allows for single-instance application behavior
		/// </summary>
		private static Mutex mutex;

		private static void HandleError(Exception ex)
		{
			MessageBox.Show(
				string.Format(
					"An error has occurred.\r\nThe program must now close.\r\n\r\nError:\r\n\r\n{0}",
					ex.Message));
		}

		private static void RunProgram()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			try
			{
				mutex = new Mutex(true, Settings.MutexGuid);
				if (mutex.WaitOne(TimeSpan.Zero, true))

					try
					{
						RunProgram();
					}
					finally
					{
						mutex.ReleaseMutex();
					}

				else
					Platform.User32.PostMessage((IntPtr)Platform.PlatformApi.HWND_BROADCAST, Platform.PlatformApi.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				HandleError(ex);
			}
			finally
			{
				if (mutex != null)
					mutex.Dispose();
			}
        }
    }
}