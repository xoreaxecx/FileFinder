using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace seeker
{
    public class VM : ObservableObject
    {
        #region fields

        long _folderCountLbl;
        long _fileCountLbl;
        double _totalSizeLbl;
        double _currentSizeLbl;
        string _timerLbl;
        string _sourceBtnText;
        string _containerBtnText;
        ObservableCollection<FileCondition> _conditions;
        ulong _minSize = 0;
        ulong _maxSize = 512;
        bool _startBtnEnabled;
        bool _startBtnWPDEnabled;
        long _counter;
        Brush _indColor;

        ICommand _sourceCommand;
        ICommand _containerCommand;
        ICommand _startCommand;
        ICommand _startWPDCommand;
        ICommand _stopCommand;
        ICommand _exitCommand;

        #endregion

        #region properties

        public List<DirectoryInfo> AcDirs { get; set; } = new List<DirectoryInfo>();
        public List<FileInfo> AcFiles { get; set; } = new List<FileInfo>();
        bool Blocked { get; set; } = false;
        public string SPath { get; set; }
        public string CPath { get; set; }
        public DispatcherTimer Timer { get; set; } = new DispatcherTimer();
        public List<string> SelectedConditions { get; set; } = new List<string>();

        DateTime StartTime { get; set; }

        public ObservableCollection<FileCondition> Conditions 
        {
            get { return _conditions; }
            set
            {
                if (value != _conditions)
                {
                    _conditions = value;
                    OnPropertyChanged("Conditions");
                }
            }
        } 

        public bool StartBtnEnabled
        {
            get { return _startBtnEnabled; }
            set
            {
                if (value != _startBtnEnabled)
                {
                    _startBtnEnabled = value;
                    OnPropertyChanged("StartBtnEnabled");
                }
            }
        }

        public bool StartBtnWPDEnabled
        {
            get { return _startBtnWPDEnabled; }
            set
            {
                if (value != _startBtnWPDEnabled)
                {
                    _startBtnWPDEnabled = value;
                    OnPropertyChanged("StartBtnWPDEnabled");
                }
            }
        }

        public Brush IndColor
        {
            get { return _indColor; }
            set
            {
                if (value != _indColor)
                {
                    _indColor = value;
                    OnPropertyChanged("IndColor");
                }
            }
        }

        public string SourceBtnText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_sourceBtnText))
                {
                    return "[source]";
                }
                else
                {
                    return _sourceBtnText;
                }
            }
            set
            {
                if (value != _sourceBtnText)
                {
                    _sourceBtnText = value;
                    OnPropertyChanged("SourceBtnText");
                }
            }
        }

        public string ContainerBtnText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_containerBtnText))
                {
                    return "[container]";
                }
                else
                {
                    return _containerBtnText;
                }
            }
            set
            {
                if (value != _containerBtnText)
                {
                    _containerBtnText = value;
                    OnPropertyChanged("ContainerBtnText");
                }
            }
        }

        public long FolderCountLbl
        {
            get { return _folderCountLbl; }
            set
            {
                if (value != _folderCountLbl)
                {
                    _folderCountLbl = value;
                    OnPropertyChanged("FolderCountLbl");
                }
            }
        }

        public long FileCountLbl
        {
            get { return _fileCountLbl; }
            set
            {
                if (value != _fileCountLbl)
                {
                    _fileCountLbl = value;
                    OnPropertyChanged("FileCountLbl");
                }
            }
        }

        public double TotalSizeLbl
        {
            get { return Math.Round(_totalSizeLbl, 2); }
            set
            {
                if (value != _totalSizeLbl)
                {
                    _totalSizeLbl = value;
                    OnPropertyChanged("TotalSizeLbl");
                }
            }
        }

        public double CurrentSizeLbl
        {
            get { return Math.Round(_currentSizeLbl, 2); }
            set
            {
                if (value != _currentSizeLbl)
                {
                    _currentSizeLbl = value;
                    OnPropertyChanged("CurrentSizeLbl");
                }
            }
        }

        public string TimerLbl
        {
            get { return _timerLbl; }
            set
            {
                if (value != _timerLbl)
                {
                    _timerLbl = value;
                    OnPropertyChanged("TimerLbl");
                }
            }
        }

        public ulong MinSize
        {
            get { return _minSize; }
            set
            {
                if (value != _minSize)
                {
                    _minSize = value;
                    OnPropertyChanged("MinSize");
                    CheckStartBtnEnabled();
                    CheckStartBtnWPDEnabled();
                }
            }
        }

        public ulong MaxSize
        {
            get { return _maxSize; }
            set
            {
                if (value != _maxSize)
                {
                    _maxSize = value;
                    OnPropertyChanged("MaxSize");
                    CheckStartBtnEnabled();
                    CheckStartBtnWPDEnabled();
                }
            }
        }

        #endregion

        #region commands

        public ICommand SourceCommand
        {
            get
            {
                if (_sourceCommand == null)
                {
                    _sourceCommand = new RelayCommand(
                        param => sourceBtn_Click());
                }
                return _sourceCommand;
            }
            set { }
        }

        public ICommand ContainerCommand
        {
            get
            {

                if (_containerCommand == null)
                {
                    _containerCommand = new RelayCommand(
                        param => containerBtn_Click());
                }
                return _containerCommand;
            }
            set { }
        }

        public ICommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                {
                    _startCommand = new RelayCommand(
                        async param => await Task.Run(() => startBtn_Click()));
                }
                return _startCommand;
            }
            set { }
        }

        public ICommand StartWPDCommand
        {
            get
            {
                if (_startWPDCommand == null)
                {
                    _startWPDCommand = new RelayCommand(
                        async param => await Task.Run(() => startWPD_Click()));
                }
                return _startWPDCommand;
            }
            set { }
        }

        public ICommand StopCommand
        {
            get
            {
                if (_stopCommand == null)
                {
                    _stopCommand = new RelayCommand(
                        param => stopBtn_Click());
                }
                return _stopCommand;
            }
            set { }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new RelayCommand(
                        param => exitBtn_Click());
                }
                return _exitCommand;
            }
            set { }
        }

        #endregion

        public VM()
        {
            IndColor = new SolidColorBrush(Colors.Red);

            Timer.Tick += timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);

            SetConditionCollection();
        }

        private void SetConditionCollection()
        {
            Conditions = new ObservableCollection<FileCondition>
            {
                new FileCondition { Name = ".jpg"},
                new FileCondition { Name = ".png"},
                new FileCondition { Name = ".mp3"},
                new FileCondition { Name = ".mp4"},
                new FileCondition { Name = ".mpeg4"},
                new FileCondition { Name = ".avi"},
                new FileCondition { Name = ".mov"},
                new FileCondition { Name = ".3gp"}
            };
        }

        private void SelectConditions()
        {
            foreach(var condition in Conditions)
            {
                if (condition.IsChecked)
                    SelectedConditions.Add(condition.Name);
            }
        }

        private bool ContainsCondition(string fileName)
        {
            foreach(var condition in SelectedConditions)
            {
                if (fileName.Contains(condition))
                    return true;
            }

            return false;
        }

        private bool AllowedSize(long size)
        {
            return (ulong)size > MinSize * 1024 * 1024 && (ulong)size < MaxSize * 1024 * 1024;
        }

        private void sourceBtn_Click()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            SPath = dialog.SelectedPath;
            if (!string.IsNullOrWhiteSpace(SPath))
            {
                SourceBtnText = SPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
            }
            else
            {
                SourceBtnText = null;
            }

            CheckStartBtnEnabled();
        }

        private void exitBtn_Click()
        {
            Environment.Exit(0);
        }

        private void containerBtn_Click()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            CPath = dialog.SelectedPath;

            if (!string.IsNullOrWhiteSpace(CPath))
            {
                ContainerBtnText = CPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
            }
            else
            {
                ContainerBtnText = null;
            }

            CheckStartBtnEnabled();
            CheckStartBtnWPDEnabled();
            CPath += "\\log report\\" + DateTime.Now.ToString("dd.MM.yyyy");
        }

        private void stopBtn_Click()
        {
            Blocked = true;
        }

        private void startBtn_Click()
        {
            SetYellowLbl();
            SelectConditions();
            AcDirs.Clear();
            AcFiles.Clear();

            Directory.CreateDirectory(CPath);
            GetAccessibleDirs(new DirectoryInfo(SPath));
            FolderCountLbl = AcDirs.Count();

            foreach (var dir in AcDirs)
            {
                if (Blocked)
                    break;
                var temp = SelectFiles();
                if (temp.Count > 0)
                    CopyFiles(temp);
            }

            SetGreenRedLbl();
        }


        void GetAccessibleDirs(DirectoryInfo dir)
        {
            try
            {
                foreach (FileInfo f in dir.GetFiles("*"))
                {
                    if (Blocked)
                        return;
                    AcFiles.Add(f);
                }
            }
            catch
            {
                return;
            }

            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                AcDirs.Add(d);
                GetAccessibleDirs(d);
            }
        }

        List<FileInfo> SelectFiles()
        {
            List<FileInfo> result = new List<FileInfo>();
            foreach (FileInfo f in AcFiles)
            {
                if (Blocked)
                    return result;

                if (f.Exists
                    && ContainsCondition(f.Name.ToLower())
                    && AllowedSize(f.Length))
                {
                    result.Add(f);
                }
            }
            FolderCountLbl -= 1;
            return result;
        }

        void CopyFiles(List<FileInfo> files)
        {
            Random rnd3 = new Random();
            foreach (var file in files)
            {
                if (Blocked)
                    return;

                CurrentSizeLbl = (double)file.Length / (1024 * 1024);
                if (File.Exists(CPath + "\\" + file.Name))
                {
                    while (File.Exists(CPath + "\\" + _counter.ToString() + file.Name))
                    {
                        _counter++;
                    }
                    file.CopyTo(CPath + "\\" + _counter.ToString() + file.Name, true);
                }
                else
                {
                    file.CopyTo(CPath + "\\" + file.Name, true);
                }
                File.SetAttributes(CPath + "\\" + file.Name, FileAttributes.Normal);
                FileCountLbl++;
                TotalSizeLbl += CurrentSizeLbl;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            TimerLbl = DateTime.Now.Subtract(StartTime).ToString(@"hh\:mm\:ss");
        }

        private void CheckStartBtnEnabled()
        {
            StartBtnEnabled = !(string.IsNullOrWhiteSpace(SPath)
                || string.IsNullOrWhiteSpace(CPath)
                || MinSize >= MaxSize);
        }

        private void CheckStartBtnWPDEnabled()
        {
            StartBtnWPDEnabled = !(string.IsNullOrWhiteSpace(CPath)
                || MinSize >= MaxSize);
        }

        private void startWPD_Click()
        {
            SetYellowLbl();

            var collection = new PortableDeviceCollection();

            collection.Refresh();

            foreach (var device in collection)
            {
                device.Connect();

                var folder = device.GetContents();
                foreach (var item in folder.Files)
                {
                    if (item is PortableDeviceFolder)
                    {
                        DisplayFolderContents(device, (PortableDeviceFolder)item);
                    }
                    if (item is PortableDeviceFile)
                    {
                        SaveFile(device, (PortableDeviceFile)item);
                    }
                }

                device.Disconnect();
            }

            SetGreenRedLbl();
        }

        public void DisplayFolderContents(PortableDevice device, PortableDeviceFolder folder)
        {
            foreach (var item in folder.Files)
            {
                if (Blocked)
                    return;

                if (item is PortableDeviceFolder)
                {
                    DisplayFolderContents(device, (PortableDeviceFolder)item);
                }
                if (item is PortableDeviceFile)
                {
                    SaveFile(device, (PortableDeviceFile)item);
                }
            }
        }

        public void SaveFile(PortableDevice device, PortableDeviceFile item)
        {
            if (ContainsCondition(item.Name))
            {
                device.DownloadFile((PortableDeviceFile)item, CPath);
            }
        }

        public void SetYellowLbl()
        {
            Blocked = false;
            Timer.Start();
            StartTime = DateTime.Now;

            App.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                IndColor = new SolidColorBrush(Colors.Yellow);
            });
        }

        public void SetGreenRedLbl()
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
             {
                 if (Blocked)
                 {
                     IndColor = new SolidColorBrush(Colors.Red);
                 }
                 else
                 {
                     IndColor = new SolidColorBrush(Colors.Green);
                 }
             }));

            Timer.Stop();
        }
    }
}