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
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;
using LatestChatty.Classes;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.ObjectModel;

namespace LatestChatty.Settings
{
	public class LatestChattySettings
	{
		private static readonly string commentSize = "CommentSize";
		private static readonly string threadNavigationByDate = "ThreadNavigationByDate";
		private static readonly string showInlineImages = "ShowInline";
		private static readonly string notificationType = "NotificationType";
		private static readonly string username = "username";
		private static readonly string password = "password";
		private static readonly string notificationUID = "notificationid";
		private static readonly string autocollapsenws = "autocollapsenws";
		private static readonly string autocollapsestupid = "autocollapsestupid";
		private static readonly string autocollapseofftopic = "autocollapseofftopic";
		private static readonly string autocollapsepolitical = "autocollapsepolitical";
		private static readonly string autocollapseinformative = "autocollapseinformative";
		private static readonly string autocollapseinteresting = "autocollapseinteresting";
		private static readonly string autopinonreply = "autopinonreply";
		private static readonly string autoremoveonexpire = "autoremoveonexpire";
		private static readonly string cloudsync = "cloudsync";

		private int commentsLeftToLoad;
		private bool loadingSettingsInternal;
		private JObject cloudSettings = null;

		public readonly IsolatedStorageSettings isoStore;

		public event EventHandler SettingsSynced;

