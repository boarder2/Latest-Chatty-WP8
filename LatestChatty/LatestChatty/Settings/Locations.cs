using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatestChatty.Settings
{
	public static class Locations
	{
		public const string ServiceHost = "http://shackapi.stonedonkey.com/";
		public const string PostUrl = ServiceHost + "post/";
		public const string CloudHost = "http://shacknotify.bit-shift.com:12244/";
		public static string MyCloudSettings
		{
			get { return CloudHost + "users/" + CoreServices.Instance.Credentials.UserName + "/settings"; }
		}

	}
}
