/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/
using System.Collections.Specialized;

namespace WebServer
{
	public class ResponseInfo
	{
		public string ResponseStatus { get; set; }
		public NameValueCollection ResponseHeaders { get; private set; }
		public byte[] ResponseBody { get; set; }

		public ResponseInfo()
		{
			this.ResponseHeaders = new NameValueCollection();
		}
	}
}