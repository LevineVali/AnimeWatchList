using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace AnimeWatchList
{
    /// <summary>
    /// Interaktionslogik für SaveLoadAnimeListWindow.xaml
    /// </summary>
    public partial class SaveLoadAnimeListWindow : Window
    {
        /// <summary>
        /// 0 = Load | 
        /// 1 = Save
        /// </summary>
        public int LoadSave;

        /// <summary>
        /// reference of mainwindow
        /// </summary>
        public MainWindow mainWindow = null;

        /// <summary>
        /// reference of Window that opened this window
        /// </summary>
        public Window window = null;

        /// <summary>
        /// list with all Filenames that exists in saves-folder
        /// </summary>
        private List<string> existingFileNames = new List<string>();

        /// <summary>
        /// Textbox of filename
        /// </summary>
        public TextBox textBox_FileName;

        /// <summary>
        /// Button for save file
        /// </summary>
        private Button button_SaveFile;

        private static int minWidth;
        private static int minHeight;

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = minWidth;
                mmi.ptMinTrackSize.y = minHeight;
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        public SaveLoadAnimeListWindow()
        {
            InitializeComponent();

            // set buttonpictures for titlebar
            // close button
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\Folder.png";

            if (File.Exists(filePath))
                image_Directory.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images\\Folder.png"));

            filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\CloseButton.png";

            if (File.Exists(filePath))
                image_Close.Source = new BitmapImage(new Uri(filePath));

            // minimize button
            filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\MinimizedButton.png";

            if (File.Exists(filePath))
                image_Minimized.Source = new BitmapImage(new Uri(filePath));

            // small-big-changer button

            if (WindowState == WindowState.Maximized)
            {
                filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\SmallButton.png";
                if (File.Exists(filePath))
                    image_MinMax.Source = new BitmapImage(new Uri(filePath));
            }
            else
            {
                filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\BigButton.png";
                if (File.Exists(filePath))
                    image_MinMax.Source = new BitmapImage(new Uri(filePath));
            }

            minWidth = (int)MinWidth;
            minHeight = (int)MinHeight;

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            button_Close.Click += (sender, e) => Close();
            button_Minimized.Click += (sender, e) => WindowState = WindowState.Minimized;
            button_MinMax.Click += (sender, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public void SetLanguage(LanguageDetail _languageDetail)
        {
            // get window
            Window window = (Window)this.FindName("SaveLoadWindow");

            // if loading
            if (LoadSave == 0)
            {
                // set titel
                window.Title = _languageDetail.MenuItemLoad;
            }
            // if saving
            else if (LoadSave == 1)
            {
                // set titel
                window.Title = _languageDetail.MenuItemSave;
            }
        }

        public void GetSavedFiles(LanguageDetail _languageDetail, ListTyp _listTyp)
        {
            // get all paths of all animelist savefiles in saves folder
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "saves\\", "*.ml");

            // get dockpanel
            DockPanel dockPanel = (DockPanel)this.FindName("dockpanel_FileList");

            // if no animelist savefile found
            if (filePaths.Length == 0)
            {
                Label label = new Label
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    Content = _languageDetail.NoFileFound
                };
                DockPanel.SetDock(label, Dock.Top);
                dockPanel.Children.Add(label);
            }
            // otherwise files are founded
            else
            {
                // go through all founded files
                foreach (string s in filePaths)
                {
                    // create formatter
                    BinaryFormatter formatter = new BinaryFormatter();

                    // create filestream
                    FileStream stream = new FileStream(s, FileMode.Open);

                    List<Anime> animeList = new List<Anime>();

                    // try open the file
                    try
                    {
                        animeList = formatter.Deserialize(stream) as List<Anime>;
                        stream.Close();

                        // create new button
                        Button button = new Button
                        {
                            Content = GetFileNameOnly(s),
                            Margin = new Thickness(0, 0, 0, 5),
                            Foreground = new SolidColorBrush(Colors.White),
                            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D)),
                            BorderBrush = Background
                        };
                        button.Style = Resources["ButtonStyleOwn"] as Style;
                        switch (_listTyp)
                        {
                            case ListTyp.MAINWINDOW:
                                button.Tag = s + "_mainwindow";
                                break;
                            case ListTyp.MERGELEFT:
                                button.Tag = s + "_left";
                                break;
                            case ListTyp.MERGENEW:
                                button.Tag = s + "_new";
                                break;
                            case ListTyp.MERGERIGHT:
                                button.Tag = s + "_right";
                                break;
                        }

                        if (LoadSave == 0)
                        {
                            button.Click += LoadFile;
                        }
                        else if (LoadSave == 1)
                        {
                            button.Click += SaveFile;
                        }
                        DockPanel.SetDock(button, Dock.Top);

                        // add button to dockpanel
                        dockPanel.Children.Add(button);

                        // add existing filename to list
                        existingFileNames.Add(GetFileNameOnly(s));
                    }
                    catch
                    {
                        stream.Close();
                    }
                }
            }

            // if savewindow
            if (LoadSave == 1)
            {
                // create sub dockpanel for label, textbox and button
                DockPanel subDockPanel = new DockPanel
                {
                    LastChildFill = false
                };
                DockPanel.SetDock(subDockPanel, Dock.Top);

                Label subLabel = new Label
                {
                    Content = _languageDetail.LabelFileName,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                DockPanel.SetDock(subLabel, Dock.Top);

                Grid grid = new Grid();

                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength();
                ColumnDefinition c2 = new ColumnDefinition();
                c2.Width = new GridLength(80, GridUnitType.Auto);

                grid.ColumnDefinitions.Add(c1);
                grid.ColumnDefinitions.Add(c2);

                DockPanel.SetDock(grid, Dock.Top);

                TextBox textBox = new TextBox
                {
                    Name = "textbox_FileName",
                    Height = 20,
                    MinWidth = 150,
                    Margin = new Thickness(0, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                textBox.TextChanged += OnFilenameChange;
                textBox_FileName = textBox;
                Grid.SetColumn(textBox, 0);

                Button button = new Button
                {
                    Content = _languageDetail.MenuItemSave,
                    Padding = new Thickness(5, 0, 5, 0),
                    Tag = "_SaveFile_",
                    Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                    BorderBrush = Background,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                button.Click += SaveFile;
                button.Style = Resources["ButtonStyleOwn"] as Style;
                button_SaveFile = button;
                Grid.SetColumn(button, 1);

                grid.Children.Add(textBox);
                grid.Children.Add(button);

                subDockPanel.Children.Add(subLabel);
                subDockPanel.Children.Add(grid);

                dockPanel.Children.Add(subDockPanel);
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            // get savepath or typ
            string pathTyp = ((Button)sender).Tag.ToString().Split("_")[0];

            MainWindow mainWindowTMP = null;
            MedialistMerger mergeWindowTMP = null;

            try
            {
                mainWindowTMP = window as MainWindow;
            }
            catch { }

            try
            {
                mergeWindowTMP = window as MedialistMerger;
            }
            catch { }

            if (mainWindowTMP != null)
            {
                // save file from mainwindow
                SaveFile(mainWindowTMP.Animes, pathTyp, sender);
            }
            else if (mergeWindowTMP != null)
            {
                // save file from mediamerger
                SaveFile(mergeWindowTMP.newAnimeList, pathTyp, sender);
            }
        }

        public void SaveFile(List<Anime> _animeList, string _pathTyp, object sender)
        {
            List<Anime> animeList = new List<Anime>();

            foreach (Anime a in _animeList)
            {
                if (a != null)
                    animeList.Add(a);
            }

            // if we save a new file
            if (_pathTyp == "_SaveFile_")
            {
                // create formatter
                BinaryFormatter formatter = new BinaryFormatter();

                // create filestream
                FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "saves\\" + textBox_FileName.Text + ".ml", FileMode.Create);

                // save the file
                formatter.Serialize(stream, animeList);

                // close filestream
                stream.Close();

                // close savewindow
                Close();
            }
            // if we overwrite an existing file
            else
            {
                // create new filestream
                FileStream stream = new FileStream(((Button)sender).Tag.ToString(), FileMode.Open);

                // get filename without ending like ".exe"
                string fileName = GetFileNameOnly(stream.Name);

                OwnMessageBox mb = new OwnMessageBox();
                mb.SetElements(OwnMessageboxTyp.WARNING_YES_NO, mainWindow.languageDetail.AnimeExists);
                mb.SetLanguage(mainWindow.languageDetail);
                mb.ShowDialog();

                if (mb.MessageboxResult == OwnMessageboxResult.YES)
                {
                    // create formatter
                    BinaryFormatter formatter = new BinaryFormatter();

                    // save the file
                    formatter.Serialize(stream, animeList);

                    // close filestream
                    stream.Close();

                    // close savewindow
                    Close();
                }
                else
                {
                    stream.Close();
                }
            }
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            string[] path_typ = ((Button)sender).Tag.ToString().Split("_");

            switch (path_typ[1])
            {
                case "mainwindow":
                    LoadFile(ListTyp.MAINWINDOW, mainWindow, path_typ[0]);
                    break;
                case "left":
                    LoadFile(ListTyp.MERGELEFT, window, path_typ[0]);
                    // clear indeceslist because new list is loaded
                    mainWindow.mergeWindow.IndecesLeft = new List<Indexer>();
                    mainWindow.mergeWindow.button_AllLeft.IsEnabled = true;
                    break;
                case "new":
                    LoadFile(ListTyp.MERGENEW, window, path_typ[0]);
                    break;
                case "right":
                    LoadFile(ListTyp.MERGERIGHT, window, path_typ[0]);
                    // clear indeceslist because new list is loaded
                    mainWindow.mergeWindow.IndecesRight = new List<Indexer>();
                    mainWindow.mergeWindow.button_AllRight.IsEnabled = true;
                    break;
            }
        }

        public void LoadFile(ListTyp _listTyp, Window _window, string _path)
        {
            // create formatter
            BinaryFormatter formatter = new BinaryFormatter();

            // create filestream
            FileStream stream = new FileStream(_path, FileMode.Open);

            // set animelist
            List<Anime> animeList = formatter.Deserialize(stream) as List<Anime>;

            // close filestream
            stream.Close();

            // get right animelist
            List<Anime> animeListToFill = new List<Anime>();

            MedialistMerger mergewindow = null;

            if (_listTyp != ListTyp.MAINWINDOW)
            {
                mergewindow = _window as MedialistMerger;
            }

            bool editButton = false;

            switch(_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    animeListToFill = mainWindow.Animes;
                    editButton = true;
                    break;
                case ListTyp.MERGELEFT:
                    animeListToFill = mergewindow.leftAnimeList;
                    editButton = false;
                    ((Label)mergewindow.FindName("label_ListName_1")).Content = GetFileNameOnly(_path);
                    break;
                case ListTyp.MERGENEW:
                    animeListToFill = mergewindow.newAnimeList;
                    editButton = true;
                    break;
                case ListTyp.MERGERIGHT:
                    animeListToFill = mergewindow.rightAnimeList;
                    editButton = false;
                    ((Label)mergewindow.FindName("label_ListName_2")).Content = GetFileNameOnly(_path);
                    break;
            }

            // clear dockpanel
            mainWindow.ClearDockpanel(animeListToFill , _listTyp, _window);

            animeListToFill.Clear();

            // go through all animes in the list
            for (int i = 0; i < animeList.Count; i++)
            {
                // add anime to right list and window
                mainWindow.AddAnime(animeList[i], animeListToFill, _listTyp, _window, editButton, null);
            }

            // close load window
            Close();
        }

        private void OpenSavesFolder(object sender, RoutedEventArgs e)
        {
            // open savesfolder
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory + "saves");
        }

        public string GetFileNameOnly(string _filePath)
        {
            string helper = null;

            // go through all chars of filepath from back to front
            for (int i = _filePath.Length - 1; i >= 0; i--)
            {
                // if last char of filename has added
                if (_filePath[i] == '\\')
                {
                    helper = helper.Split(".")[1];
                    break;
                }
                // add char
                helper += _filePath[i];
            }

            string result = null;

            // go through all chars of helper string from back to front
            for (int i = helper.Length - 1; i >= 0; i--)
            {
                result += helper[i];
            }

            return result;
        }

        private void OnFilenameChange(object sender, TextChangedEventArgs e)
        {
            // go through all filenames
            foreach (string s in existingFileNames)
            {
                // if current text of textbox is equal to one existing filename
                if (s == textBox_FileName.Text)
                {
                    // disable savebutton and return to end this function task
                    button_SaveFile.IsEnabled = false;
                    return;
                }
            }

            // if no existingfilename matched current textbox text, enable savebutton
            button_SaveFile.IsEnabled = true;
        }
    }
}
