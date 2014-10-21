/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using WebServer.Properties;

namespace WebServer
{
	public static class WebServerProgram
	{
		#region Static Fields

		public static readonly string[] DefaultDocuments = { "index.php", "index.phphtml", "index.html", "index.htm", "index.txt" };

		#endregion

		#region Static Properties

		public static ServerSettings ServerSettings
		{
			get;
			set;
		}

		private static NotifyIcon NotifyIcon
		{
			get;
			set;
		}

		#endregion

		#region Static Methods

		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			// default settings
			WebServerProgram.ServerSettings = new ServerSettings();

			// get settings from user
#if !DEBUG
			using (SettingsForm settingsForm = new SettingsForm())
			{
				DialogResult result = settingsForm.ShowDialog();
				if (result == DialogResult.OK)
					WebServerProgram.ServerSettings = settingsForm.ServerSettings;
				else
					return;
			}
#endif

			// reset the log
			File.WriteAllText("log.txt", "Remote End Point, Requested Path\r\n");

			// generate the localhost url
			string localhostURL = (WebServerProgram.ServerSettings.HTTPPort == 80) ?
				"http://localhost/" : string.Format("http://localhost:{0}/", WebServerProgram.ServerSettings.HTTPPort);

			// create the notify icon
			WebServerProgram.NotifyIcon = new NotifyIcon();
			WebServerProgram.NotifyIcon.Icon = Resources.WebServerIcon;
			WebServerProgram.NotifyIcon.Text = string.Format("Web Server running on port {0}", WebServerProgram.ServerSettings.HTTPPort);
			WebServerProgram.NotifyIcon.DoubleClick += (sender, e) => { Process.Start(localhostURL); };
			WebServerProgram.NotifyIcon.ContextMenu = new ContextMenu();
			WebServerProgram.NotifyIcon.ContextMenu.MenuItems.Add("Exit", (sender, e) => { WebServerProgram.NotifyIcon.Visible = false; Application.Exit(); });
			WebServerProgram.NotifyIcon.Visible = true;
			WebServerProgram.NotifyIcon.ShowBalloonTip(100, "Web Server running...", WebServerProgram.NotifyIcon.Text, ToolTipIcon.Info);

			// start the server listen loop
			Thread serverLoopThread = new Thread(WebServerProgram.ServerLoop);
			serverLoopThread.Start();

			// start a browser and the message loop
			Process.Start(localhostURL);
			Application.Run();
		}

		private static void ServerLoop()
		{
			TcpListener listener = new TcpListener(IPAddress.Any, WebServerProgram.ServerSettings.HTTPPort);
			listener.Start();
			while (true)
			{
				// accept a client
				TcpClient client = listener.AcceptTcpClient();

				// handle the client in a new thread
				Thread clientThread = new Thread(RequestHandler.HandleClient);
				clientThread.Start(client);
			}
		}

		#endregion
	}
}