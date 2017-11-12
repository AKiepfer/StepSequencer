using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Caliburn.Micro;
using FileExplorerUniversal.Control.Interop;
using ManagedAudioEngineUniversal.Core;
using ManagedAudioEngineUniversal.Model;
using SelectionMode = FileExplorerUniversal.Control.Interop.SelectionMode;

namespace FileExplorerUniversal
{
    public partial class FileExplorer : UserControl, INotifyPropertyChanged
    {
        public delegate void OnDismissEventHandler(StorageTarget target, object file);

        private Frame _currentFrame;
        private Page _currentPage;

        private SamplePlayer _player;

        #region Control Properties

        private ObservableCollection<FileExplorerItem> _currentItems;
        private string _currentPath;
        private ExtensionRestrictions _extensionRestrictions;
        private List<string> _extensions;
        private SelectionMode _selectionMode;
        private StorageTarget _storageTarget;

        /// <summary>
        ///     Used to specify the extensions that are shown in the explorer view. Only used if ExtensionRestrictions is set to
        ///     Custom.
        /// </summary>
        public List<string> Extensions
        {
            get { return _extensions; }
            set
            {
                if (_extensions != value)
                {
                    _extensions = value;
                    NotifyPropertyChanged("Extensions");
                }
            }
        }

        /// <summary>
        ///     Determines which files are shown in the explorer view. If StorageTarget is set to ExternalStorage, InheritManifest
        ///     boundaries automatically applied regardless of any custom settings.
        /// </summary>
        public ExtensionRestrictions ExtensionRestrictions
        {
            get { return _extensionRestrictions; }
            set
            {
                if (_extensionRestrictions != value)
                {
                    _extensionRestrictions = value;
                    NotifyPropertyChanged("ExtensionRestrictions");
                }
            }
        }

        /// <summary>
        ///     The container that carries the items currently visible to the user.
        /// </summary>
        public ObservableCollection<FileExplorerItem> CurrentItems
        {
            get { return _currentItems; }
            private set
            {
                if (_currentItems != value)
                {
                    _currentItems = value;
                    NotifyPropertyChanged("CurrentItems");
                }
            }
        }

