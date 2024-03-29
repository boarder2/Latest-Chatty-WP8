﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LatestChatty.Classes;
using LatestChatty.Controls;
using LatestChatty.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using LatestChatty.Settings;

namespace LatestChatty.Pages
{
	public partial class ThreadPage : PhoneApplicationPage
	{
		public Rectangle SelectedFill = null;
		private CommentThread thread;
		private ApplicationBarMenuItem pinMenuItem;
		private ApplicationBarIconButton navigationModeButton;
		private bool navigateByDate;

		public ThreadPage()
		{
			System.Diagnostics.Debug.WriteLine("Thread - ctor");
			InitializeComponent();
            this.pinMenuItem = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
			this.navigationModeButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
			this.navigateByDate = LatestChattySettings.Instance.ThreadNavigationByDate;
			this.SetNavigationModeIcon();

			try
			{
				this.commentBrowser.NavigateToString(CoreServices.Instance.CommentBrowserString);
			}
			catch
			{
				//If there was a problem, navigate back in the stack if we can.
				if(NavigationService.CanGoBack)
				{
					NavigationService.GoBack();
				}
			}
		}

		private void SelectComment(Comment c)
		{
			this.thread.SelectComment(c);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Thread - OnNavigatedTo - URI: {0}", e.Uri);
			string sStory, sComment;
			var storyId = NavigationContext.QueryString.TryGetValue("Story", out sStory) ? int.Parse(sStory) : 10;
			this.SizeBrowser();

			if (NavigationContext.QueryString.TryGetValue("Comment", out sComment))
			{
				var commentId = int.Parse(sComment);
				if (this.thread != null)
				{
					this.thread.PropertyChanged -= ThreadPropertyChanged;
				}

				//Attempt to get the thread from cache.  This means it was the last thread we viewed.
				this.thread = CoreServices.Instance.GetCommentThread(commentId);
				if (this.thread == null)
				{
					//If it wasn't the last thread viewed, we'll load the browser with the javascript needed to make changing text in the browser not flicker.
					this.commentBrowser.NavigateToString(CoreServices.Instance.CommentBrowserString);
					this.thread = new CommentThread(commentId, storyId);
				}

				this.DataContext = this.thread;
				var selectedCommentId = CoreServices.Instance.GetSelectedComment();
				if (selectedCommentId > 0) this.thread.SelectComment(selectedCommentId);

				//TODO: This is so dirty.
				//When trying to data bind directly to the Text property if the DataContext isn't available right away 
				// (and in this case it never will be), an exception is thrown because an ApplicationBarMenuItem cannot have an empty Text property.
				// So... I guess I'll do it this way for now.  Ugh.
				this.thread.PropertyChanged += ThreadPropertyChanged;

			}
		}

		private void SizeBrowser()
		{
			switch (LatestChattySettings.Instance.CommentSize)
			{
				case CommentViewSize.Half:
					this.commentBrowser.Height = 350;
					break;
				case CommentViewSize.Huge:
					this.commentBrowser.Height = 500;
					break;
				default:
					this.commentBrowser.Height = 250;
					break;
			}
		}

