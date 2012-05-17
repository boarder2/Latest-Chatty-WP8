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
