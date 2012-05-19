using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using LatestChatty.Classes;
using System.Xml.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;

namespace LatestChatty.ViewModels
{
	public class WatchList : NotifyPropertyChangedBase
	{
		public ObservableCollection<Comment> Comments { get; private set; }

		private List<Comment> refreshedComments = new List<Comment>();

		private List<int> subscribedComments = new List<int>();

		public event EventHandler RefreshCompleted;

		private int commentsLeftToLoad = 0;

		public WatchList()
		{
			this.Comments = new ObservableCollection<Comment>();
			LoadWatchList();
		}

		~WatchList()
		{
			SaveWatchList();
		}

		public void Add(Comment c)
		{
			if (!this.IsOnWatchList(c)) { this.subscribedComments.Add(c.id); this.DownloadComment(c.id); this.SaveWatchList(); }
		}

		public void Remove(Comment c)
		{
			if (this.IsOnWatchList(c))
			{
				this.subscribedComments.Remove(c.id);
				this.SaveWatchList();
				var commentToRemove = this.Comments.FirstOrDefault(c1 => c1.id == c.id);
				if (commentToRemove != null)
				{
					this.Comments.Remove(commentToRemove);
				}
			}
		}

		public bool IsOnWatchList(Comment c)
		{
			return this.subscribedComments.Any(i => i == c.id);
		}

		public bool AddOrRemove(Comment c)
		{
			if (this.IsOnWatchList(c))
			{
				this.Remove(c);
				return true;
			}
			else
			{
				this.Add(c);
				return false;
			}
		}

		public void SaveWatchList()
		{
			DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.Create, isf))
				{
					ser.WriteObject(stream, this.subscribedComments);
				}
			}
		}

		public void LoadWatchList()
		{
			try
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("watchlist.txt", FileMode.OpenOrCreate, isf))
					{
						this.subscribedComments = ser.ReadObject(stream) as List<int>;
					}
				}
			}

			catch { }/*(Exception e)
			{
				MessageBox.Show(string.Format("problem loading pinned threads. {0}", e.ToString()));
			}*/
		}

		//This may not be the optimal way to do this, but it works...
		public void Refresh()
		{
			this.refreshedComments.Clear();
			this.commentsLeftToLoad = this.subscribedComments.Count;
			if (this.commentsLeftToLoad > 0)
			{
				foreach (var commentId in this.subscribedComments)
				{
					this.DownloadComment(commentId);
				}
			}
			else
			{
				this.OnRefreshCompleted();
			}
		}
		
		private void DownloadComment(int commentId)
		{
			var request = CoreServices.Instance.ServiceHost + "thread/" + commentId + ".xml";
			CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
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
					for (insertAt = 0; insertAt < this.refreshedComments.Count; insertAt++)
					{
						//Keep looking
						if (comment.id > this.refreshedComments[insertAt].id)
						{
							continue;
						}
						//Already exists... don't add it twice.  (This could happen if they click refresh fast)
						if (comment.id == this.refreshedComments[insertAt].id)
						{
							return;
						}
						//We belong before this one.
						if (comment.id < this.refreshedComments[insertAt].id)
						{
							break;
						}
					}
					this.refreshedComments.Insert(insertAt, comment);
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

		private void OnRefreshCompleted()
		{
			if (this.commentsLeftToLoad == 0)
			{
				this.Comments.Clear();
				foreach (var c in this.refreshedComments)
				{
					this.Comments.Add(c);
				}
				this.refreshedComments.Clear();
				if (this.RefreshCompleted != null)
				{
					this.RefreshCompleted(this, EventArgs.Empty);
				}
			}
		}
	}
}