		void ThreadPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("IsWatched"))
			{
				this.SetPinnedMenuText();
			}
		}

		void SetPinnedMenuText()
		{
			this.pinMenuItem.Text = this.thread.IsWatched ? "unpin thread" : "pin thread";
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Thread - OnNavigatedFrom");
			CoreServices.Instance.CancelDownloads();
			if (this.thread.FlatComments.Count > 0)
			{
				CoreServices.Instance.SetCurrentCommentThread(thread);
			}
			base.OnNavigatedFrom(e);
		}

		void BrowserNavigating(object sender, NavigatingEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Thread - Navigating");

			string s = e.Uri.ToString();

			if (s.Contains("shacknews.com/chatty?id="))
			{
				int c = int.Parse(s.Split('=')[1].Split('#')[0]);
				CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + c, UriKind.Relative));
				e.Cancel = true;
				return;
			}

			WebBrowserTask task = new WebBrowserTask();
			task.Uri = new Uri(s);
			task.Show();
			e.Cancel = true;
		}

		private void RefreshClick(object sender, EventArgs e)
		{
			this.thread.Refresh();
		}

		private void ReplyClick(object sender, EventArgs e)
		{
            if (!CoreServices.Instance.LoginVerified)
            {
                MessageBox.Show("You must be logged in to reply to comments");
            }
			var c = this.thread.SelectedComment;
			if (c != null)
			{
                CoreServices.Instance.ReplyContext = new LatestChatty.CoreServices.ReplyToContext(this.thread.FlatComments.First(), c);
				CoreServices.Instance.Navigate(new Uri("/Pages/CommentPost.xaml?Story=" + c.storyid, UriKind.Relative));
			}
		}

		private void ToggleThreadMode(object sender, EventArgs e)
		{
			this.navigateByDate = !this.navigateByDate;
			this.SetNavigationModeIcon();
		}

		private void NextClick(object sender, EventArgs e)
		{
			if (this.thread.SelectedComment != null)
			{
				Comment displayComment = null;
				if (this.navigateByDate)
				{
					displayComment = this.thread.FlatComments.OrderBy(c => c.id).FirstOrDefault(c => c.id > this.thread.SelectedComment.id);
				}
				else
				{
					var currentIndex = this.thread.FlatComments.IndexOf(this.thread.SelectedComment);
					if (currentIndex < this.thread.FlatComments.Count - 1)
					{
						displayComment = this.thread.FlatComments[currentIndex + 1];
					}
					if (displayComment != null) this.thread.SelectComment(displayComment);
				}

				//Didn't find one, wrap to beginning.
				if (displayComment == null) displayComment = this.thread.FlatComments.First();
				if (displayComment != null) this.thread.SelectComment(displayComment);
			}
		}

		private void PreviousClick(object sender, EventArgs e)
		{
			if (this.thread.SelectedComment != null)
			{
				var currentIndex = this.thread.FlatComments.IndexOf(this.thread.SelectedComment);
				Comment displayComment = null;
				if (this.navigateByDate)
				{
					if (currentIndex > 0)
					{
						displayComment = this.thread.FlatComments.OrderBy(c => c.id).LastOrDefault(c => c.id < this.thread.SelectedComment.id);
					}
					else
					{
						//At the beginning, so in order to wrap back, we need to take the oldest comment sorted by ID.
						displayComment = this.thread.FlatComments.OrderBy(c => c.id).Last();
					}
				}
				else
				{
					if (currentIndex > 0)
					{
						displayComment = this.thread.FlatComments[currentIndex - 1];
					}
				}

				//Didn't find one, wrap to end.
				if (displayComment == null) displayComment = this.thread.FlatComments.Last();
				if (displayComment != null) this.thread.SelectComment(displayComment);
			}
		}

		private void PinClick(object sender, EventArgs e)
		{
			thread.TogglePinned();
		}

		private void ShareThreadClick(object sender, EventArgs e)
		{
			var shareTask = new ShareLinkTask
			{
				LinkUri = new Uri("http://www.shacknews.com/chatty/?id=" + thread.SeedCommentId.ToString(), UriKind.Absolute),
				Message = "Check out this thread on shacknews.com",
				Title = "Shack Chatty Thread"
			};
			shareTask.Show();
		}

		private void OpenInBrowserClick(object sender, EventArgs e)
		{
			var browserTask = new WebBrowserTask
			{
				Uri = new Uri("http://www.shacknews.com/chatty/?id=" + thread.SeedCommentId.ToString(), UriKind.Absolute)
			};
			browserTask.Show();
		}

		private void SetNavigationModeIcon()
		{
			this.navigationModeButton.Text = this.navigateByDate ? "top down" : "by date";
			this.navigationModeButton.IconUri = new Uri(this.navigateByDate ? @"Images/TimeIcon.png" : @"Images/TopDownIcon.png", UriKind.Relative);
		}
	}
}