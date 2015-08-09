using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MainInterface
{
	public partial class OptionsForm : Form
	{
		public OptionsForm()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		public MainForm.Options Options
		{
			get
			{
				return new MainForm.Options
				{
					ActionInterval = TimeSpan.FromMilliseconds((double)delayControl.Value),
					TargetUrl = urlTextBox.Text
				};
			}
			set
			{
				delayControl.Value = (decimal)value.ActionInterval.TotalMilliseconds;
				urlTextBox.Text = value.TargetUrl;
			}
		}
	}
}
