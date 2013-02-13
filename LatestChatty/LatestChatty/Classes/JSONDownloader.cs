using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace LatestChatty.Classes
{
    public class JSONDownloaderAsync
    {
        TaskCompletionSource<JObject> taskCompletion;

        async public Task<JObject> GetJSON(string uri)
        {
            var downloader = new JSONDownloader(uri, this.DownloadComplete);
            this.taskCompletion = new TaskCompletionSource<JObject>();
            downloader.Start();
            await this.taskCompletion.Task.AsAsyncAction();
            return taskCompletion.Task.Result;
        }

        private void DownloadComplete(JObject response)
        {
            this.taskCompletion.SetResult(response);
        }
    }
	public class JSONDownloader : GETDownloader
	{
		public delegate void JSONDownloaderCallback(JObject response);
		public event EventHandler<EventArgs> Finished;
		private JSONDownloaderCallback doneCallback;

		public JSONDownloader(string getURI, JSONDownloaderCallback callback)
			: base(getURI, null)
		{
			this.doneCallback = callback;
		}

		protected override void InvokeDelegate(IAsyncResult result)
		{
			try
			{
				WebResponse response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
				StreamReader reader = new StreamReader(response.GetResponseStream());
				string responseString = reader.ReadToEnd();
				var jObject = JObject.Parse(responseString);

				//TODO: A lot of expensive stuff always happens here
				// We should consider allowing this to run on a separate thread and having the responders be responsible for getting the changes to the UI thread.
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					if (!this.cancelled) this.doneCallback(jObject);
				});
			}
			catch
			{
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					if (!this.cancelled) doneCallback(null);
				});
			}

			if (this.Finished != null)
			{
				if (!this.cancelled) this.Finished(this, EventArgs.Empty);
			}
		}
	}
}
