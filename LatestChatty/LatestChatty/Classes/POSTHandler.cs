using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using LatestChatty.Settings;
using System.Text;

namespace LatestChatty.Classes
{
	public class POSTHandler
	{
		private string content;
		public delegate void POSTDelegate(bool success);
		POSTDelegate postCompleteEvent;
		private bool showExceptionMessageBox;

		public POSTHandler(string postUrl, string content, POSTDelegate callback)
		{
			System.Diagnostics.Debug.WriteLine("Posting: {0}", content);

			this.content = content;
			this.postCompleteEvent = callback;

			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(postUrl);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Headers[HttpRequestHeader.Authorization] = Convert.ToBase64String(Encoding.UTF8.GetBytes(CoreServices.Instance.Credentials.UserName + ":" + CoreServices.Instance.Credentials.Password));
			request.Credentials = CoreServices.Instance.Credentials;

			this.showExceptionMessageBox = true;

			IAsyncResult token = request.BeginGetRequestStream(new AsyncCallback(BeginPostCallback), request);
		}

		public void BeginPostCallback(IAsyncResult result)
		{
			HttpWebRequest request = result.AsyncState as HttpWebRequest;

			Stream requestStream = request.EndGetRequestStream(result);
			StreamWriter streamWriter = new StreamWriter(requestStream);
			streamWriter.Write(content);
			streamWriter.Flush();
			streamWriter.Close();

			request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
		}

		public void ResponseCallback(IAsyncResult result)
		{
			var success = false;
			var failureMessage = string.Empty;

			try
			{
				HttpWebRequest request = result.AsyncState as HttpWebRequest;
				HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

				//Doesn't seem like the API is actually returning failure codes, but... might as well handle it in case it does some time.
				if (response.StatusCode != HttpStatusCode.OK)
				{
					failureMessage = "Bad response code.  Check your username and password.";
				}
				else
				{
					success = true;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("HTTP Send failed because: {0}", ex);
				failureMessage = "Posting failed.";
			}

			finally
			{
				Deployment.Current.Dispatcher.BeginInvoke(() =>
						{
							if (!success && this.showExceptionMessageBox)
							{
								MessageBox.Show(failureMessage);
							}
							if (postCompleteEvent != null)
							{
								postCompleteEvent(success);
							}
						});
			}
		}
	}
}
