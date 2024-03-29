﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Linq;
using LatestChatty.Classes;
using LatestChatty.Settings;

namespace LatestChatty.ViewModels
{
	public class MyPostsList : INotifyPropertyChanged
	{
		public ObservableCollection<SearchResult> SearchResults { get; set; }

		public MyPostsList()
		{
			LoadMyPosts();
		}

		~MyPostsList()
		{
			SaveMyPosts();
		}

		void GetCommentsCallback(XDocument response)
		{
			try
			{
				var ObjChatty = from x in response.Descendants("result")
									 select new SearchResult(x);

				SearchResults.Clear();
				foreach (SearchResult singleResult in ObjChatty)
				{
					SearchResults.Add(singleResult);
				}

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Cannot load comments posted by author " + CoreServices.Instance.Credentials.UserName + ".");
			}
		}

		public void Refresh()
		{
			if (CoreServices.Instance.LoginVerified)
			{
				string request = Locations.ServiceHost + "Search/?Author=" + CoreServices.Instance.Credentials.UserName;
				CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
			}
		}

		public void SaveMyPosts()
		{
			DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<SearchResult>));

			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("myposts.txt", FileMode.Create, isf))
				{
					ser.WriteObject(stream, SearchResults);
				}
			}
		}

		public void LoadMyPosts()
		{
			SearchResults = new ObservableCollection<SearchResult>();
			try
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<SearchResult>));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("myposts.txt", FileMode.OpenOrCreate, isf))
					{
						SearchResults = ser.ReadObject(stream) as ObservableCollection<SearchResult>;
					}
				}

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
				}
			}
			catch
			{
			}
		}

		public void Logout()
		{
			this.SearchResults.Clear();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
