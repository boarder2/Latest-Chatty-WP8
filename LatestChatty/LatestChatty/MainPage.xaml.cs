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
using LatestChatty.Controls;
using Microsoft.Phone.Shell;
using LatestChatty.Classes;
using LatestChatty.Settings;
using System.Windows.Controls.Primitives;

namespace LatestChatty
{
	public partial class MainPage : PhoneApplicationPage
	{
		LoginControl _login;
		private int _refreshing = 0;

		// Constructor
		public MainPage()
		{
			InitializeComponent();

			var maintenanceWorker = new System.ComponentModel.BackgroundWorker();
			maintenanceWorker.DoWork += (sender, args) =>
			{
				NotificationHelper.RegisterForNotifications();

				//Sync settings.  When this is finished, we'll try to load replies and whatnot.
				IncrementRefresher();
				Deployment.Current.Dispatcher.BeginInvoke(() =>
					 {
						 LatestChattySettings.Instance.LoadLongRunningSettings();
					 });
			};

			//Need to implement this for the watch list.
			CoreServices.Instance.MyPosts.PropertyChanged += (s, a) => DecrementRefresher();
			CoreServices.Instance.MyReplies.PropertyChanged += (s, a) => DecrementRefresher();
			LatestChattySettings.Instance.SettingsSynced += (s, a) => SettingsSynced();

			Pinned.DataContext = LatestChattySettings.Instance.PinnedComments;
			MyPosts.DataContext = CoreServices.Instance.MyPosts;
			MyReplies.DataContext = CoreServices.Instance.MyReplies;

			maintenanceWorker.RunWorkerAsync();

			if (!CoreServices.Instance.LoginVerified)
			{
				LoginText.Text = "login";
			}
			else
			{
				this.messagesSection.Visibility = Visibility.Visible;
			}
		}

		private void SettingsSynced()
		{
			if (CoreServices.Instance.LoginVerified)
			{
				IncrementRefresher();
				CoreServices.Instance.MyPosts.Refresh();
				IncrementRefresher();
				CoreServices.Instance.MyReplies.Refresh();
			}
			DecrementRefresher();
		}

		private void Chatty_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/ChattyPage.xaml", UriKind.Relative));
		}

		private void Stories_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/HeadlinesPage.xaml", UriKind.Relative));
		}

		private void Messages_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/MessagesPage.xaml", UriKind.Relative));
		}

		private void Search_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/SearchPage.xaml", UriKind.Relative));
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.CancelDownloads();
			NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
		}

		async private void Login_Click(object sender, RoutedEventArgs e)
		{
			if (!CoreServices.Instance.LoginVerified)
			{
				var login = new Login();
				if (await login.ShowLogin())
				{
					LoginText.Text = "logout";
					this.messagesSection.Visibility = Visibility.Visible;
					//IncrementRefresher();
					//IncrementRefresher();
					//IncrementRefresher();
					//CoreServices.Instance.MyPosts.Refresh();
					//CoreServices.Instance.MyReplies.Refresh();
					//LatestChattySettings.Instance.LoadLongRunningSettings();
				}
			}
			else
			{
				CoreServices.Instance.Logout();
				LoginText.Text = "login";
			}
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			base.OnBackKeyPress(e);
		}

		protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			_refreshing = 0;
		}

		private void MyPosts_Click(object sender, RoutedEventArgs e)
		{
			IncrementRefresher();
			CoreServices.Instance.MyPosts.Refresh();
		}

		private void MyReplies_Click(object sender, RoutedEventArgs e)
		{
			IncrementRefresher();
			CoreServices.Instance.MyReplies.Refresh();
		}

		private void Pinned_Click(object sender, RoutedEventArgs e)
		{
			IncrementRefresher();
			LatestChattySettings.Instance.LoadLongRunningSettings();
		}

		private void IncrementRefresher()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
				 {
					 _refreshing++;
					 if (_refreshing > 0)
					 {
						 ProgressBar.Visibility = Visibility.Visible;
						 ProgressBar.IsIndeterminate = true;
					 }
				 });
		}

		private void DecrementRefresher()
		{
			//Since this will now get called when watchlist items are refreshed - even though we didn't request it.
			if (_refreshing > 0)
			{
				_refreshing--;
				System.Diagnostics.Debug.WriteLine("Decrementing refresher. {0}", _refreshing);
				if (_refreshing == 0)
				{
					ProgressBar.Visibility = Visibility.Collapsed;
					ProgressBar.IsIndeterminate = false;
				}
			}
		}
	}
}