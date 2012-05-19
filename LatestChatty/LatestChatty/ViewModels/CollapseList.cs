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
	public class CollapseList : NotifyPropertyChangedBase
	{
		private List<int> collapsedComments = new List<int>();

		public CollapseList()
		{
			LoadCollapseList();
		}

		~CollapseList()
		{
			SaveCollapseList();
		}

		public void Add(Comment c)
		{
			if (!this.IsOnCollapseList(c)) 
			{
				this.collapsedComments.Add(c.id);
				this.SaveCollapseList();
			}
		}

		public void Remove(Comment c)
		{
			if (this.IsOnCollapseList(c))
			{
				this.collapsedComments.Remove(c.id);
				this.SaveCollapseList();
			}
		}

		public bool IsOnCollapseList(Comment c)
		{
			return this.collapsedComments.Any(i => i == c.id);
		}

		public bool AddOrRemove(Comment c)
		{
			if (this.IsOnCollapseList(c))
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

		public void SaveCollapseList()
		{
			DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("Collapselist.txt", FileMode.Create, isf))
				{
					ser.WriteObject(stream, this.collapsedComments);
				}
			}
		}

		public void LoadCollapseList()
		{
			try
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(List<int>));

				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("Collapselist.txt", FileMode.OpenOrCreate, isf))
					{
						this.collapsedComments = ser.ReadObject(stream) as List<int>;
					}
				}
			}
			catch {
				this.collapsedComments = new List<int>();
			}

			this.collapsedComments = new List<int>(this.collapsedComments.OrderByDescending(l => l).Take(700).ToList());
		}
	}
}
