using Caliburn.Micro;

namespace FileExplorerUniversal.Control.Interop
{
    public class FileExplorerItem : PropertyChangedBase
    {
        public FileExplorerItem()
        {
            
        }

        private string _name;
        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if (value != _displayName)
                {
                    _displayName = value;
                    NotifyOfPropertyChange(() => DisplayName);
                }
            }
        }

        private string _path;
        public string Path 
        {
            get
            {
                return _path;
            }
            set
            {
                if (value != _path)
                {
                    _path = value;
                    NotifyOfPropertyChange(() => Path);
                }
            }
        }

        private bool _isFolder;
        public bool IsFolder 
        {
            get
            {
                return _isFolder;
            }
            set
            {
                if (value != _isFolder)
                {
                    _isFolder = value;
                    NotifyOfPropertyChange(() => IsFolder);
                }
            }
        }

        private bool _selected;
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    NotifyOfPropertyChange(() => Selected);
                }
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
