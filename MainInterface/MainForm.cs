using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.IO;
using System.Diagnostics;

namespace MainInterface
{
    public partial class MainForm : Form
    {
		private bool intentionalClose;
		private NavigationMode mode;
		private string targetUrl;

        public MainForm()
        {
            InitializeComponent();

			intentionalClose = false;
			mode = NavigationMode.Idle;
			targetUrl = "http://danniondemand.com";
			credentials = new Queue<AccountInfo>();
		}

		public enum NavigationMode
		{
			Unknown,
			Idle,
			DoLoadSite,
			DoLogout,
			DoLogin,
			DoWork,
			DoReload,
		}

		private void HideMe()
		{
			Hide();
			WindowState = FormWindowState.Minimized;
		}

		private void ShowMe()
		{
			WindowState = FormWindowState.Normal;
			Show();
			Activate();
		}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
			if (!intentionalClose)
			{
				bool cancel = false;

				switch (e.CloseReason)
				{
					case CloseReason.UserClosing: cancel = true; break;
					default: cancel = false; break;
				}
				if (cancel)
				{
					e.Cancel = true;
					HideMe();
				}
			}
        }

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowMe();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			intentionalClose = true;
			Close();
		}

		private void notifyIcon1_DoubleClick(object sender, EventArgs e)
		{
			ShowMe();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Platform.Win32.WM_SHOWME)
				ShowMe();
			base.WndProc(ref m);
		}

		private class AccountInfo
		{
			public string Username { get; set; }
			public string Password { get; set; }
			public int MinuteCount { get; set; }
			public int RewardPoints { get; set; }
			public AccountInfo()
			{
				Username = Password = string.Empty;
				MinuteCount = -1;
				RewardPoints = -1;
			}
		}

		private Queue<AccountInfo> credentials;

		private void LoadCredentials()
		{
			credentials.Clear();
			using (FileStream readStream = new FileStream("credentials.txt", FileMode.Open, FileAccess.Read))
			{
				using (StreamReader reader = new StreamReader(readStream))
				{
					string textLine = null;
					do
					{
						textLine = reader.ReadLine();
						if (textLine != null)
						{
							string[] parts = textLine.Split('\t');
							if (parts != null && parts.Length > 0)
							{
								var info = new AccountInfo();
								if (parts.Length >= 2)
								{
									info.Username = parts[0];
									info.Password = parts[1];
									if (parts.Length >= 3)
									{
										info.MinuteCount = int.Parse(parts[2]);
										if (parts.Length >= 4)
											info.RewardPoints = int.Parse(parts[3]);
									}
								}
							}
							credentials.Enqueue(new AccountInfo { Username = parts[0], Password = parts[1] });
						}
					} while (textLine != null);
					reader.ReadLine();
				}
			}
		}

		private void EditCredentials()
		{
			Process.Start("credentials.txt");
		}

		private void StartAutomation()
		{
			optionsToolStripMenuItem.Enabled = false;
			timer1.Stop();

			if (mode == NavigationMode.Idle)
				DoInitialize();

			timer1.Start();
		}

		private int totalCount;

		private void DoInitialize()
		{
			try
			{
				LoadCredentials();
				totalCount = credentials.Count;
			}
			finally
			{
				mode = NavigationMode.DoLoadSite;
			}
		}

		private void DoLoadSite()
		{
			try
			{
				webBrowser1.Url = new Uri(targetUrl);
			}
			finally
			{
				mode = NavigationMode.DoLogout;
			}
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			webBrowser1.Dispose();
		}

		private HtmlElement GetEmailElement()
		{
			HtmlElement result = null;

			result = webBrowser1.Document.GetElementById("email");

			return result;
		}

		//private HtmlElement GetEnterSiteElement()
		//{
		//    HtmlElement result = null;

		//    var elements = webBrowser1.Document.GetElementsByTagName("a");
		//    foreach (HtmlElement element in elements)
		//    {
		//        if (element.GetAttribute("href") == @"http://ct.vod.com/index.php")
		//        {
		//            result = element;
		//            break;
		//        }
		//    } 
			
		//    return result;
		//}

		private HtmlElement GetPasswordElement()
		{
			HtmlElement result = null;

			HtmlElementCollection inputCollection = webBrowser1.Document.GetElementsByTagName("input");
			foreach (HtmlElement inputElement in inputCollection)
			{
				if (inputElement.Name == "password")
				{
					result = inputElement;
					break;
				}
			}

			return result;
		}

		private HtmlElement GetLoginElement()
		{
			HtmlElement result = null;

			result = webBrowser1.Document.GetElementById("login_submit");

			return result;
		}

		private HtmlElement GetLogoutElement()
		{
			HtmlElement result = null;

			var elements = webBrowser1.Document.GetElementsByTagName("a");
			foreach (HtmlElement element in elements)
			{
				if (element.GetAttribute("href").StartsWith(@"http://www.vod.com/logout.php?"))
				{
					result = element;
					break;
				}
			}

			return result;
		}

		private	void DoLogin()
		{
			bool success = false;

			AccountInfo login = null;
			if (credentials.Count > 0)
				login = credentials.Dequeue();

			if (login != null)
			{
				try
				{
					var emailInput = GetEmailElement();
					var passwordInput = GetPasswordElement();
					var loginButton = GetLoginElement();

					emailInput.InnerText = login.Username;
					passwordInput.InnerText = login.Password;

					loginButton.InvokeMember("click");

					success = true;
				}
				catch (Exception)
				{
					credentials.Enqueue(login);
				}
				finally
				{
					mode = success ? NavigationMode.DoWork : NavigationMode.DoLoadSite;
				}
			}
			else
			{
				// all logins have been processed; we are done
				StopAutomation();
			}
		}

		private void DoWork()
		{
			try
			{
				//webBrowser1.Document.InvokeScript("click_minutes_small");
				InjectScript("flick_minutes", @"$.ajax({ type: 'POST', url: '/backend.php', data: { click_minutes_small: 1} });", true);
			}
			finally
			{
				mode = NavigationMode.DoReload;
			}
		}

		private void DoReload()
		{
			try
			{
				webBrowser1.Navigate(targetUrl);
			}
			finally
			{
				mode = NavigationMode.DoLogout;
			}
		}

		private void DoLogout()
		{
			try
			{
				var logoutElement = GetLogoutElement();
				if (logoutElement != null)
					logoutElement.InvokeMember("click");
			}
			finally
			{
				mode = NavigationMode.DoLogin;
			}
		}

		//private void DoEnterSite()
		//{
		//    try
		//    {
		//        var elements = webBrowser1.Document.GetElementsByTagName("a");
		//        HtmlElement enterElement = null;
		//        foreach (HtmlElement element in elements)
		//        {
		//            if (element.GetAttribute("href") == @"http://ct.vod.com/index.php")
		//            {
		//                enterElement = element;
		//                break;
		//            }
		//        }
		//        enterElement.InvokeMember("click");
		//    }
		//    finally
		//    {
		//        mode = NavigationMode.DoLogout;
		//    }
		//}

		private void InjectScript(string scriptName, string scriptContents, bool runScript = false)
		{
			var scriptElement = webBrowser1.Document.CreateElement("script");
			((IHTMLScriptElement)scriptElement.DomElement).text = "function " + scriptName + "() { " + scriptContents + "}";
			webBrowser1.Document.GetElementsByTagName("head")[0].AppendChild(scriptElement);
			if (runScript)
				webBrowser1.Document.InvokeScript(scriptName);
		}

		private void StopAutomation()
		{
			timer1.Stop();
			mode = NavigationMode.Idle;
			statusLabel.Text = "Ready";
			optionsToolStripMenuItem.Enabled = true;
		}

		private void NextStep()
		{
			try
			{

				switch (mode)
				{
					case NavigationMode.DoLoadSite:
						statusLabel.Text = "Loading";
						DoLoadSite();
						break;
					case NavigationMode.DoLogin:
						DoLogin();
						break;
					case NavigationMode.DoWork:
						DoWork();
						break;

					case NavigationMode.DoReload:
						DoReload();
						break;

					case NavigationMode.DoLogout:
						DoLogout();
						statusLabel.Text =
							string.Format(
								"Running    |    Completed: {0}    |    Remaining: {1}",
								totalCount - credentials.Count,
								credentials.Count);
						break;
				}
			}
			catch (Exception) { }
		}

		private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			//if (!webBrowser1.IsBusy)
			//    NextStep();
		}

		private void nextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			NextStep();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			NextStep();
		}

		private int dailyMinuteOffset;

		private DateTime lastRunTime;

		private int GenerateNewDailyOffset()
		{
			// generate number of minutes to offset randomly.
			// target time is between 23:05 inclusive and 23:45 exclusive.
			return (new Random().Next(40)) + 5;
		}

		private void dailyTimer_Tick(object sender, EventArgs e)
		{
			DateTime now = DateTime.Now;

			if (dailyMinuteOffset < 0)
				dailyMinuteOffset = GenerateNewDailyOffset();

			// target a time during the 23:00 hour of the day
			if (now.Hour == 23
				&& now.Minute == dailyMinuteOffset
				&& lastRunTime.Date != now.Date
				&& mode == NavigationMode.Idle)
			{
				lastRunTime = now;
				dailyMinuteOffset = GenerateNewDailyOffset();
				StartAutomation();
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			lastRunTime = DateTime.MinValue;
			dailyMinuteOffset = -1;
			dailyTimer.Start();
		}

		private void browserInterfaceToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			webBrowser1.Visible = browserInterfaceToolStripMenuItem.Checked;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (AboutBox1 form = new AboutBox1())
				form.ShowDialog();
		}

		private void startToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StartAutomation();
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			timer1.Stop();
			optionsToolStripMenuItem.Enabled = true;
		}

		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StopAutomation();
		}

		private void editCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EditCredentials();
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (OptionsForm form = new OptionsForm())
			{
				form.Delay = timer1.Interval;
				form.TargetUrl = targetUrl;
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					timer1.Interval = form.Delay;
					targetUrl = form.TargetUrl;
				}
			}
		}
    }
}