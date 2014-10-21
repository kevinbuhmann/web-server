/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebServer
{
	public partial class SettingsForm : Form
	{
		public ServerSettings ServerSettings
		{
			get
			{
				return this.propertyGrid.SelectedObject as ServerSettings;
			}
			set
			{
				this.propertyGrid.SelectedObject = value;
			}
		}

		public SettingsForm()
		{
			this.InitializeComponent();
			this.ServerSettings = new ServerSettings();
		}
	}
}