﻿// Copyright (c) Microsoft. All rights reserved
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using UWPQuickStart.Models;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechRecognition;

namespace UWPQuickStart
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            NavigationHistory = new Stack<Type>();

            EventModel = new EventModel();
        }

        //Tracks user navigation to handle the case where the user presses the back button.
        internal static Stack<Type> NavigationHistory { get; set; }

        //Singleton object
        internal static EventModel EventModel { get; set; }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            await CreateFrameAndNavigate();
        }

        private async System.Threading.Tasks.Task<Frame> CreateFrameAndNavigate()
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///CortanaRules.xml"));
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(EventMainPage));
            }
            // Ensure the current window is active
            Window.Current.Activate();
            return rootFrame;
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind != ActivationKind.VoiceCommand)
            {
                return;
            }

            var EventArgs = args as VoiceCommandActivatedEventArgs;
            SpeechRecognitionResult ActivateResult = EventArgs.Result;
            string CommandName = ActivateResult.RulePath[0];
            string textSpoken = ActivateResult.Text;
            Type StartingPage = typeof(Views.EventHome);

            switch (CommandName)
            {
                case "GetStarted":
                    {
                        // We don't want to do anything special in this case
                        break;
                    }
                case "Photos":
                    {
                        StartingPage = typeof(Views.Photos);
                        break;
                    }
                default:
                    {
                        // We got a command we don't recognize, but for now we'll just ignore
                        break;
                    }
            }

            Frame rootFrame = await CreateFrameAndNavigate();
            (rootFrame.Content as EventMainPage).SwitchToWindow(StartingPage);
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}