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
		
		public readonly IsolatedStorageSettings isoStore;

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
	}
}
