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
using Microsoft.Phone.Tasks;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace LatestChatty.Controls
{
    public class Login
    {
        async public Task<bool> ShowLogin()
        {
            var tcs = new TaskCompletionSource<bool>();
            var p = new Popup();
            p.Child = new LoginControl(tcs);
            var task = tcs.Task;
            p.IsOpen = true;
            await task.AsAsyncAction();
            p.IsOpen = false;
            return task.Result;
        }
    }

	public partial class LoginControl : UserControl
	{
        TaskCompletionSource<bool> taskCompletion;

		public LoginControl(TaskCompletionSource<bool> tcs)
		{
			InitializeComponent();
            this.taskCompletion = tcs;
		}

        public void LoginVerification(bool verified)
		{
			if (verified)
			{
                this.taskCompletion.SetResult(true);
			}
			else
			{
				VerificationFailed.Visibility = Visibility.Visible;
			}
			ProgressBar.Visibility = Visibility.Collapsed;
			ProgressBar.IsIndeterminate = false;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			VerificationFailed.Visibility = Visibility.Collapsed;
			ProgressBar.Visibility = Visibility.Visible;
			ProgressBar.IsIndeterminate = false;

			CoreServices.Instance.TryLogin(usernameTB.Text, passwordTB.Password, LoginVerification);
		}

		private void Register_Click(object sender, RoutedEventArgs e)
		{
			WebBrowserTask task = new WebBrowserTask();

			task.Uri = new Uri("http://www.shacknews.com/create_account.x");
			task.Show();
		}

	}
}
