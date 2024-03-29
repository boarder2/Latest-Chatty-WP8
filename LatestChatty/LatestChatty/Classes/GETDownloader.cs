﻿using System;
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
using System.Text;

namespace LatestChatty.Classes
{
	public class GETDownloader
	{
		public delegate void GETDelegate(IAsyncResult result);
		GETDelegate getCallback;
		HttpWebRequest request;
		protected bool cancelled;
        private readonly bool canCancel;

		public string Uri { get; private set; }


        public GETDownloader(string getURI, GETDelegate callback)
            : this(getURI, true, callback) { }

        public GETDownloader(string getURI, bool canCancel, GETDelegate callback)
        {
            this.Uri = getURI;
            getCallback = callback;
            this.canCancel = canCancel;
        }

		public void Start()
		{
			this.request = (HttpWebRequest)HttpWebRequest.Create(this.Uri);
			this.request.Method = "GET";
			this.request.Headers[HttpRequestHeader.CacheControl] = "no-cache";
			request.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(CoreServices.Instance.Credentials.UserName + ":" + CoreServices.Instance.Credentials.Password));
			//this.request.Credentials = CoreServices.Instance.Credentials;

			try
			{
				IAsyncResult token = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
			}
			catch (WebException)
			{
				//TODO: Catch cancellation exception and throw everything else.
				System.Diagnostics.Debugger.Break();
			}
		}

		public void Cancel()
		{
            if (!this.canCancel) return;

			request.Abort();
			System.Diagnostics.Debug.WriteLine("Cancelling download for {0}", request.RequestUri);
			this.cancelled = true;
		}

		public void ResponseCallback(IAsyncResult result)
		{
			if (!this.cancelled)
			{
				InvokeDelegate(result);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Skipping callback because request was cancelled.");
			}
		}

		virtual protected void InvokeDelegate(IAsyncResult result)
		{
			getCallback(result);
		}
	}
}
