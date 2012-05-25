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
using LatestChatty.Classes;

namespace LatestChatty.Controls
{
	public partial class CommentListControl : UserControl
	{
		//ScrollViewer listScrollViewer;

		public CommentListControl()
		{
			InitializeComponent();
			//this.listScrollViewer = FindChildOfType<ScrollViewer>(commentListBox);
			this.commentListBox.Hold += new EventHandler<GestureEventArgs>(commentListBox_Hold);
		}

		void commentListBox_Hold(object sender, GestureEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Hold event. Sender: ", sender);
		}

		private void SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox lbSender = sender as ListBox;
			// If selected index is -1 (no selection) do nothing
			if (lbSender.SelectedIndex == -1)
				return;

			Comment c = lbSender.SelectedItem as Comment;

			CoreServices.Instance.Navigate(new Uri("/Pages/ThreadPage.xaml?Comment=" + c.id + "&Story=" + c.storyid, UriKind.Relative));
			lbSender.SelectedIndex = -1;
		}

		private void TogglePin_Click(object sender, RoutedEventArgs e)
		{
			var dc = (sender as FrameworkElement).DataContext;
			var comment = dc as Comment;
			if (comment.IsLoadMoreComment) return;
			comment.IsPinned = !comment.IsPinned;
		}

		private void ToggleCollapsed_Click(object sender, RoutedEventArgs e)
		{
			var dc = (sender as FrameworkElement).DataContext;
			var comment = dc as Comment;
			if (comment.IsLoadMoreComment) return;
			comment.IsCollapsed = !comment.IsCollapsed;
		}

		private void LoadMore_Click(object sender, RoutedEventArgs e)
		{
			var commentThread = this.commentListBox.DataContext as LatestChatty.ViewModels.CommentList;
			if (commentThread != null)
			{
				commentThread.LoadMore();
			}
		}
	}
}
