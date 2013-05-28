using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml.Linq;

namespace LatestChatty.Classes
{
	public class XMLDownloader : GETDownloader
	{
		public delegate void XMLDownloaderCallback(XDocument response);
		public event EventHandler<EventArgs> Finished;
		private XMLDownloaderCallback _delegate;

		public XMLDownloader(string getURI, XMLDownloaderCallback callback)
			: base(getURI, null)
		{
			_delegate = callback;
		}

		protected override void InvokeDelegate(IAsyncResult result)
		{
			try
			{
				var response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
				var reader = new StreamReader(response.GetResponseStream());
				var responseString = reader.ReadToEnd();
				var XMLResponse = XDocument.Parse(responseString);

				//TODO: A lot of expensive stuff always happens here
				// We should consider allowing this to run on a separate thread and having the responders be responsible for getting the changes to the UI thread.
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					if (!this.cancelled) _delegate(XMLResponse);
				});
			}
			catch
			{
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					if (!this.cancelled) _delegate(null);
				});
			}

			if (this.Finished != null)
			{
				if (!this.cancelled) this.Finished(this, EventArgs.Empty);
			}
		}
	}
}
