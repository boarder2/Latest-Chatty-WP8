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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using BugSense;
using BugSense.Core.Model;

namespace LatestChatty
{
	public partial class App : Application
	{
		#region Fast App Resume Stuff
		//Using http://code.msdn.microsoft.com/wpapps/Fast-app-resume-backstack-f16baaa6 as an example.

		enum SessionType
		{
			None,
			Home,
			DeepLink
		}

		// Set to Home when the app is launched from Primary tile. 
		// Set to DeepLink when the app is launched from Deep Link. 
		private SessionType sessionType = SessionType.None;

		// Set to true when the page navigation is being reset  
		bool relaunched = false;

		// set to true when 5 min passed since the app was relaunched 
		bool clearPageStack = false;

		IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings; 
 
		#endregion

		/// <summary>
		/// Provides easy access to the root frame of the Phone Application.
		/// </summary>
		/// <returns>The root frame of the Phone Application.</returns>
		public PhoneApplicationFrame RootFrame { get; private set; }

		/// <summary>
		/// Constructor for the Application object.
		/// </summary>
		public App()
		{
			BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(Current), RootFrame, "4b208f0f");
			// Global handler for uncaught exceptions. 
			//UnhandledException += Application_UnhandledException;

			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// Display the current frame rate counters.
				Application.Current.Host.Settings.EnableFrameRateCounter = true;

				// Show the areas of the app that are being redrawn in each frame.
				//Application.Current.Host.Settings.EnableRedrawRegions = true;

				// Enable non-production analysis visualization mode, 
				// which shows areas of a page that are being GPU accelerated with a colored overlay.
				//Application.Current.Host.Settings.EnableCacheVisualization = true;
			}

			// Standard Silverlight initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();
		}

		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		private void Application_Launching(object sender, LaunchingEventArgs e)
		{
			this.RemoveCurrentTimeSetting();
			CoreServices.Instance.Initialize();
		}

		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void Application_Activated(object sender, ActivatedEventArgs e)
		{
			CoreServices.Instance.Activated();
			//When the application is launched we need to check if it's been too long since the last time it was opened.
			this.clearPageStack = IsResumeExpired();
		}

		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void Application_Deactivated(object sender, DeactivatedEventArgs e)
		{
			CoreServices.Instance.Deactivated();
			this.SaveCurrentTime();
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void Application_Closing(object sender, ClosingEventArgs e)
		{
			this.RemoveCurrentTimeSetting();
		}

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// A navigation has failed; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		#region Phone application initialization

		// Avoid double-initialization
		private bool phoneApplicationInitialized = false;

		// Do not add any additional code to this method
		private void InitializePhoneApplication()
		{
			if (phoneApplicationInitialized)
				return;

			// Create the frame but don't set it as RootVisual yet; this allows the splash
			// screen to remain active until the application is ready to render.
			//RootFrame = new PhoneApplicationFrame();
			RootFrame = new TransitionFrame();
			RootFrame.Navigating += FrameNavigating;
			RootFrame.Navigated += CompleteInitializePhoneApplication;

			// Handle navigation failures
			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			// Ensure we don't initialize again
			phoneApplicationInitialized = true;
		}

		#region Fast App Resume Methods
		void FrameNavigating(object sender, NavigatingCancelEventArgs e)
		{
			//Handle application resuming here.
			if (sessionType == SessionType.None && e.NavigationMode == NavigationMode.New)
			{
			    // This block will run if the current navigation is part of the app's intial launch 

			    // Keep track of Session Type  
			    if (e.Uri.ToString().Contains("DeepLink=true"))
			    {
			        sessionType = SessionType.DeepLink;
			    }
			    else if (e.Uri.ToString().Contains("/MainPage.xaml"))
			    {
			        sessionType = SessionType.Home;
			    }
			}


			if (e.NavigationMode == NavigationMode.Reset)
			{
			    // This block will execute if the current navigation is a relaunch. 
			    // If so, another navigation will be coming, so this records that a relaunch just happened 
			    // so that the next navigation can use this info. 
			    relaunched = true;
			}
			else if (e.NavigationMode == NavigationMode.New && relaunched)
			{
			    // This block will run if the previous navigation was a relaunch 
			    relaunched = false;

			    if (e.Uri.ToString().Contains("DeepLink=true"))
			    {
			        // This block will run if the launch Uri contains "DeepLink=true" which 
			        // was specified when the secondary tile was created in MainPage.xaml.cs 

			        sessionType = SessionType.DeepLink;
			        // The app was relaunched via a Deep Link. 
			        // The page stack will be cleared. 
			    }
			    else if (e.Uri.ToString().Contains("/MainPage.xaml"))
			    {
			        // This block will run if the navigation Uri is the main page 
			        if (sessionType == SessionType.DeepLink)
			        {
			            // When the app was previously launched via Deep Link and relaunched via Main Tile, we need to clear the page stack.  
			            sessionType = SessionType.Home;
			        }
			        else
			        {
			            if (!clearPageStack)
			            {
			                //The app was previously launched via Main Tile and relaunched via Main Tile. Cancel the navigation to resume. 
			                e.Cancel = true;
			                RootFrame.Navigated -= ClearBackStackAfterReset;
			            }
			        }
			    }

			    clearPageStack = false;
			}             
		}

		private void CheckForResetNavigation(object sender, NavigationEventArgs e)
		{
			// If the app has received a 'reset' navigation, then we need to check 
			// on the next navigation to see if the page stack should be reset 
			if (e.NavigationMode == NavigationMode.Reset)
				RootFrame.Navigated += ClearBackStackAfterReset;
		}

		private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
		{
			// Unregister the event so it doesn't get called again 
			RootFrame.Navigated -= ClearBackStackAfterReset;

			// Only clear the stack for 'new' (forward) and 'refresh' navigations 
			if (e.NavigationMode != NavigationMode.New)
				return;

			// For UI consistency, clear the entire page stack 
			while (RootFrame.RemoveBackEntry() != null)
			{
				; // do nothing 
			}
		}

		public bool AddOrUpdateValue(string Key, Object value)
		{
			bool valueChanged = false;

			// If the key exists 
			if (settings.Contains(Key))
			{
				// If the value has changed 
				if (settings[Key] != value)
				{
					// Store the new value 
					settings[Key] = value;
					valueChanged = true;
				}
			}
			// Otherwise create the key. 
			else
			{
				settings.Add(Key, value);
				valueChanged = true;
			}
			return valueChanged;
		}


		public void RemoveValue(string Key)
		{
			// If the key exists 
			if (settings.Contains(Key))
			{
				settings.Remove(Key);
			}
		}
		public void SaveCurrentTime()
		{
			if (AddOrUpdateValue("DeactivateTime", DateTimeOffset.Now))
			{
				settings.Save();
			}
		}

		public void RemoveCurrentTimeSetting()
		{
			RemoveValue("DeactivateTime");
			settings.Save();
		}

		bool IsResumeExpired()
		{
			DateTimeOffset lastDeactivated;

			if (settings.Contains("DeactivateTime"))
			{
				lastDeactivated = (DateTimeOffset)settings["DeactivateTime"];
			}

			var currentDuration = DateTimeOffset.Now.Subtract(lastDeactivated);

			TimeSpan expired;

#if DEBUG
			expired = TimeSpan.FromSeconds(30);
#else
			expired = TimeSpan.FromMinutes(30);
#endif

			return TimeSpan.FromSeconds(currentDuration.TotalSeconds) > expired;
		} 
		#endregion

		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
		{
			// Set the root visual to allow the application to render
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			// Remove this handler since it is no longer needed
			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		#endregion
	}
}