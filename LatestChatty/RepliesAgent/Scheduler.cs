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
using Microsoft.Phone.Scheduler;

namespace RepliesAgent
{
	public static class Scheduler
	{
		/// <summary>
		/// Removes the agent from the services.
		/// </summary>
		public static void RemoveTask()
		{
			if (ScheduledActionService.Find(RepliesTask.TaskName) != null)
				ScheduledActionService.Remove(RepliesTask.TaskName);
		}

		/// <summary>
		/// Refreshes timeout on the task, only if it's already in the scheduled services.
		/// </summary>
		public static void RefreshTask()
		{
			//Only refresh if it's already added.
			if (ScheduledActionService.Find(RepliesTask.TaskName) != null)
			{
				Scheduler.RemoveTask();
				Scheduler.AddTask();
			}
		}

		/// <summary>
		/// Adds the agent to the services.
		/// </summary>
		public static void AddTask()
		{
			//TODO: Detect 256MB devices.
			if (ScheduledActionService.Find(RepliesTask.TaskName) == null)
			{
				PeriodicTask task = new PeriodicTask(RepliesTask.TaskName);
				task.Description = "Periodically checks for replies to threads you posted in LatestChatty.  Updates LiveTile with results.";

				task.ExpirationTime = DateTime.Now.AddDays(14);
				try
				{
					ScheduledActionService.Add(task);
#if DEBUG
					ScheduledActionService.LaunchForTest("LatestChatty", TimeSpan.FromSeconds(10));
#endif

				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Can't schedule agent; either there are too many other agents scheduled or you have disabled this agent in Settings.");
					return;
				}
			}
		}
	}
}
