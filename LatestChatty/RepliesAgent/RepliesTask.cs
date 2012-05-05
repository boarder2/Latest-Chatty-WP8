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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using LatestChatty.Common;

namespace RepliesAgent
{
	public class RepliesTask : ScheduledTaskAgent
	{
		public static string TaskName = "LatestChatty";

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
			try
			{
#if DEBUG
			ScheduledActionService.LaunchForTest("LatestChatty", TimeSpan.FromSeconds(10));
#endif

				//IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
				if (!string.IsNullOrWhiteSpace(LatestChattySettings.Instance.Username))
				{
					username = LatestChattySettings.Instance.Username;
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

				this.lastInAppReplyCount = LatestChattySettings.Instance.LastInAppReplyCount;
				this.lastTileReplyCount = LatestChattySettings.Instance.LastTileReplyCount;

#if DEBUG
			var rand = new Random();
			this.lastInAppReplyCount -= rand.Next(99);
#endif

				var uri = "http://shackapi.stonedonkey.com/Search/?ParentAuthor=" + username;
				var request = (HttpWebRequest)HttpWebRequest.Create(uri);
				request.Method = "GET";
				request.Headers[HttpRequestHeader.CacheControl] = "no-cache";

				var token = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception: {0}", ex);
				//Always complete so the tile continues to refresh if the exception can be recovered from.
				NotifyComplete();
			}
		}

		private void ResponseCallback(IAsyncResult result)
		{
			var wait = new System.Threading.ManualResetEvent(true);

			try
			{
				//var settings = IsolatedStorageSettings.ApplicationSettings;
				var response = ((HttpWebRequest)result.AsyncState).EndGetResponse(result);
				var reader = new StreamReader(response.GetResponseStream());
				var responseString = reader.ReadToEnd();
				var XMLResponse = XDocument.Parse(responseString);

				var totalReplies = int.Parse(XMLResponse.Element("results").Attribute("total_results").Value);

				//Do nothing, there aren't any new replies that we don't already know about.
				if (totalReplies <= this.lastInAppReplyCount) goto notifyComplete;

				//Ok, so we've got a new reply
				var latestReply = XMLResponse.Descendants("result").FirstOrDefault();

				//This shouldn't happen, but... yeah.
				if (latestReply == null) goto notifyComplete;

				//Parse the body and author (I don't see any _target in the xml, so we'll take this out to save as much battery as possible.)
				var replyText = latestReply.Element("body").Value;
				var replyAuthor = latestReply.Attribute("author").Value;

				//Find the right tile to update.  If they've got the shortcut to chatty pinned, use it.  If not, use the default tile.
				var tileToUpdate =
					ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("ChattyPage"))
					?? ShellTile.ActiveTiles.FirstOrDefault();

				//If they don't have it pinned at all there won't be any tile
				if (tileToUpdate != null && (LatestChattySettings.Instance.NotificationType == NotificationType.Tile || LatestChattySettings.Instance.NotificationType == NotificationType.TileAndToast))
				{
					//If there's a tile to update, we need to block NotifyComplete until the image is fully loaded.
					wait.Reset();
					Deployment.Current.Dispatcher.BeginInvoke(() =>
						{
							var newTilePath = "/Shared/ShellContent/LCFrontTileImage.jpg";

							var newTileImage = new BitmapImage(new Uri("ApplicationIcon-NoLamp.png", UriKind.Relative));
							newTileImage.CreateOptions = BitmapCreateOptions.None;
							newTileImage.ImageOpened += (sender, e) =>
							{
								try
								{
									var textToWrite = new System.Windows.Controls.TextBlock()
									{
										Width = 80,
										Height = 80,
										FontSize = 80,
										VerticalAlignment = VerticalAlignment.Center,
										Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
										TextAlignment = TextAlignment.Right,
										Text = (totalReplies - this.lastInAppReplyCount).ToString(),
									};

									var backgroundImage = new System.Windows.Controls.Image();
									backgroundImage.Source = newTileImage;

									using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
									{
										var finalBitmap = new WriteableBitmap(173, 173);
										finalBitmap.Render(backgroundImage, new TranslateTransform());
										finalBitmap.Render(textToWrite, new TranslateTransform() { X = 85, Y = 2 });
										if (isoStore.FileExists(newTilePath)) { isoStore.DeleteFile(newTilePath); }
										using (var writeStream = isoStore.CreateFile(newTilePath))
										{
											//Draws the bitmap
											finalBitmap.Invalidate();
											finalBitmap.SaveJpeg(writeStream, finalBitmap.PixelWidth, finalBitmap.PixelHeight, 0, 100);
											writeStream.Close();
										}
									}

									var tileData = new StandardTileData();
									tileData.BackgroundImage = new Uri(string.Format("isostore:{0}", newTilePath), UriKind.Absolute);
									tileData.BackTitle = replyAuthor;
									tileData.BackContent = replyText;

									tileToUpdate.Update(tileData);
								}
								finally
								{
									wait.Set();
								}
							};
						});
				}

				//If we don't have any more replies that we haven't toasted, bail now.  Otherwise let's toast!
				if (totalReplies <= this.lastTileReplyCount) goto notifyComplete;

				//Update the last reply count.
				LatestChattySettings.Instance.LastTileReplyCount = totalReplies;

				if (LatestChattySettings.Instance.NotificationType == NotificationType.TileAndToast)
				{
					var toast = new ShellToast();
					toast.Title = replyAuthor + " replied:";
					toast.Content = replyText;
					toast.NavigationUri = new Uri("/Pages/ThreadPage.xaml?Comment=" + int.Parse(latestReply.Attribute("id").Value) + "&Story=" + int.Parse(latestReply.Attribute("story_id").Value), UriKind.Relative);
					toast.Show();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception: {0}", ex);
				//If we get an exception, just exit out so the process isn't hung waiting
				wait.Set();
			}


		notifyComplete:

			wait.WaitOne();
			NotifyComplete();
		}
	}
}
