// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ListViewInteraction;
using ListViewInteraction.Common;
using Windows.Storage;

namespace ListViewInteraction
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            this.Suspending += new SuspendingEventHandler(OnSuspending);
        }

        async protected void OnSuspending(object sender, SuspendingEventArgs args)
        {
            SuspendingDeferral deferral = args.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        async protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //     Do an asynchronous restore
                await SuspensionManager.RestoreAsync();
            }
            StorageFolder localfolder = ApplicationData.Current.LocalFolder;
            var folders = await localfolder.GetFoldersAsync();
            foreach (StorageFolder folder in folders)
            {
                await folder.DeleteAsync();
            }
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage));
            Window.Current.Content = rootFrame;
            MainPage p = rootFrame.Content as MainPage;
            p.RootNamespace = this.GetType().Namespace;
            Window.Current.Activate();
        }
    }
}
