/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Web;

namespace WebServer
{
	public class RequestInfo
	{
		public string HTTPVersion { get; private set; }
		public string RequestMethod { get; private set; }
		public string RequestPath { get; private set; }
		public string QueryString { get; private set; }
		public NameValueCollection RequestHeaders { get; private set; }
		public byte[] RequestBody { get; private set; }

		public string RequestPathAbsolute
		{
			get
			{
				// get absolute path
				string requestPathAbsolute = Path.GetFullPath(string.Format("{0}{1}", WebServerProgram.ServerSettings.DocumentRoot, this.RequestPath));

				// default document
				if (requestPathAbsolute.EndsWith(@"\".ToString()))
				{
					foreach (string defaultDocument in WebServerProgram.DefaultDocuments)
					{
						if (File.Exists(requestPathAbsolute + defaultDocument))
							requestPathAbsolute += defaultDocument;
					}
				}

				// return
				return requestPathAbsolute;
			}
		}

		private RequestInfo()
		{

		}

		public static RequestInfo FromRequestStream(NetworkStream stream, DateTime startTime)
		{
			RequestInfo requestInfo = new RequestInfo();

			// wait for data...
			while (!stream.DataAvailable)
			{
				if ((DateTime.Now - startTime).TotalSeconds > WebServerProgram.ServerSettings.RequestTimeoutSeconds)
					return null;
				Thread.Sleep(10);
			}

			// read the HTTP command and request headers
			string httpCommand = "", requestHeadersStr = "";
			while (!requestHeadersStr.EndsWith("\r\n\r\n"))
			{
				int val = stream.ReadByte();
				if (!httpCommand.EndsWith("\r\n"))
					httpCommand += (char)val;
				else
					requestHeadersStr += (char)val;
			}

			// parse headers
			requestInfo.RequestHeaders = RequestInfo.ParseHeaders(requestHeadersStr);

			// read the request body
			requestInfo.RequestBody = new byte[] { };
			if (!string.IsNullOrEmpty(requestInfo.RequestHeaders["Content-Length"]))
			{
				int contentLength = int.Parse(requestInfo.RequestHeaders["Content-Length"]);
				requestInfo.RequestBody = new byte[contentLength];

				// read contentLength bytes into requestBody
				int read = 0;
				while (read < contentLength)
					read += stream.Read(requestInfo.RequestBody, read, contentLength - read);
			}

			// parse the HTTP command
			httpCommand = httpCommand.Trim();
			string[] httpCommandParts = httpCommand.Split(' ');
			requestInfo.RequestMethod = httpCommandParts[0];
			string pathAndQuery = httpCommandParts[1];
			requestInfo.HTTPVersion = httpCommandParts[2];

			// split the path and query
			int indexOfQuery = pathAndQuery.IndexOf('?');
			requestInfo.RequestPath = (indexOfQuery == -1) ? pathAndQuery : pathAndQuery.Substring(0, indexOfQuery);
			requestInfo.QueryString = (indexOfQuery == -1) ? "" : pathAndQuery.Substring(indexOfQuery + 1);

			// url decode the path
			requestInfo.RequestPath = HttpUtility.UrlDecode(requestInfo.RequestPath);

			// return
			return requestInfo;
		}

		public static NameValueCollection ParseHeaders(string headers)
		{
			string[] headerLines = headers.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			NameValueCollection headerCollection = new NameValueCollection();
			foreach (string header in headerLines)
			{
				string[] headerParts = header.Split(new string[] { ": " }, 2, StringSplitOptions.None);
				headerCollection.Add(headerParts[0].Trim(), headerParts[1].Trim());
			}
			return headerCollection;
		}
	}
}