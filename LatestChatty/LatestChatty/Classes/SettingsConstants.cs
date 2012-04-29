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

namespace LatestChatty.Classes
{
	public static class SettingsConstants
	{
		public static readonly string CommentSize = "CommentSize";
		public static readonly string ThreadNavigationByDate = "ThreadNavigationByDate";
		public static readonly string ShowInlineImages = "ShowInline";
	}

	public enum CommentViewSize
	{
		Small,
		Half,
		Huge
	}

	public enum ShowInlineImages
	{
		Always,
		OnWiFi,
		Never
	}
}
