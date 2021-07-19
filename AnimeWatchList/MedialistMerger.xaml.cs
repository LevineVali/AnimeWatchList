using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AnimeWatchList
{
    /// <summary>
    /// Interaktionslogik für MedialistMerger.xaml
    /// </summary>
    public partial class MedialistMerger : Window
    {
        /// <summary>
        /// reference of mainwindow
        /// </summary>
        public MainWindow mainWindow = null;

        public List<Anime> leftAnimeList = new List<Anime>();
        public List<Anime> newAnimeList = new List<Anime>();
        public List<Anime> rightAnimeList = new List<Anime>();

        // reference to the labels
        public Label LeftAnimeList_Name;
        public Label RightAnimeList_Name;

        public List<Indexer> IndecesLeft = new List<Indexer>();
        public List<Indexer> IndecesRight = new List<Indexer>();

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

        public MedialistMerger()
        {
            InitializeComponent();

            // get labes that represents listnames(filenames)
            LeftAnimeList_Name = (Label)this.FindName("label_ListName_1");
            RightAnimeList_Name = (Label)this.FindName("label_ListName_2");

            // set buttonpictures for titlebar
            // close button
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\CloseButton.png";

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
            // get all elements to translate
            Button allLeft = (Button)this.FindName("button_AllLeft");
            Button loadLeft = (Button)this.FindName("button_LoadLeft");
            Button allRight = (Button)this.FindName("button_AllRight");
            Button loadRight = (Button)this.FindName("button_LoadRight");
            Button saveNewList = (Button)this.FindName("button_SaveNewList");
            Label newListName = (Label)this.FindName("label_NewListName");

            // translate them
            allLeft.Content = _languageDetail.ButtonAll;
            allRight.Content = _languageDetail.ButtonAll;
            loadLeft.Content = _languageDetail.MenuItemLoad;
            loadRight.Content = _languageDetail.MenuItemLoad;
            saveNewList.Content = _languageDetail.MenuItemSave;
            newListName.Content = _languageDetail.LabelNewList;

            // set click events for buttons just to not write another function for it xD
            loadLeft.Click += mainWindow.MenuItem_OpenSaveLoadWindow;
            loadRight.Click += mainWindow.MenuItem_OpenSaveLoadWindow;
            saveNewList.Click += mainWindow.MenuItem_OpenSaveLoadWindow;
            // set dropevents
            label_AnimeList_New.Drop += new DragEventHandler(mainWindow.TargetDrop);
            button_RemoveAnimeNewList.Drop += new DragEventHandler(mainWindow.TargetDrop);
            // set trashicon
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\images\\Trash.png";
            if (File.Exists(path))
                image_Trash.Source = new BitmapImage(new Uri(path));
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // set right sizes for each scrollviewer
            scrollviewer_LeftList.Width = e.NewSize.Width / 3 - 27;
            scrollviewer_LeftList.Height = e.NewSize.Height - 137;

            scrollviewer_NewList.Width = e.NewSize.Width / 3 - 27;
            scrollviewer_NewList.Height = e.NewSize.Height - 137;

            scrollviewer_RightList.Width = e.NewSize.Width / 3 - 27;
            scrollviewer_RightList.Height = e.NewSize.Height - 137;

            button_SaveNewList.Width = scrollviewer_NewList.Width - 45;
        }

        private void MoveAllAnimes(object sender, RoutedEventArgs e)
        {
            string typ = ((Button)sender).Tag.ToString();
            int index;

            if (typ == "Left")
            {
                for (int i = 0; i < dockpanel_AnimeList_Left.Children.Count; i++)
                {
                    // get right textblock
                    TextBlock tb = (TextBlock)this.FindName("textblock_AnimeName_" + i + "_Left");
                    // check its visibility
                    if (tb.IsEnabled == true)
                    {
                        // get index of anime
                        index = int.Parse(tb.Tag.ToString());

                        // set indexer
                        Indexer indexer = new Indexer
                        {
                            Key = newAnimeList.Count,
                            Value = index
                        };

                        if (mainWindow.CheckForExistingAnime(leftAnimeList[i], newAnimeList))
                        {
                            OwnMessageBox mb = new OwnMessageBox();
                            mb.SetElements(OwnMessageboxTyp.WARNING_REPLACE_ADD_ABORT, mainWindow.languageDetail.AnimeExists);
                            mb.SetLanguage(mainWindow.languageDetail);
                            mb.ShowDialog();

                            if (mb.MessageboxResult == OwnMessageboxResult.ADD)
                            {
                                // add anime to correct list and correct dockpanel
                                mainWindow.AddAnime(leftAnimeList[i], newAnimeList, ListTyp.MERGENEW, this, true, i);
                                label_AnimeList_New.AllowDrop = false;
                                button_SaveNewList.IsEnabled = true;
                                button_RemoveAnimeNewList.IsEnabled = true;
                                // add index to indeceslist
                                IndecesLeft.Add(indexer);

                                if (IndecesLeft.Count == dockpanel_AnimeList_Left.Children.Count)
                                {
                                    button_AllLeft.IsEnabled = false;
                                }

                                continue;
                            }
                            else if (mb.MessageboxResult == OwnMessageboxResult.REPLACE)
                            {
                                mainWindow.ReEnableDragedAnime(mainWindow.IndexOfMatchedAnime);

                                // remove matched anime
                                newAnimeList[mainWindow.IndexOfMatchedAnime] = null;
                                dockpanel_AnimeList_New.Children[mainWindow.IndexOfMatchedAnime].Visibility = Visibility.Collapsed;

                                // add anime to correct list and correct dockpanel
                                mainWindow.AddAnime(leftAnimeList[i], newAnimeList, ListTyp.MERGENEW, this, true, i);
                                label_AnimeList_New.AllowDrop = false;
                                button_SaveNewList.IsEnabled = true;
                                button_RemoveAnimeNewList.IsEnabled = true;
                                // add index to indeceslist
                                IndecesLeft.Add(indexer);

                                if (IndecesLeft.Count == dockpanel_AnimeList_Left.Children.Count)
                                {
                                    button_AllLeft.IsEnabled = false;
                                }

                                continue;
                            }

                            continue;
                        }

                        IndecesLeft.Add(indexer);

                        mainWindow.AddAnime(leftAnimeList[i], newAnimeList, ListTyp.MERGENEW, this, true, i);
                        
                        tb.IsEnabled = false;
                        tb.Foreground = new SolidColorBrush(Colors.LightSlateGray);
                    }
                }

                button_AllLeft.IsEnabled = false;
                button_RemoveAnimeNewList.IsEnabled = true;
                button_SaveNewList.IsEnabled = true;
            }
            else if (typ == "Right")
            {
                for (int i = 0; i < dockpanel_AnimeList_Right.Children.Count; i++)
                {
                    // get right textblock
                    TextBlock tb = (TextBlock)this.FindName("textblock_AnimeName_" + i + "_Right");
                    // check its visibility
                    if (tb.IsEnabled == true)
                    {
                        // get index of anime
                        index = int.Parse(tb.Tag.ToString());

                        // set indexer
                        Indexer indexer = new Indexer
                        {
                            Key = newAnimeList.Count,
                            Value = index
                        };

                        if (mainWindow.CheckForExistingAnime(rightAnimeList[i], newAnimeList))
                        {
                            OwnMessageBox mb = new OwnMessageBox();
                            mb.SetElements(OwnMessageboxTyp.WARNING_REPLACE_ADD_ABORT, mainWindow.languageDetail.AnimeExists);
                            mb.SetLanguage(mainWindow.languageDetail);
                            mb.ShowDialog();

                            if (mb.MessageboxResult == OwnMessageboxResult.ADD)
                            {
                                // add anime to correct list and correct dockpanel
                                mainWindow.AddAnime(rightAnimeList[i], newAnimeList, ListTyp.MERGENEW, this, true, i);
                                label_AnimeList_New.AllowDrop = false;
                                button_SaveNewList.IsEnabled = true;
                                button_RemoveAnimeNewList.IsEnabled = true;
                                // add index to indeceslist
                                IndecesRight.Add(indexer);

                                if (IndecesRight.Count == dockpanel_AnimeList_Right.Children.Count)
                                {
                                    button_AllRight.IsEnabled = false;
                                }

                                tb.IsEnabled = false;
                                tb.Foreground = new SolidColorBrush(Colors.LightSlateGray);

                                continue;
                            }
                            else if (mb.MessageboxResult == OwnMessageboxResult.REPLACE)
                            {
                                mainWindow.ReEnableDragedAnime(mainWindow.IndexOfMatchedAnime);

                                // remove matched anime
                                newAnimeList[mainWindow.IndexOfMatchedAnime] = null;
                                dockpanel_AnimeList_New.Children[mainWindow.IndexOfMatchedAnime].Visibility = Visibility.Collapsed;

                                // add anime to correct list and correct dockpanel
                                mainWindow.AddAnime(rightAnimeList[i], newAnimeList, ListTyp.MERGENEW, this, true, i);
                                label_AnimeList_New.AllowDrop = false;
                                button_SaveNewList.IsEnabled = true;
                                button_RemoveAnimeNewList.IsEnabled = true;
                                // add index to indeceslist
                                IndecesRight.Add(indexer);

                                if (IndecesRight.Count == dockpanel_AnimeList_Right.Children.Count)
                                {
                                    button_AllRight.IsEnabled = false;
                                }

                                tb.IsEnabled = false;
                                tb.Foreground = new SolidColorBrush(Colors.LightSlateGray);

                                continue;
                            }

                            continue;
                        }

                        IndecesRight.Add(indexer);

                        mainWindow.AddAnime(rightAnimeList[i], newAnimeList, ListTyp.MERGENEW, this, true, i);

                        tb.IsEnabled = false;
                        tb.Foreground = new SolidColorBrush(Colors.LightSlateGray);
                    }
                }

                button_AllRight.IsEnabled = false;
                button_RemoveAnimeNewList.IsEnabled = true;
                button_SaveNewList.IsEnabled = true;
            }
        }
    }

    /// <summary>
    /// Class to track which anime from which list is Draged to the newList to re-enable the disabled TextBlock of draged anime
    /// </summary>
    public class Indexer
    {
        /// <summary>
        /// Index of the anime in the new List
        /// </summary>
        public int Key;

        /// <summary>
        /// Index of the anime in the other List from where this anime is comming
        /// </summary>
        public int Value;
    }
}