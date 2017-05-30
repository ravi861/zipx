// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Documents;
using ListViewInteraction.DataSource;
using ListViewInteraction.Routines;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.IO.Compression;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using ListViewInteraction.Common;
using Windows.UI.ApplicationSettings;

namespace ListViewInteraction
{
	partial class MainPage : ListViewInteraction.Common.LayoutAwarePage
	{
		#region Properties


		private string _rootNamespace;

		public string RootNamespace
		{
			get { return _rootNamespace; }
			set { _rootNamespace = value; }
		}


		#endregion


        FolderData storeData = null;
        static StorageFolder storfolder;
		public MainPage()
		{
			InitializeComponent();

			//SetFeatureName(FEATURE_NAME);

			Loaded += new RoutedEventHandler(MainPage_Loaded);
			Window.Current.SizeChanged += new WindowSizeChangedEventHandler(MainPage_SizeChanged);
			DisplayProperties.LogicalDpiChanged += new DisplayPropertiesEventHandler(DisplayProperties_LogicalDpiChanged);

            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
		}

		void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			// Figure out what resolution and orientation we are in and respond appropriately
			//CheckResolutionAndViewState();

            storeData = new FolderData();
            //ItemGridView.ItemsSource = storeData.Collection;
		}

		#region Resolution and orientation code

		void DisplayProperties_LogicalDpiChanged(object sender)
		{
			//CheckResolutionAndViewState();
		}

		void CheckResolutionAndViewState()
		{
			VisualStateManager.GoToState(this, ApplicationView.Value.ToString() + DisplayProperties.ResolutionScale.ToString(), false);
		}

		void MainPage_SizeChanged(Object sender, Windows.UI.Core.WindowSizeChangedEventArgs args)
                {
                         //CheckResolutionAndViewState();
		}

		#endregion

		private void SetFeatureName(string str)
		{
			//FeatureName.Text = str;
		}

		async void Footer_Click(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
		}

		public void NotifyUser(string strMessage, NotifyType type)
		{
		}

