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
using Microsoft.Phone.Shell;

namespace LatestChatty.ViewModels
{
	public class MyRepliesList : INotifyPropertyChanged
	{
		public ObservableCollection<SearchResult> SearchResults { get; set; }

		public MyRepliesList()
		{
			LoadMyReplies();
		}

		~MyRepliesList()
		{
			SaveMyReplies();
		}

		void GetCommentsCallback(XDocument response)
		{
			try
			{
				var results = from x in response.Descendants("result")
								  select new SearchResult(x);

				var totalReplies = int.Parse(response.Element("results").Attribute("total_results").Value);

				CoreServices.Instance.Settings[SettingsConstants.LastInAppReplyCount] = totalReplies;
				CoreServices.Instance.Settings.Save();

				var tileToUpdate =
					ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("ChattyPage"))
					?? ShellTile.ActiveTiles.FirstOrDefault();

				//Get rid of tile data that's now old.
				tileToUpdate.Update(new StandardTileData());

				SearchResults.Clear();
				foreach (SearchResult singleResult in results)
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
				string request = CoreServices.Instance.ServiceHost + "Search/?ParentAuthor=" + CoreServices.Instance.Credentials.UserName;
				CoreServices.Instance.QueueDownload(request, GetCommentsCallback);
			}
		}

		public void SaveMyReplies()
		{
			DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<SearchResult>));

			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("myreplies.txt", FileMode.Create, isf))
				{
					ser.WriteObject(stream, SearchResults);
				}
			}
		}

		public void LoadMyReplies()
		{
			try
			{
				SearchResults = new ObservableCollection<SearchResult>();
				DataContractSerializer ser = new DataContractSerializer(typeof(ObservableCollection<SearchResult>));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("myreplies.txt", FileMode.OpenOrCreate, isf))
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

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
