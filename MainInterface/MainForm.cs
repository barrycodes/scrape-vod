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

		[Serializable]
		public class Options
		{
			public string TargetUrl { get; set; }
			public TimeSpan ActionInterval { get; set; }

			public static Options Default
			{
				get
				{
					return new Options { 
						TargetUrl = "http://www.vod.com",
						ActionInterval = TimeSpan.FromMilliseconds(2500) };
				}
			}
		}

		private Options _options;

        public MainForm()
        {
            InitializeComponent();

			intentionalClose = false;
			mode = NavigationMode.Idle;
			tasks = new Queue<AccountInfo>();
			allCredentials = new AccountInfo[] { };
			currentLogin = null;

			_options = LoadOptions();

			timer1.Interval = (int)_options.ActionInterval.TotalMilliseconds;
		}

		private static Options LoadOptions()
		{
			Options result = null;

			try
			{
				result = (Options)CommonTypes.SettingsManager.LoadSettings("@barrycodes", "VodFarmer");
			}
			catch { }

			return result ?? Options.Default;
		}

		private void StoreCredentials()
		{
			if (allCredentials != null && allCredentials.Length >= 1)

				try
				{
					CommonTypes.SettingsManager.AssureFolderExists("@barrycodes", "VodFarmer");
					string settingsPath = CommonTypes.SettingsManager.GetSettingsPath("@barrycodes", "VodFarmer");
					using (FileStream credentialsWriter = new FileStream(Path.Combine(settingsPath, "credentials.txt"), FileMode.Create, FileAccess.Write))
					{
						using (StreamWriter writer = new StreamWriter(credentialsWriter))
						{
							foreach (AccountInfo account in allCredentials)
							{
								writer.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", account.Username, account.Password, account.MinuteCount, account.RewardPoints));
							}
						}
					}
				}
				catch { }

		}

		private void StoreOptions(Options options)
		{
			try
			{
				CommonTypes.SettingsManager.StoreSettings("@barrycodes", "VodFarmer", options);
			}
			catch { }

			StoreCredentials();
		}

		public enum NavigationMode
		{
			Unknown,
			Idle,
			DoLoadSite,
			DoNextLogout,
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
			if (m.Msg == Platform.PlatformApi.WM_SHOWME)
				ShowMe();
			base.WndProc(ref m);
		}

		private class AccountInfo
		{
			public string Username { get; set; }
			public string Password { get; set; }
			public int MinuteCount { get; set; }
			public int RewardPoints { get; set; }

			public static AccountInfo Default
			{
				get
				{
					return new AccountInfo
					{
						Username = string.Empty,
						Password = string.Empty,
						MinuteCount = -1,
						RewardPoints = -1,
					};
				}
			}
		}

		private Queue<AccountInfo> tasks;

		private AccountInfo[] allCredentials;

		private static AccountInfo[] LoadCredentials()
		{
			List<AccountInfo> results = new List<AccountInfo>();

			CommonTypes.SettingsManager.AssureFolderExists("@barrycodes", "VodFarmer");

			using (FileStream readStream = new FileStream(Path.Combine(CommonTypes.SettingsManager.GetSettingsPath("@barrycodes", "VodFarmer"), "credentials.txt"), FileMode.Open, FileAccess.Read))
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
									results.Add(info);
								}
							}
						}
					} while (textLine != null);
					reader.ReadLine();
				}
			}
			return results.ToArray();
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
				allCredentials = LoadCredentials();
				tasks = new Queue<AccountInfo>(allCredentials);
				totalCount = tasks.Count;
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
				webBrowser1.Url = new Uri(_options.TargetUrl);
			}
			finally
			{
				mode = NavigationMode.DoLogout;
			}
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			StoreOptions(_options);
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

		private AccountInfo currentLogin;

		private	void DoLogin()
		{
			bool success = false;

			AccountInfo login = null;
			if (tasks.Count > 0)
				login = tasks.Dequeue();

			if (login != null)
			{
				currentLogin = login;

				try
				{
					var emailInput = GetEmailElement();
					var passwordInput = GetPasswordElement();
					var loginButton = GetLoginElement();

					emailInput.InnerText = currentLogin.Username;
					passwordInput.InnerText = currentLogin.Password;

					loginButton.InvokeMember("click");

					success = true;
				}
				catch (Exception)
				{
					tasks.Enqueue(login);
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
				webBrowser1.Navigate(_options.TargetUrl);
			}
			finally
			{
				mode = NavigationMode.DoNextLogout;
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

		private void UpdateMinuteCount()
		{
			try
			{
				string result = (string)InjectScript("getRemainingMinutes", "return $('#account_center > div.body > div.highlight > ul:first-child > li:first-child').text();", true);
				currentLogin.MinuteCount = int.Parse(result.Split(' ')[0]);
			}
			catch { }
		}

		private void DoNextLogout()
		{
			UpdateMinuteCount();
			DoLogout();
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

		private object InjectScript(string scriptName, string scriptContents, bool runScript = false)
		{
			object result = null;

			var scriptElement = webBrowser1.Document.CreateElement("script");
			((IHTMLScriptElement)scriptElement.DomElement).text = "function " + scriptName + "() { " + scriptContents + "}";
			webBrowser1.Document.GetElementsByTagName("head")[0].AppendChild(scriptElement);
			if (runScript)
				result = webBrowser1.Document.InvokeScript(scriptName);

			return result;
		}

		private void StopAutomation()
		{
			timer1.Stop();
			mode = NavigationMode.Idle;
			statusLabel.Text = "Ready";
			optionsToolStripMenuItem.Enabled = true;
			StoreOptions(_options);
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

					case NavigationMode.DoNextLogout:

						DoNextLogout();

						statusLabel.Text =
							string.Format(
								"Running    |    Completed: {0}    |    Remaining: {1}",
								totalCount - tasks.Count,
								tasks.Count);
						break;

					case NavigationMode.DoLogout:

						DoLogout();
		
						statusLabel.Text =
							string.Format(
								"Running    |    Completed: {0}    |    Remaining: {1}",
								totalCount - tasks.Count,
								tasks.Count);
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
				form.Options = _options;
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					_options = form.Options;
					StoreOptions(_options);
				}
			}
		}
    }
}