        public void DoNavigation(Type inPageType, Frame inFrame, Type outPageType, Frame outFrame)
		{

		}

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            SettingsCommand defaultsCommand = new SettingsCommand("privacy", "Privacy",
                (handler) =>
                {
                    Privacy sf = new Privacy();
                    sf.Show();
                });
            e.Request.ApplicationCommands.Add(defaultsCommand);
        }

        private async Task<StorageFolder> extractFiles(StorageFile zipfilename)
        {
            StorageFolder storfolder = null;
            Uri _baseUri = new Uri("ms-appx:///");
            try
            {
                // Create stream for compressed files in memory
                using (MemoryStream zipMemoryStream = new MemoryStream())
                {
                    using (Windows.Storage.Streams.IRandomAccessStream zipStream = await zipfilename.OpenAsync(FileAccessMode.Read))
                    {
                        // Read compressed data from file to memory stream
                        using (Stream instream = zipStream.AsStreamForRead())
                        {
                            byte[] buffer = new byte[1024];
                            while (instream.Read(buffer, 0, buffer.Length) > 0)
                            {
                                zipMemoryStream.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }
                    storfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(zipfilename.DisplayName, CreationCollisionOption.GenerateUniqueName);

                    // Create zip archive to access compressed files in memory stream
                    using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Read))
                    {
                        int count = zipArchive.Entries.Count;
                        int i = 0;
                        Item item;
                        // For each compressed file...
                        foreach (ZipArchiveEntry entry in zipArchive.Entries)
                        {
                            i++;
                            // ... read its uncompressed contents
                            using (Stream entryStream = entry.Open())
                            {
                                if (entry.Name != "")
                                {
                                    item = new Item();
                                    string fileName = entry.FullName.Replace("/", @"\");
                                    byte[] buffer = new byte[entry.Length];
                                    entryStream.Read(buffer, 0, buffer.Length);

                                    // Create a file to store the contents
                                    StorageFile uncompressedFile = await storfolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                                    // Store the contents
                                    using (IRandomAccessStream uncompressedFileStream = await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                                    {
                                        using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                                        {
                                            outstream.Write(buffer, 0, buffer.Length);
                                            outstream.Flush();
                                        }
                                    }
                                    item.Link = zipfilename.DisplayName + "/" + entry.FullName;
                                    item.Title = entry.Name;
                                    String[] ext = entry.Name.Split('.');
                                    if (ext.Length > 1)
                                    {
                                        item.SetImage(_baseUri, Routines.Routines.getImageType("." + ext[1]));
                                        item.FileType = ext[1] + " File";
                                    }
                                    else
                                    {
                                        item.SetImage(_baseUri, Routines.Routines.getImageType("."));
                                        item.FileType = "Config File";
                                    }
                                    storeData.Collection.Add(item);
                                    int x = ((i * 100) / count);
                                    copy1.Text = "Extracting files " + x + "%";
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex1)
            {
                ProgressRing1.IsActive = false;
                DisplayMsg("Error extracting files!!!");
                if (storfolder != null) storfolder.DeleteAsync();
                return null;
            }
            finally
            {
                ProgressRing1.IsActive = false;
                ItemGridView.ItemsSource = storeData.Collection;
                chzip.IsEnabled = false;
                exall.IsEnabled = true;
                exsel.IsEnabled = false;
                clsel.IsEnabled = false;
                clcon.IsEnabled = true;

                zipfilename.DeleteAsync();
            }
            return storfolder;
        }


        public async Task extracter(StorageFile filename)
        {
            ProgressRing1.IsActive = true;

            storfolder = await extractFiles(filename);
            if (storfolder == null) return;
        }

        private async void chooseFile(object sender, RoutedEventArgs e)
        {
            copy1.Text = "Extracting files";
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".zip");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (null != file)
            {
                chzip.IsEnabled = false;

                try
                {
                    StorageFile savedFile = await file.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    await extracter(savedFile);
                    Windows.Storage.FileProperties.BasicProperties fileprop = await file.GetBasicPropertiesAsync();
                    this.copy1.Text = file.DisplayName + ".zip | " + ((fileprop.Size) / 1024).ToString() + " KB";
                }
                catch
                {
                    DisplayMsg("File extraction failed!!!");
                }
            }
        }

        public async void ExtractAll_Click_1(object sender, RoutedEventArgs e)
        {
            StorageFolder newFolder = null;
            copy1.Text = "Extracting all files";
            try
            {
                Windows.Storage.Search.CommonFileQuery query = Windows.Storage.Search.CommonFileQuery.OrderByName;
                IReadOnlyList<StorageFile> files = await storfolder.GetFilesAsync(query);
                newFolder = await Windows.Storage.DownloadsFolder.CreateFolderAsync(storfolder.Name, CreationCollisionOption.GenerateUniqueName);
                foreach (StorageFile file in files)
                {
                    await file.CopyAsync(newFolder, file.Name, NameCollisionOption.GenerateUniqueName);
                }
            }
            catch
            {
                DisplayMsg("Error copying files to Downloads!!!");
            }
            copy1.Text = "Copied to Downloads/ZipX/" + newFolder.DisplayName;
        }

        public static String[] parseString(String path)
        {
            String[] finalPath = new String[2];
            String[] tempPath = null;
            int i;
            if (path.ToString().Contains("/"))
            {
                tempPath = path.Split('/');
                for (i = 0; i < tempPath.Length - 1; i++)
                {
                    finalPath[0] = finalPath[0] + tempPath[i] + "/";
                }
                finalPath[1] = tempPath[i];
            }
            else
            {
                finalPath[0] = "";
                finalPath[1] = path;
            }
            return finalPath;
        }

        private async void Selection_Click_1(object sender, RoutedEventArgs e)
        {
            copy1.Text = "Copying....";
            StorageFolder storeFolder = null;
            try 
            {
                storeFolder = await DownloadsFolder.CreateFolderAsync(storfolder.Name.ToString(), CreationCollisionOption.GenerateUniqueName);
                if (ItemGridView.SelectedItems.Count != 0)
                {
                    foreach (Item item in ItemGridView.SelectedItems)
                    {
                        //Uri uri = new Uri("ms-appdata:///local/" + item.Link);
                        String[] filepath = parseString(item.Link);
                        StorageFolder imageFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(filepath[0].Replace("/", "\\"));
                        StorageFile storageFile = await imageFolder.GetFileAsync(filepath[1]);
                        //StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                        await storageFile.CopyAsync(storeFolder, item.Title, NameCollisionOption.GenerateUniqueName);
                    }
                }
            }
            catch
            {
                DisplayMsg("Error copying selected files to Downloads!!!");
            }
            ItemGridView.SelectedItems.Clear();
            copy1.Text = "Copied to Downloads/ZipX/" + storeFolder.DisplayName;
        }

        private void ClearSel_Click_1(object sender, RoutedEventArgs e)
        {
            ItemGridView.SelectedItems.Clear();
            exsel.IsEnabled = false;

        }

        private async void Clear_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                storeData.Collection.Clear();
                ItemGridView.ItemsSource = null;
                StorageFolder localfolder = ApplicationData.Current.LocalFolder;
                var folders = await localfolder.GetFoldersAsync();
                foreach (StorageFolder folder in folders)
                {
                    await folder.DeleteAsync();
                }
            }
            catch
            {
                DisplayMsg("Error deleting temporary files!!!");
            }
            chzip.IsEnabled = true;
            exall.IsEnabled = false;
            exsel.IsEnabled = false;
            clsel.IsEnabled = false;
            clcon.IsEnabled = false;
            copy1.Text = "Select File";
        }

        private void ItemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            copy1.Text = ItemGridView.SelectedItems.Count.ToString() + " files selected";
            if (ItemGridView.SelectedItems.Count == 0)
            {
                exsel.IsEnabled = false;
                clsel.IsEnabled = false;
            }
            else
            {
                exsel.IsEnabled = true;
                clsel.IsEnabled = true;
            }
        }

        public static async void DisplayMsg(String errormsg)
        {
            // Create the message dialog and set its content; it will get a default "Close" button since there aren't any other buttons being added
            MessageDialog msg = new MessageDialog("Oops!!!!");
            msg.Title = errormsg;
            try
            {
                IUICommand result = await msg.ShowAsync();
            }
            catch
            { }
        }

	}

	public enum NotifyType
	{
		StatusMessage,
		ErrorMessage
	};
}
