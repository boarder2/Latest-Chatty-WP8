using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using LatestChatty.Classes;
using LatestChatty.Settings;

namespace LatestChatty.Pages
{
	public partial class SettingsPage : PhoneApplicationPage
	{
		private bool loaded = false;
		NotificationType loadedNotificationType;

		public SettingsPage()
		{
			InitializeComponent();

			this.sizePicker.SelectedItem = this.sizePicker.Items.First(
				item => ((ListPickerItem)item).Tag.ToString().Equals(
					Enum.GetName(typeof(CommentViewSize), LatestChattySettings.Instance.CommentSize), StringComparison.InvariantCultureIgnoreCase));

			this.showEmbeddedImagesPicker.SelectedItem = this.showEmbeddedImagesPicker.Items.First(
				item => ((ListPickerItem)item).Tag.ToString().Equals(
					Enum.GetName(typeof(ShowInlineImages), LatestChattySettings.Instance.ShowInlineImages), StringComparison.InvariantCultureIgnoreCase));

			this.notificationTypePicker.SelectedItem = this.notificationTypePicker.Items.First(
				item => ((ListPickerItem)item).Tag.ToString().Equals(
					Enum.GetName(typeof(NotificationType), LatestChattySettings.Instance.NotificationType), StringComparison.InvariantCultureIgnoreCase));
			this.loadedNotificationType = LatestChattySettings.Instance.NotificationType;

			this.autoCollapseNws.IsChecked = LatestChattySettings.Instance.AutoCollapseNws;
			this.autoCollapseStupid.IsChecked = LatestChattySettings.Instance.AutoCollapseStupid;
			this.autoCollapseOfftopic.IsChecked = LatestChattySettings.Instance.AutoCollapseOffTopic;
			this.autoCollapsePolitical.IsChecked = LatestChattySettings.Instance.AutoCollapsePolitical;
			this.autoCollapseInteresting.IsChecked = LatestChattySettings.Instance.AutoCollapseInteresting;
			this.autoCollapseInformative.IsChecked = LatestChattySettings.Instance.AutoCollapseInformative;
			this.navigationPicker.SelectedIndex = LatestChattySettings.Instance.ThreadNavigationByDate ? 0 : 1;
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			this.loaded = true;
		}

		private void AddChatty_Click(object sender, RoutedEventArgs e)
		{
			ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("ChattyPage"));

			if (tile == null)
			{
				StandardTileData data = new StandardTileData
				{
					BackgroundImage = new Uri("Background.png", UriKind.Relative),
					Title = "GoToChatty"
				};

				ShellTile.Create(new Uri("/Pages/ChattyPage.xaml", UriKind.Relative), data);
			}
		}

		private void NotificationTypePickerChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.loaded) return;


			var picker = sender as ListPicker;
			if (picker != null)
			{
				var notificationType = (NotificationType)Enum.Parse(typeof(NotificationType), ((ListPickerItem)picker.SelectedItem).Tag as string, true);
				if (this.loadedNotificationType != notificationType)
				{
					LatestChattySettings.Instance.NotificationType = notificationType;
					//Set it to the current type so we make sure we get back here.
					this.loadedNotificationType = notificationType;
					switch (notificationType)
					{
						case NotificationType.Tile:
						case NotificationType.TileAndToast:
							NotificationHelper.ReRegisterForNotifications();
							break;
						default:
							NotificationHelper.UnRegisterNotifications();
							break;
					}
				}
			}
		}
		private void CommentSizePickerChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.loaded) return;

			var picker = sender as ListPicker;
			if (picker != null)
			{
				var val = (CommentViewSize)Enum.Parse(typeof(CommentViewSize), ((ListPickerItem)picker.SelectedItem).Tag as string, true);
				LatestChattySettings.Instance.CommentSize = val;
			}
		}

		private void ShowEmbeddedImagesPickerChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.loaded) return;

			var picker = sender as ListPicker;
			if (picker != null)
			{
				var val = (ShowInlineImages)Enum.Parse(typeof(ShowInlineImages), ((ListPickerItem)picker.SelectedItem).Tag as string, true);
				LatestChattySettings.Instance.ShowInlineImages = val;
			}
		}

		private void NextBehaviorPickerChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.loaded) return;

			var picker = sender as ListPicker;
			if (picker != null)
			{
				LatestChattySettings.Instance.ThreadNavigationByDate = Boolean.Parse(((ListPickerItem)picker.SelectedItem).Tag as string);
			}
		}

		private void AutoCollapsePost_Checked(object sender, RoutedEventArgs e)
		{
			var toggle = sender as ToggleSwitch;
			if (toggle != null)
			{
				switch(toggle.Header.ToString())
				{
					case "nws":
						LatestChattySettings.Instance.AutoCollapseNws = toggle.IsChecked.HasValue ? toggle.IsChecked.Value : false;
						break;
					case "offtopic":
						LatestChattySettings.Instance.AutoCollapseOffTopic = toggle.IsChecked.HasValue ? toggle.IsChecked.Value : false;
						break;
					case "political":
						LatestChattySettings.Instance.AutoCollapsePolitical = toggle.IsChecked.HasValue ? toggle.IsChecked.Value : false;
						break;
					case "stupid":
						LatestChattySettings.Instance.AutoCollapseStupid = toggle.IsChecked.HasValue ? toggle.IsChecked.Value : false;
						break;
					case "informative":
						LatestChattySettings.Instance.AutoCollapseInteresting = toggle.IsChecked.HasValue ? toggle.IsChecked.Value : false;
						break;
					case "interesting":
						LatestChattySettings.Instance.AutoCollapseInformative = toggle.IsChecked.HasValue ? toggle.IsChecked.Value : false;
						break;
					
				}
			}
		}
	}
}