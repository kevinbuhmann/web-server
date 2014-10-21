/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
	public class RequestHandler
	{
		#region Fields

		private DateTime startTime;
		private TcpClient tcpClient;
		private NetworkStream stream;
		private RequestInfo requestInfo;
		private ResponseInfo responseInfo;

		#endregion

		#region Constructors

		private RequestHandler(TcpClient tcpClient)
		{
			this.startTime = DateTime.Now;
			this.tcpClient = tcpClient;
			this.stream = this.tcpClient.GetStream();
		}

		#endregion

		#region Static Methods

		public static void HandleClient(object tcpClientObj)
		{
			TcpClient tcpClient = (tcpClientObj as TcpClient);
			if (tcpClient == null)
				throw new ArgumentException("tcpClientObj must be an instance of TcpClient.", "tcpClientObj");

			new RequestHandler(tcpClient).HandleRequest();
		}

		private static byte[] GZipCompress(byte[] rawData)
		{
			using (MemoryStream tempStream = new MemoryStream())
			{
				using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Compress, true))
					gzipStream.Write(rawData, 0, rawData.Length);
				return tempStream.ToArray();
			}
		}

		#endregion

		#region Methods

		private void HandleRequest()
		{
			// initialize response
			this.responseInfo = new ResponseInfo();
			this.responseInfo.ResponseHeaders["Server"] = "PHP MVS Razor Web Server";
			this.responseInfo.ResponseHeaders["Connection"] = "keep-alive";

			try
			{
				this.requestInfo = RequestInfo.FromRequestStream(this.stream, startTime);
				if (this.requestInfo != null)
				{
					// log the TCP end points and the requested path
					File.AppendAllText("log.txt", string.Format("{0}, {1}\r\n",
						this.tcpClient.Client.RemoteEndPoint, this.requestInfo.RequestPath));

					this.ProcessHTTPRequest();
				}
			}
			catch (Exception e)
			{
				this.responseInfo.ResponseStatus = "500 Internal Server Error";
				this.responseInfo.ResponseHeaders["Content-Type"] = "text/plain";
				this.responseInfo.ResponseBody = Encoding.ASCII.GetBytes(e.ToString());
			}

			// compress and write response
			this.CompressResponse();
			this.WriteHTTPResponse();

			// recurse for next request if using HTTP keep-alive
			bool keepAliveRequested = (this.requestInfo != null) && (this.requestInfo.RequestHeaders["Connection"] == "keep-alive");
			if (keepAliveRequested && ((DateTime.Now - startTime).TotalSeconds < WebServerProgram.ServerSettings.RequestTimeoutSeconds))
				this.HandleRequest();
			else
			{
				this.stream.Close();
				this.tcpClient.Close();
			}
		}

		private void ProcessHTTPRequest()
		{
			if ((this.requestInfo.HTTPVersion != "HTTP/1.0") && (this.requestInfo.HTTPVersion != "HTTP/1.1"))
				this.responseInfo.ResponseStatus = "400 Invalid HTTP version";
			else
				this.responseInfo = RequestExecutor.ExecuteRequest(this.requestInfo);
		}

		private void CompressResponse()
		{
			if ((this.responseInfo.ResponseBody != null) && (this.responseInfo.ResponseBody.Length > 0))
			{
				string acceptEncoding = this.requestInfo.RequestHeaders["Accept-Encoding"];
				bool clientAcceptsGzip = !string.IsNullOrEmpty(acceptEncoding) && acceptEncoding.ToLower().Contains("gzip");
				if (clientAcceptsGzip)
				{
					byte[] compressedResponseBody = RequestHandler.GZipCompress(this.responseInfo.ResponseBody);
					if (compressedResponseBody.Length < this.responseInfo.ResponseBody.Length)
					{
						this.responseInfo.ResponseHeaders["Content-Encoding"] = "gzip";
						this.responseInfo.ResponseBody = compressedResponseBody;
					}
				}
			}
		}

		private void WriteHTTPResponse()
		{
			try
			{
				// add the Content-Length header
				this.responseInfo.ResponseHeaders["Content-Length"] = (this.responseInfo.ResponseBody != null) ?
					this.responseInfo.ResponseBody.Length.ToString() : "0";

				// write HTTP headers
				StreamWriter writer = new StreamWriter(this.stream);
				writer.WriteLine("HTTP/1.0 {0}", this.responseInfo.ResponseStatus);
				foreach (string header in this.responseInfo.ResponseHeaders.Keys)
					writer.WriteLine("{0}: {1}", header, this.responseInfo.ResponseHeaders[header]);
				writer.WriteLine();
				writer.Flush();

				// write the content
				if (this.responseInfo.ResponseBody != null)
					this.stream.Write(this.responseInfo.ResponseBody, 0, this.responseInfo.ResponseBody.Length);
			}
			catch (Exception) { }
		}

		#endregion
	}
}