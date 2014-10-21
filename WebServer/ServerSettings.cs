/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/

namespace WebServer
{
	public class ServerSettings
	{
		public int HTTPPort { get; set; }

		public string DocumentRoot { get; set; }

		public string PHPExecutable { get; set; }

		public int RequestTimeoutSeconds { get; set; }

		public ServerSettings()
		{
			this.HTTPPort = 9050;
			this.DocumentRoot = @"..\..\web\";
			this.PHPExecutable = @"..\..\php\php-cgi.exe";
			this.RequestTimeoutSeconds = 5;
		}
	}
}