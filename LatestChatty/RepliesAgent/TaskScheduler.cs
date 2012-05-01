using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using System.Net;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Text.RegularExpressions;

namespace RepliesAgent
{
	public class ScheduledAgent : ScheduledTaskAgent
	{
		/// <summary>
		/// Agent that runs a scheduled task
		/// </summary>
		/// <param name="task">
		/// The invoked task
		/// </param>
		/// <remarks>
		/// This method is called when a periodic or resource intensive task is invoked
		/// </remarks>
		private int lastInAppReplyCount = 0;
		private int lastTileReplyCount = 0;

		protected override void OnInvoke(ScheduledTask task)
		{
			string username;

			IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains("username"))
			{
				username = settings["username"].ToString();
			}
			else
			{
				var toast = new ShellToast();
				toast.Title = "LatestChatty: ";
				toast.Content = "Please login to receive reply notifications";
				toast.NavigationUri = new Uri("/", UriKind.Relative);
				toast.Show();
				NotifyComplete();
				return;
			}

			//Clean up old persisted id.
			if (settings.Contains("lastReplySeen"))
			{
				settings.Remove("lastReplySeen");
			}

			if (settings.Contains("LastInAppReplyCount"))
			{
				this.lastInAppReplyCount = int.Parse(settings["LastInAppReplyCount"].ToString());
			}

			if (settings.Contains("LastTileReplyCount"))
			{
				this.lastTileReplyCount = int.Parse(settings["LastInAppReplyCount"].ToString());
			}

			var uri = "http://shackapi.stonedonkey.com/Search/?ParentAuthor=" + username;
			var request = (HttpWebRequest)HttpWebRequest.Create(uri);
			request.Method = "GET";
			request.Headers[HttpRequestHeader.CacheControl] = "no-cache";

			var token = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
		}

		public void ResponseCallback(IAsyncResult result)
		{
			try
			{
				var settings = IsolatedStorageSettings.ApplicationSettings;
				var response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
				var reader = new StreamReader(response.GetResponseStream());
				var responseString = reader.ReadToEnd();
				var XMLResponse = XDocument.Parse(responseString);

				var totalReplies = int.Parse(XMLResponse.Element("results").Attribute("total_results").Value);

				//Do nothing, there aren't any new replies that we don't already know about.
				if (totalReplies <= this.lastInAppReplyCount) return;

				//Ok, so we've got a new reply
				var latestReply = XMLResponse.Descendants("result").FirstOrDefault();

				//This shouldn't happen, but... yeah.
				if (latestReply == null) return;

				//Parse the body and author (I don't see any _target in the xml, so we'll take this out to save as much battery as possible.)
				var replyText = latestReply.Element("body").Value;
				var replyAuthor = latestReply.Attribute("author").Value;

				//Find the right tile to update.  If they've got the shortcut to chatty pinned, use it.  If not, use the default tile.
				var tileToUpdate = 
					ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("ChattyPage"))
					?? ShellTile.ActiveTiles.FirstOrDefault();

				//If they don't have it pinned at all there won't be any tile
				if (tileToUpdate != null)
				{
					var tileData = new StandardTileData();

					tileData.Count = totalReplies - this.lastInAppReplyCount;
					tileData.BackTitle = replyText;
					tileData.BackContent = replyAuthor;

					tileToUpdate.Update(tileData);
				}

				//If we don't have any more replies that we haven't toasted, bail now.  Otherwise let's toast!
				if (totalReplies < this.lastTileReplyCount) return;

				//Update the last reply count.
				if (settings.Contains("LastTileReplyCount"))
				{
					settings["LastInAppReplyCount"] = totalReplies;
					settings.Save();
				}

				var toast = new ShellToast();
				toast.Title = "LatestChatty: " + replyAuthor;
				toast.Content = replyText;
				toast.NavigationUri = new Uri("/Pages/ThreadPage.xaml?Comment=" + int.Parse(latestReply.Attribute("id").Value) + "&Story=" + int.Parse(latestReply.Attribute("story_id").Value), UriKind.Relative);
				toast.Show();
			}
			catch
			{
			}

			NotifyComplete();
		}
	}
}
