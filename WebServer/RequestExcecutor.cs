/**************************************************
 * Web Server                                     *
 * Author: Kevin Phelps                           *
 * https://github.com/kevinphelps/WebServer       *
 * http://kevinphelps.me/                         *
 * Copyright 2014 Kevin Phelps                    *
 **************************************************/
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WebServer
{
	public static class RequestExecutor
	{
		public static ResponseInfo ExecuteRequest(RequestInfo requestInfo)
		{
			bool fileExists = File.Exists(requestInfo.RequestPathAbsolute);
			string extension = Path.GetExtension(requestInfo.RequestPathAbsolute).ToLower();

			if (fileExists && extension == ".php")
				return RequestExecutor.HandlePHP(requestInfo);
			else if (fileExists)
				return RequestExecutor.HandleFile(requestInfo);
			else
				return RequestExecutor.Handle404(requestInfo);
		}

		private static ResponseInfo HandleFile(RequestInfo requestInfo, string filenameOverride = null)
		{
			string filename = filenameOverride ?? requestInfo.RequestPathAbsolute;
			string extension = Path.GetExtension(filename).ToLower();

			ResponseInfo responseInfo = new ResponseInfo();
			responseInfo.ResponseBody = File.ReadAllBytes(filename);
			responseInfo.ResponseHeaders["Content-Type"] = RequestExecutor.GetContentType(extension);
			responseInfo.ResponseHeaders["Content-Length"] = responseInfo.ResponseBody.Length.ToString();
			return responseInfo;
		}

		private static ResponseInfo HandlePHP(RequestInfo requestInfo, string filenameOverride = null)
		{
			string filename = filenameOverride ?? requestInfo.RequestPathAbsolute;

			// execute php-cgi.exe
			Process phpProcess = new Process();
			phpProcess.StartInfo.CreateNoWindow = true;
			phpProcess.StartInfo.UseShellExecute = false;
			phpProcess.StartInfo.RedirectStandardInput = true;
			phpProcess.StartInfo.RedirectStandardOutput = true;
			phpProcess.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("latin1");
			phpProcess.StartInfo.FileName = WebServerProgram.ServerSettings.PHPExecutable;
			phpProcess.StartInfo.EnvironmentVariables["GATEWAY_INTERFACE"] = "CGI/1.1";
			phpProcess.StartInfo.EnvironmentVariables["SERVER_PORT"] = WebServerProgram.ServerSettings.HTTPPort.ToString();
			phpProcess.StartInfo.EnvironmentVariables["SERVER_PROTOCOL"] = requestInfo.HTTPVersion;
			phpProcess.StartInfo.EnvironmentVariables["REDIRECT_STATUS"] = "302 Redirect";
			phpProcess.StartInfo.EnvironmentVariables["DOCUMENT_ROOT"] = Path.GetFullPath(WebServerProgram.ServerSettings.DocumentRoot);
			phpProcess.StartInfo.EnvironmentVariables["SCRIPT_FILENAME"] = filename;
			phpProcess.StartInfo.EnvironmentVariables["QUERY_STRING"] = requestInfo.QueryString;
			phpProcess.StartInfo.EnvironmentVariables["REQUEST_METHOD"] = requestInfo.RequestMethod;
			phpProcess.StartInfo.EnvironmentVariables["HTTP_ACCEPT"] = requestInfo.RequestHeaders["Accept-Encoding"];
			phpProcess.StartInfo.EnvironmentVariables["HTTP_USER_AGENT"] = requestInfo.RequestHeaders["User-Agent"];
			phpProcess.StartInfo.EnvironmentVariables["CONTENT_TYPE"] = requestInfo.RequestHeaders["Content-Type"];
			phpProcess.StartInfo.EnvironmentVariables["CONTENT_LENGTH"] = requestInfo.RequestBody.Length.ToString();
			phpProcess.StartInfo.EnvironmentVariables["REMOTE_ADDR"] = "";
			phpProcess.StartInfo.EnvironmentVariables["REMOTE_HOST"] = "";
			phpProcess.Start();

			// input the request body
			phpProcess.StandardInput.BaseStream.Write(requestInfo.RequestBody, 0, requestInfo.RequestBody.Length);
			phpProcess.StandardInput.BaseStream.Flush();

			// capture php-cgi.exe output
			string phpHeadersStr = "";
			List<byte> content = new List<byte>();
			int val;
			while ((val = phpProcess.StandardOutput.Read()) != -1)
			{
				if (!phpHeadersStr.EndsWith("\r\n\r\n"))
					phpHeadersStr += (char)val;
				else
					content.Add((byte)val);
			}

			// wait php-cgi.exe for exit
			phpProcess.WaitForExit();

			// start creating the response
			ResponseInfo responseInfo = new ResponseInfo();

			// add the headers returned by php-cgi.ext
			NameValueCollection phpHeaders = RequestInfo.ParseHeaders(phpHeadersStr);
			foreach (string header in phpHeaders.Keys)
			{
				if (header == "Status")
					responseInfo.ResponseStatus = phpHeaders["Status"];
				else
					responseInfo.ResponseHeaders[header] = phpHeaders[header];
			}

			// set the response body
			responseInfo.ResponseBody = content.ToArray();

			// return the response info
			return responseInfo;
		}

		private static ResponseInfo Handle404(RequestInfo requestInfo)
		{
			ResponseInfo responseInfo = new ResponseInfo();
			responseInfo.ResponseStatus = "404 Not Found";
			return responseInfo;
		}

		private static string GetContentType(string extension)
		{
			string contentType = "application/unknown";
			RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(extension);
			if (regKey != null && regKey.GetValue("Content Type") != null)
				contentType = regKey.GetValue("Content Type").ToString();
			return contentType;
		}
	}
}