        /// <summary>
        ///     The currently active path.
        /// </summary>
        public string CurrentPath
        {
            get { return _currentPath; }
            private set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    NotifyPropertyChanged("CurrentPath");
                }
            }
        }

        /// <summary>
        ///     Determines whether the control will return a folder, file, or multiple files.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set
            {
                if (_selectionMode != value)
                {
                    _selectionMode = value;
                    NotifyPropertyChanged("SelectionMode");
                }
            }
        }

        public StorageTarget StorageTarget
        {
            get { return _storageTarget; }
            set
            {
                if (_storageTarget != value)
                {
                    _storageTarget = value;
                    NotifyPropertyChanged("StorageTarget");
                }
            }
        }

        #endregion

        public FileExplorer()
        {
            InitializeComponent();

            Extensions = new List<string>();
            SelectedItems = new List<FileExplorerItem>();
            DataContext = this;
        }

        //private Stack<ExternalStorageFolder> _externalFolderTree { get; set; }
        private Stack<IStorageFolderEx> FolderTree { get; set; }

        private List<FileExplorerItem> SelectedItems { get; set; }

        /// <summary>
        ///     Handles property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public event OnDismissEventHandler OnDismiss;

        private void Initialize()
        {
            //this.DataContext = this;
            CurrentItems = new ObservableCollection<FileExplorerItem>();

            var bounds = Window.Current.Bounds;
            LayoutRoot.Width = bounds.Width;
            LayoutRoot.Height = bounds.Height - 120;

            LayoutRoot.HorizontalAlignment = HorizontalAlignment.Center;

            _currentFrame = (Frame)Window.Current.Content;
            _currentPage = (Page)_currentFrame.Content;
            
            RootPopup.IsOpen = true;
            
            InitializeStorageContainers();
        }

        private async void InitializeStorageContainers()
        {
            FolderTree = new Stack<IStorageFolderEx>();

            if (OnlyLocalStorage)
            {
                GetTreeForFolder(new StorageFolderEx(ApplicationData.Current.LocalFolder));
                return;
            }

            var virtualFolder = new VirtualStorageFolder();

            try
            {
                virtualFolder.AddFolder(new StorageFolderEx(ApplicationData.Current.LocalFolder));
            }
            catch
            {
                Debug.WriteLine("INT_STORAGE_ERROR: There was a problem accessing internal storage.");
            }
            
            try
            {
                // Get the logical root folder for all external storage devices.
                StorageFolder externalDevices = KnownFolders.RemovableDevices;

                // Get the first child folder, which represents the SD card.
                StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();

                if (sdCard != null)
                {
                    virtualFolder.AddFolder(new StorageFolderEx(sdCard, "SDCARD"));
                }
            }
            catch
            {
                Debug.WriteLine("EXT_STORAGE_ERROR: There was a problem accessing external storage.");
            }

            try
            {
                StorageFolder assetFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");

                StorageFolder samples = await assetFolder.GetFolderAsync("Samples");

                virtualFolder.AddFolder(new StorageFolderEx(samples));
            }
            catch
            {
                Debug.WriteLine("INT_STORAGE_ERROR: There was a problem accessing internal storage.");
            }

            GetTreeForFolder(virtualFolder);
        }

        public bool OnlyLocalStorage { get; set; }

        /// <summary>
        ///     Will retrieve the full folder and file tree for a folder from the internal storage.
        /// </summary>
        /// <param name="folder">The instance of the folder for which the tree will be retrieved.</param>
        private async void GetTreeForFolder(IStorageFolderEx folder)
        {
            if (!FolderTree.Contains(folder))
                FolderTree.Push(folder);

            ProcessSelectedItems();

            CurrentItems.Clear();

            IEnumerable<IStorageFolderEx> folderList = await folder.GetFoldersAsync();

            foreach (IStorageFolderEx _folder in folderList)
            {
                FileExplorerItem item =
                    (from c in SelectedItems where c.Path == _folder.Path select c).FirstOrDefault();

                var _addItem = new FileExplorerItem
                {
                    IsFolder = true,
                    Name = _folder.Name,
                    DisplayName = _folder.DisplayName,
                    Path = _folder.Path,
                    Selected =  false
                };

                CurrentItems.Add(_addItem);
            }

            IEnumerable<IStorageFileEx> fileList = await folder.GetFilesAsync();

            if (fileList != null)
            {
                foreach (IStorageFileEx _file in fileList)
                {
                    //FileExplorerItem item = GetItemFromPath(_file.Path);

                    if (((ExtensionRestrictions & (ExtensionRestrictions.Custom | ExtensionRestrictions.InheritManifest)) !=
                         0) && (Extensions.Count != 0))
                    {
                        string extension = Path.GetExtension(_file.Name);
                        if (Extensions.FindIndex(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)) != -1)
                        {
                            CurrentItems.Add(new FileExplorerItem
                            {
                                IsFolder = false,
                                Name = _file.Name,
                                DisplayName = _file.Name,
                                Path = _file.Path,
                                Selected = false
                            });
                        }
                    }
                    else
                    {
                        CurrentItems.Add(new FileExplorerItem
                        {
                            IsFolder = false,
                            Name = _file.Name,
                            Path = _file.Path,
                            DisplayName = _file.Name,
                            Selected = false
                        });
                    }
                }
            }

            CurrentPath = FolderTree.First()
                .Path;
        }

        private FileExplorerItem GetItemFromPath(string path)
        {
            return (from c in SelectedItems where c.Path == path select c).FirstOrDefault();
        }

        private void ProcessSelectedItems()
        {
            foreach (FileExplorerItem item in CurrentItems)
            {
                FileExplorerItem targetItem = GetItemFromPath(item.Path);
                if (item.Selected && targetItem == null)
                {
                    SelectedItems.Add(item);
                }
                else if (!item.Selected && targetItem != null)
                {
                    SelectedItems.Remove(GetItemFromPath(item.Path));
                }
            }
        }

        private async Task SelectedSingleItem(FileExplorerItem selectedItem)
        {
            foreach (FileExplorerItem item in CurrentItems)
            {
                if (item!= selectedItem)
                {
                    item.Selected = false;
                }
                else
                {
                    item.Selected = true;

                    if (SelectionMode == SelectionMode.FileWithOpen)
                    {
                        try
                        {
                            IStorageFolderEx folder = FolderTree.First();
                            IStorageFileEx file = await folder.GetFileAsync(item.Name);

                            if (_player != null)
                            {
                                _player.Dispose();
                            }

                            if (file != null)
                            {
                                _player = new SamplePlayer();

                                _player.WithVoicePool(AudioDefines.VoicePool)
                                    .WithXAudio(AudioDefines.XAudio)
                                    .WithWaveFormat(AudioDefines.WaveFormat)
                                    .WithChannelVolume(0.8)
                                    .WithInput(file)
                                    .BuildAsync().ContinueWith(task => task.Result.Play(), TaskScheduler.Default);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }

                FileExplorerItem targetItem = GetItemFromPath(item.Path);
                if (item.Selected && targetItem == null)
                {
                    SelectedItems.Add(item);
                }
                else if (!item.Selected && targetItem != null)
                {
                    SelectedItems.Remove(GetItemFromPath(item.Path));
                }
            }
        }

        public void Show()
        {
            Initialize();
        }
        
        private void Dismiss(object file)
        {
            if (_player != null)
            {
                _player.Dispose();
            }

            SelectedSingleItem(null);
           
            RootPopup.IsOpen = false;
            
            if (OnDismiss != null)
                OnDismiss(StorageTarget, file);
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                Execute.OnUIThreadAsync(() => PropertyChanged(this, new PropertyChangedEventArgs(info)));
            }
        }

        //private async void FileExplorerItemSelect(object sender, GestureEventArgs e)
        //{
        //    var item = ((FrameworkElement) sender).Tag as FileExplorerItem;

        //    if (item == null)
        //    {
        //        return;
        //    }

        //    if (item.IsFolder)
        //    {
        //        GetTreeForFolder(await FolderTree.First()
        //            .GetFolderAsync(item.Name));
        //    }
        //    else
        //    {
        //        if (SelectionMode == SelectionMode.FileWithOpen)
        //        {
                  
        //            SelectedSingleItem(item);
        //        }
        //    }
        //}

        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;

            if (check != null)
            {
                check.IsChecked = false;
            }

            ProcessSelectedItems();

            if (SelectionMode == SelectionMode.FileWithOpen)
            {
                try
                {
                    IStorageFolderEx folder = FolderTree.Pop();

                    if (SelectedItems == null || !SelectedItems.Any())
                    {
                        Dismiss(folder);
                        return;
                    }

                    IStorageFileEx file = await folder.GetFileAsync(SelectedItems.First().Name);

                    if (file != null)
                    {
                        Dismiss(file);
                        return;
                    }

                    Dismiss(folder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Can't Dismiss", ex.Message);
                }

                Dismiss(null);
            }
            else
            {
                Dismiss(SelectedItems);
            }
        }

        private void BtnDismiss_OnClick(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;

            if (check != null)
            {
                check.IsChecked = false;
            }

            Dismiss(null);
        }

        private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).Tag as FileExplorerItem;

            if (item == null)
            {
                return;
            }

            if (item.IsFolder)
            {
                var folder = FolderTree.First();

                if (folder.Path == item.Path)
                {
                    return;
                }

                GetTreeForFolder(await FolderTree.First()
                    .GetFolderAsync(item.Name));
            }
            else
            {
                if (SelectionMode == SelectionMode.FileWithOpen)
                {

                    SelectedSingleItem(item);
                }
           }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;

            if (check != null)
            {
                check.IsChecked = false;
            }

            SelectedSingleItem(null);

            if (FolderTree.Count > 1)
            {
                FolderTree.Pop();
                GetTreeForFolder(FolderTree.First());
            }
            else
            {
                Dismiss(null);
            }
        }
    }
}