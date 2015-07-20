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

		public int Delay
		{
			get
			{
				return (int)delayControl.Value;
			}
			set
			{
				delayControl.Value = (decimal)value;
			}
		}

		public string TargetUrl
		{
			get
			{
				return urlTextBox.Text;
			}
			set
			{
				urlTextBox.Text = value;
			}
		}
	}
}