		private static LatestChattySettings instance = null;
		public static LatestChattySettings Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new LatestChattySettings();
				}
				return instance;
			}
		}

		//TODO: This can probably be optimized to cache values on-load and then only update when setters are called.
		//Otherwise, it's hitting the isostore every time and I don't know if that does caching or not. One would hope so.
		public LatestChattySettings()
		{
			this.isoStore = IsolatedStorageSettings.ApplicationSettings;
			this.pinnedCommentsCollection = new ObservableCollection<Comment>();
			this.PinnedComments = new ReadOnlyObservableCollection<Comment>(this.pinnedCommentsCollection);

			if (!this.isoStore.Contains(notificationType))
			{
				this.isoStore.Add(notificationType, NotificationType.None);
			}
			if (!this.isoStore.Contains(commentSize))
			{
				this.isoStore.Add(commentSize, CommentViewSize.Small);
			}
			if (!this.isoStore.Contains(threadNavigationByDate))
			{
				this.isoStore.Add(threadNavigationByDate, true);
			}
			if (!this.isoStore.Contains(showInlineImages))
			{
				this.isoStore.Add(showInlineImages, ShowInlineImages.Always);
			}
			if (!this.isoStore.Contains(username))
			{
				this.isoStore.Add(username, string.Empty);
			}
			if (!this.isoStore.Contains(password))
			{
				this.isoStore.Add(password, string.Empty);
			}
			if (!this.isoStore.Contains(notificationUID))
			{
				this.isoStore.Add(notificationUID, Guid.NewGuid());
			}
			if (!this.isoStore.Contains(autocollapsenws))
			{
				this.isoStore.Add(autocollapsenws, true);
			}
			if (!this.isoStore.Contains(autocollapsestupid))
			{
				this.isoStore.Add(autocollapsestupid, false);
			}
			if (!this.isoStore.Contains(autocollapseofftopic))
			{
				this.isoStore.Add(autocollapseofftopic, false);
			}
			if (!this.isoStore.Contains(autocollapsepolitical))
			{
				this.isoStore.Add(autocollapsepolitical, false);
			}
			if (!this.isoStore.Contains(autocollapseinformative))
			{
				this.isoStore.Add(autocollapseinformative, false);
			}
			if (!this.isoStore.Contains(autocollapseinteresting))
			{
				this.isoStore.Add(autocollapseinteresting, false);
			}
			if (!this.isoStore.Contains(autopinonreply))
			{
				this.isoStore.Add(autopinonreply, false);
			}
			if (!this.isoStore.Contains(autoremoveonexpire))
			{
				this.isoStore.Add(autoremoveonexpire, false);
			}
			if (!this.isoStore.Contains(cloudsync))
			{
				this.isoStore.Add(cloudsync, false);
			}
		}

		#region Properties
		private ObservableCollection<Comment> pinnedCommentsCollection;
		public ReadOnlyObservableCollection<Comment> PinnedComments
		{
			get;
			private set;
		}
		private List<int> pinnedCommentIds = new List<int>();

		public bool CloudSync
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(cloudsync, out v);
				return v;
			}
			set
			{
				this.isoStore[cloudsync] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoCollapseNws
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autocollapsenws, out v);
				return v;
			}
			set
			{
				this.isoStore[autocollapsenws] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoCollapseStupid
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autocollapsestupid, out v);
				return v;
			}
			set
			{
				this.isoStore[autocollapsestupid] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoCollapseOffTopic
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autocollapseofftopic, out v);
				return v;
			}
			set
			{
				this.isoStore[autocollapseofftopic] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoCollapsePolitical
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autocollapsepolitical, out v);
				return v;
			}
			set
			{
				this.isoStore[autocollapsepolitical] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoCollapseInformative
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autocollapseinformative, out v);
				return v;
			}
			set
			{
				this.isoStore[autocollapseinformative] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoCollapseInteresting
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autocollapseinteresting, out v);
				return v;
			}
			set
			{
				this.isoStore[autocollapseinteresting] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoPinOnReply
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue<bool>(autopinonreply, out v);
				return v;
			}
			set
			{
				this.isoStore[autopinonreply] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public bool AutoRemoveOnExpire
		{
			get
			{
				bool v;
				this.isoStore.TryGetValue(autoremoveonexpire, out v);
				return v;
			}
			set
			{
				this.isoStore[autoremoveonexpire] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}

		public Guid NotificationID
		{
			get
			{
				Guid v;
				this.isoStore.TryGetValue<Guid>(notificationUID, out v);
				return v;
			}
			//set
			//{
			//   this.isoStore[notificationUID] = value;
			//   this.isoStore.Save();
			//}
		}

		public NotificationType NotificationType
		{
			get
			{
				NotificationType v;
				this.isoStore.TryGetValue<NotificationType>(notificationType, out v);
				return v;
			}
			set
			{
				this.isoStore[notificationType] = value;
				this.isoStore.Save();
			}
		}

		public CommentViewSize CommentSize
		{
			get
			{
				CommentViewSize size;
				this.isoStore.TryGetValue<CommentViewSize>(commentSize, out size);
				return size;
			}
			set
			{
				this.isoStore[commentSize] = value;
				this.isoStore.Save();
			}
		}

		public ShowInlineImages ShowInlineImages
		{
			get
			{
				ShowInlineImages val;
				this.isoStore.TryGetValue<ShowInlineImages>(showInlineImages, out val);
				return val;
			}
			set
			{
				this.isoStore[showInlineImages] = value;
				this.isoStore.Save();
				this.SaveToCloud();
			}
		}


		public bool ThreadNavigationByDate
		{
			get
			{
				return (bool)this.isoStore[threadNavigationByDate];
			}
			set
			{
				this.isoStore[threadNavigationByDate] = value;
				this.isoStore.Save();
			}
		}

		public string Username
		{
			get
			{
				return this.isoStore[username].ToString();
			}
			set
			{
				this.isoStore[username] = value;
				this.isoStore.Save();
			}
		}

		public string Password
		{
			get
			{
				return this.isoStore[password].ToString();
			}
			set
			{
				this.isoStore[password] = value;
				this.isoStore.Save();
			}
		}
		#endregion

		//This should be in an extension method since it's app specific, but... meh.
		public bool ShouldShowInlineImages
		{
			get
			{
				ShowInlineImages val;
				this.isoStore.TryGetValue<ShowInlineImages>(showInlineImages, out val);

				if (val == ShowInlineImages.Never)
				{
					return false;
				}

				if (val == ShowInlineImages.OnWiFi)
				{
					var type = NetworkInterface.NetworkInterfaceType;
					return type == NetworkInterfaceType.Ethernet ||
						 type == NetworkInterfaceType.Wireless80211;
				}

				//Always.
				return true;
			}
		}

		public void AddWatchedComment(Comment c)
		{
			if (!this.pinnedCommentIds.Contains(c.id))
			{
				this.pinnedCommentIds.Add(c.id);
				this.pinnedCommentsCollection.Add(c);
				this.SaveToCloud();
			}
		}

		public void RemoveWatchedComment(Comment c)
		{
			if (this.pinnedCommentIds.Contains(c.id))
			{
				var cRemove = this.pinnedCommentsCollection.SingleOrDefault(c1 => c1.id == c.id);
				if (cRemove != null)
				{
					this.pinnedCommentIds.Remove(c.id);
					this.pinnedCommentsCollection.Remove(cRemove);
					this.SaveToCloud();
				}
			}
		}

		public void LoadLongRunningSettings()
		{
			this.loadingSettingsInternal = true;
			this.pinnedCommentsCollection.Clear();
			if (!this.CloudSync)
			{
				try
				{
					DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

					using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
					{
						using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.OpenOrCreate, isf))
						{
							this.pinnedCommentIds = ser.ReadObject(stream) as List<int>;
						}
					}
				}

				catch { }

				this.GetPinnedComments();
				this.loadingSettingsInternal = false;
			}
			else
			{
				try
				{
					//TODO: Don't let these downloads be canceleable.
					var downloader = new JSONDownloader(Locations.MyCloudSettings, GotCloudSettings);
					downloader.Start();
				}
				catch (WebException e)
				{
					var r = e.Response as HttpWebResponse;
					if (r != null)
					{
						if (r.StatusCode == HttpStatusCode.Forbidden || r.StatusCode == HttpStatusCode.NotFound)
						{
							return;
						}
					}
					throw;
				}
			}
		}


		private void GetCommentsCallback(XDocument response)
		{
			try
			{
				//Don't throw an exception if we didn't get anything.
				//I found this can happen when you pin a post that later gets nuked.  Ultimately it should be removed from the list of stuff to retrieve, probably, but for now we just won't error.
				if (response != null)
				{
					XElement x = response.Elements("comments").Elements("comment").First();
					var storyId = int.Parse(response.Element("comments").Attribute("story_id").Value);
					//Don't save the counts when we load these posts.
					var comment = new Comment(x, storyId, false, 0);
					var insertAt = 0;
					//Sort them the same all the time.
					for (insertAt = 0; insertAt < this.pinnedCommentsCollection.Count; insertAt++)
					{
						//Keep looking
						if (comment.id > this.pinnedCommentsCollection[insertAt].id)
						{
							continue;
						}
						//Already exists... don't add it twice.  (This could happen if they click refresh fast)
						if (comment.id == this.pinnedCommentsCollection[insertAt].id)
						{
							return;
						}
						//We belong before this one.
						if (comment.id < this.pinnedCommentsCollection[insertAt].id)
						{
							break;
						}
					}
					this.pinnedCommentsCollection.Insert(insertAt, comment);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem refreshing pinned comments.");
			}
			finally
			{
				this.commentsLeftToLoad--;
				this.OnRefreshCompleted();
			}
		}

		private void GotCloudSettings(JObject result)
		{
			if (result != null)
			{
				this.cloudSettings = result;
				this.AutoCollapseInformative = result[autocollapseinformative] != null ? (bool)result[autocollapseinformative] : true;
				this.AutoCollapseInteresting = result[autocollapseinteresting] != null ? (bool)result[autocollapseinteresting] : true;
				this.AutoCollapseNws = result[autocollapsenws] != null ? (bool)result[autocollapsenws] : true;
				this.AutoCollapseOffTopic = result[autocollapseofftopic] != null ? (bool)result[autocollapseofftopic] : true;
				this.AutoCollapsePolitical = result[autocollapsepolitical] != null ? (bool)result[autocollapsepolitical] : true;
				this.AutoCollapseStupid = result[autocollapsestupid] != null ? (bool)result[autocollapsestupid] : true;
				this.AutoPinOnReply = (result[autopinonreply] != null) ? (bool)result[autopinonreply] : false;
				this.AutoRemoveOnExpire = (result[autoremoveonexpire] != null) ? (bool)result[autoremoveonexpire] : false;
				this.ShowInlineImages = (ShowInlineImages)Enum.Parse(typeof(ShowInlineImages), result[showInlineImages] != null ? (string)result[showInlineImages] : "Always", true);

				this.pinnedCommentIds = result["watched"].Children().Select(t => (int)t).ToList();
				this.GetPinnedComments();
			}

			this.loadingSettingsInternal = false;
		}

		public void SaveToCloud()
		{
			try
			{
				//If cloud sync is enabled
				if (!this.loadingSettingsInternal && LatestChattySettings.Instance.CloudSync)
				{
					System.Diagnostics.Debug.WriteLine("Syncing to cloud...");
					//If we don't have settings already, create them.
					if (this.cloudSettings == null)
					{
						this.cloudSettings = new JObject(
								  new JProperty("watched",
										new JArray(this.pinnedCommentIds)
										),
								  new JProperty(showInlineImages, this.ShowInlineImages),
								  new JProperty(autocollapseinformative, this.AutoCollapseInformative),
								  new JProperty(autocollapseinteresting, this.AutoCollapseInteresting),
								  new JProperty(autocollapsenws, this.AutoCollapseNws),
								  new JProperty(autocollapseofftopic, this.AutoCollapseOffTopic),
								  new JProperty(autocollapsepolitical, this.AutoCollapsePolitical),
								  new JProperty(autocollapsestupid, this.AutoCollapseStupid),
								  new JProperty(autopinonreply, this.AutoPinOnReply),
								  new JProperty(autoremoveonexpire, this.AutoRemoveOnExpire));
					}
					else
					{
						//If we do have settings, use them.
						this.cloudSettings.CreateOrSet("watched", new JArray(this.pinnedCommentIds));
						this.cloudSettings.CreateOrSet(showInlineImages, this.ShowInlineImages);
						this.cloudSettings.CreateOrSet(autocollapseinformative, this.AutoCollapseInformative);
						this.cloudSettings.CreateOrSet(autocollapseinteresting, this.AutoCollapseInteresting);
						this.cloudSettings.CreateOrSet(autocollapsenws, this.AutoCollapseNws);
						this.cloudSettings.CreateOrSet(autocollapseofftopic, this.AutoCollapseOffTopic);
						this.cloudSettings.CreateOrSet(autocollapsepolitical, this.AutoCollapsePolitical);
						this.cloudSettings.CreateOrSet(autocollapsestupid, this.AutoCollapseStupid);
						this.cloudSettings.CreateOrSet(autopinonreply, this.AutoPinOnReply);
						this.cloudSettings.CreateOrSet(autoremoveonexpire, this.AutoRemoveOnExpire);
					}
					var post = new POSTHandler(Locations.MyCloudSettings, this.cloudSettings.ToString(), null);
				}
			}
			catch
			{
				System.Diagnostics.Debug.Assert(false);
			}
		}

		private void GetPinnedComments()
		{
			if (this.pinnedCommentIds != null)
			{
				this.commentsLeftToLoad = this.pinnedCommentIds.Count;
				if (this.commentsLeftToLoad > 0)
				{
					foreach (var commentId in this.pinnedCommentIds)
					{
						this.QueueCommentDownload(commentId);
					}
				}
				else
				{
					this.OnRefreshCompleted();
				}
			}
		}

		private void QueueCommentDownload(int commentId)
		{
			var request = Locations.ServiceHost + "thread/" + commentId + ".xml";
			CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
		}

		private void OnRefreshCompleted()
		{
			if (this.commentsLeftToLoad == 0)
			{
				if (this.SettingsSynced != null)
				{
					this.SettingsSynced(this, EventArgs.Empty);
				}
			}
		}
	}

	internal static class JSONExtensions
	{
		internal static JObject CreateOrSet(this JObject j, string propertyName, JToken obj)
		{
			if (j[propertyName] != null)
			{
				j[propertyName] = obj;
			}
			else
			{
				j.Add(propertyName, obj);
			}
			return j;
		}

		internal static JObject CreateOrSet(this JObject j, string propertyName, object obj)
		{
			return j.CreateOrSet(propertyName, new JValue(obj));
		}
	}
}
