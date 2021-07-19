using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace AnimeWatchList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// If existing anime found, this is the index of the checked list
        /// </summary>
        public int IndexOfMatchedAnime;

        /// <summary>
        /// If existing anime found, this is a reference of the anime that was foundend int the checked list
        /// </summary>
        public Anime MatchedAnime;

        /// <summary>
        /// List with all Genres
        /// </summary>
        public Genre GenreList = new Genre();

        /// <summary>
        /// list of all Animes
        /// </summary>
        public List<Anime> Animes = new List<Anime>();

        /// <summary>
        /// List of all languagesdetails
        /// </summary>
        public List<LanguageDetail> LanguageDetails = new List<LanguageDetail>();

        /// <summary>
        /// setting of system-language
        /// </summary>
        public string Setting;

        /// <summary>
        /// current language with right texts
        /// </summary>
        public LanguageDetail languageDetail;

        /// <summary>
        /// Count of Anime of this window.
        /// Is required for RegisterName of Elements in it
        /// </summary>
        public int AnimeCount;

        /// <summary>
        /// reference to the MedialistMergerer
        /// </summary>
        public MedialistMerger mergeWindow = null;

        /// <summary>
        /// MenuItem for saving animelist
        /// </summary>
        private MenuItem saveAnimeList;

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

        public MainWindow()
        {
            InitializeComponent();

            // create folder if they are missing
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "data");
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "saves");

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "data\\settings.dat";

            // get menuitem for saving animelist
            saveAnimeList = (MenuItem)this.FindName("menuitem_Save");

            // disable saving animelist because no animelist is loaded in mainwindow
            saveAnimeList.IsEnabled = false;

            // if settings file exist
            if (File.Exists(filePath))
            {
                // load setting file
                // create formatter
                BinaryFormatter formatter = new BinaryFormatter();

                // create filestream
                FileStream stream = new FileStream(filePath, FileMode.Open);

                // try open the file
                try
                {
                    Setting = formatter.Deserialize(stream) as string;
                    stream.Close();
                }
                catch
                {
                    stream.Close();
                    CreateDefaultSettingData(filePath);
                }
            }
            // otherwise
            else
            {
                CreateDefaultSettingData(filePath);
            }

            filePath = AppDomain.CurrentDomain.BaseDirectory + "data\\genre.dat";

            // if genre file exist
            if (File.Exists(filePath))
            {
                // load genre file
                // create formatter
                BinaryFormatter formatter = new BinaryFormatter();

                // create filestream
                FileStream stream = new FileStream(filePath, FileMode.Open);

                // try open the file
                try
                {
                    GenreList = formatter.Deserialize(stream) as Genre;
                    stream.Close();
                }
                catch
                {
                    stream.Close();
                    CreateDefaultGenreData(filePath);
                }
            }
            // otherwise
            else
            {
                CreateDefaultGenreData(filePath);
            }

            filePath = AppDomain.CurrentDomain.BaseDirectory + "data\\languagedetail.dat";

            // if genre file exist
            if (File.Exists(filePath))
            {
                // load genre file
                // create formatter
                BinaryFormatter formatter = new BinaryFormatter();

                // create filestream
                FileStream stream = new FileStream(filePath, FileMode.Open);

                // try open the file
                try
                {
                    LanguageDetails = formatter.Deserialize(stream) as List<LanguageDetail>;
                    stream.Close();
                }
                catch
                {
                    stream.Close();
                    CreateDefaultLanguagesDetailData(filePath);
                }
            }
            // otherwise
            else
            {
                CreateDefaultLanguagesDetailData(filePath);
            }

            foreach (LanguageDetail ld in LanguageDetails)
            {
                if (ld.Name == Setting)
                {
                    SetLanguage(ld);
                    languageDetail = ld;
                }

                CreateLanguageMenuItem(ld);
            }

            // set buttonpictures for titlebar
            // close button
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

        public void AddAnime(Anime _anime)
        {
            // add anime to mainwindow animelist
            AddAnime(_anime, Animes, ListTyp.MAINWINDOW, this, true, null);

            // increase animecount
            AnimeCount++;
        }

        public void AddAnime(Anime _anime, List<Anime> _animeList, ListTyp _listTyp, Window _window, bool _editButton, int? _index)
        {
            // get animecount
            int animeCount = _animeList.Count;

            if (_index != null)
                animeCount = (int)_index;

            // add Anime to the list
            _animeList.Add(_anime);

            // add animeblock to mainWindow
            // create MainGrid
            Grid MainGrid = new Grid();
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    MainGrid.Name = "gird_Anime_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    MainGrid.Name = "gird_Anime_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    MainGrid.Name = "gird_Anime_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    MainGrid.Name = "gird_Anime_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(MainGrid.Name, MainGrid);
            DockPanel.SetDock(MainGrid, Dock.Top);

            // create RowDefinitions
            RowDefinition R1 = new RowDefinition();
            RowDefinition R2 = new RowDefinition();

            // add rowdefenitions to MainGrid
            MainGrid.RowDefinitions.Add(R1);
            MainGrid.RowDefinitions.Add(R2);

            #region AnimeName
            // create dockpanel for animename
            DockPanel AnimeNameDockPanel = new DockPanel();
            Grid.SetRow(AnimeNameDockPanel, 0);
            AnimeNameDockPanel.Margin = new Thickness(0, 0, 0, 5);
            AnimeNameDockPanel.LastChildFill = false;
            AnimeNameDockPanel.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D));
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeNameDockPanel.Name = "dockpanel_AnimeName_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeNameDockPanel.Name = "dockpanel_AnimeName_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeNameDockPanel.Name = "dockpanel_AnimeName_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeNameDockPanel.Name = "dockpanel_AnimeName_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeNameDockPanel.Name, AnimeNameDockPanel);

            // create showhidebutton
            Button ShowHideButton = new Button();
            DockPanel.SetDock(ShowHideButton, Dock.Left);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    ShowHideButton.Tag = "Show_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    ShowHideButton.Tag = "Show_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    ShowHideButton.Tag = "Show_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    ShowHideButton.Tag = "Show_" + animeCount + "_Right";
                    break;
            }
            ShowHideButton.Content = "V";
            ShowHideButton.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x4D, 0x4D, 0x4D));
            ShowHideButton.BorderBrush = ShowHideButton.Background;
            ShowHideButton.Foreground = new SolidColorBrush(Colors.Wheat);
            ShowHideButton.Click += ShowHideDetail;
            ShowHideButton.Padding = new Thickness(5, 0, 5, 0);
            ShowHideButton.Style = Resources["ButtonStyleOwn"] as Style;

            // create textblock for animename
            TextBlock AnimeNameTextBlock = new TextBlock();
            DockPanel.SetDock(AnimeNameTextBlock, Dock.Left);
            AnimeNameTextBlock.Foreground = new SolidColorBrush(Colors.White);
            AnimeNameTextBlock.Margin = new Thickness(0, 0, 5, 0);
            AnimeNameTextBlock.Padding = new Thickness(5, 2, 0, 5);
            AnimeNameTextBlock.Height = 26;
            AnimeNameTextBlock.FontSize = 15;
            AnimeNameTextBlock.Tag = animeCount;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeNameTextBlock.Name = "textblock_AnimeName_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeNameTextBlock.Name = "textblock_AnimeName_" + animeCount + "_Left";
                    AnimeNameTextBlock.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(StartDragEvent);
                    break;
                case ListTyp.MERGENEW:
                    AnimeNameTextBlock.Name = "textblock_AnimeName_" + animeCount + "_New";
                    AnimeNameTextBlock.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(StartDragEvent);
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeNameTextBlock.Name = "textblock_AnimeName_" + animeCount + "_Right";
                    AnimeNameTextBlock.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(StartDragEvent);
                    break;
            }
            _window.RegisterName(AnimeNameTextBlock.Name, AnimeNameTextBlock);

            // create Combobox for change language of animename
            ComboBox AnimeNameLanguageComboBox = new ComboBox();
            DockPanel.SetDock(AnimeNameLanguageComboBox, Dock.Right);
            AnimeNameLanguageComboBox.Width = 200;
            AnimeNameLanguageComboBox.Height = 25;
            AnimeNameLanguageComboBox.SelectionChanged += OnSelectionChanged;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeNameLanguageComboBox.Tag = "N_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeNameLanguageComboBox.Tag = "N_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeNameLanguageComboBox.Tag = "N_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeNameLanguageComboBox.Tag = "N_" + animeCount + "_Right";
                    break;
            }

            // set right AnimeName and Language and set comboboxItems
            for (int i = 0; i < _anime.Names.Count; i++)
            {
                // create new ComboboxItems
                ComboBoxItem AnimeNameCBI = new ComboBoxItem();

                // create new dockpanel
                DockPanel dockPanel = new DockPanel();

                // find right path
                string path = "";
                for (int j = 0; j < LanguageDetails.Count; j++)
                {
                    if (_anime.Names[i].Name == LanguageDetails[j].Name)
                    {
                        path = LanguageDetails[j].PicturePath_Complete;
                    }
                }

                // create new image for icon
                Image icon = new Image()
                {
                    Width = 20,
                    Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + path))
                };
                DockPanel.SetDock(icon, Dock.Left);

                // create new label for language name
                Label text = new Label()
                {
                    Padding = new Thickness(0, -3, 0, 0),
                    Margin = new Thickness(5, 0, 0, 0),
                    Content = _anime.Names[i].Name,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                DockPanel.SetDock(text, Dock.Left);

                // add icon and text to dockpanel
                dockPanel.Children.Add(icon);
                dockPanel.Children.Add(text);

                // add dockpanel to comboboxitem
                AnimeNameCBI.Content = dockPanel;

                // add comboboxItem to Combobox
                AnimeNameLanguageComboBox.Items.Add(AnimeNameCBI);

                if (_anime.Names[i].Name == Setting)
                {
                    AnimeNameLanguageComboBox.SelectedIndex = i;
                    AnimeNameTextBlock.Text = _anime.Names[i].Value;
                }
                // last iteration and no name in the setting-language found
                if (i == _anime.Names.Count - 1 && AnimeNameTextBlock.Text == "")
                {
                    // set first name and language of the list
                    AnimeNameLanguageComboBox.SelectedIndex = 0;
                    AnimeNameTextBlock.Text = _anime.Names[0].Value;
                }
            }

            // add elements to DockPanel
            AnimeNameDockPanel.Children.Add(ShowHideButton);
            AnimeNameDockPanel.Children.Add(AnimeNameTextBlock);
            AnimeNameDockPanel.Children.Add(AnimeNameLanguageComboBox);
            #endregion

            #region AnimeDetail MainDockpanel
            // create MainDockPanel for animedetails
            DockPanel MainAnimeDetailDockpanel = new DockPanel();
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    MainAnimeDetailDockpanel.Name = "dockpanel_MainAnimeDetailDockpanel_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    MainAnimeDetailDockpanel.Name = "dockpanel_MainAnimeDetailDockpanel_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    MainAnimeDetailDockpanel.Name = "dockpanel_MainAnimeDetailDockpanel_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    MainAnimeDetailDockpanel.Name = "dockpanel_MainAnimeDetailDockpanel_" + animeCount + "_Right";
                    break;
            }
            DockPanel.SetDock(MainAnimeDetailDockpanel, Dock.Top);
            Grid.SetRow(MainAnimeDetailDockpanel, 1);
            _window.RegisterName(MainAnimeDetailDockpanel.Name, MainAnimeDetailDockpanel);

            #region AnimeDetail

            #region AnimeDetail Top
            // create grid for animedetails
            Grid AnimeDetailGrid = new Grid();
            DockPanel.SetDock(AnimeDetailGrid, Dock.Top);
            AnimeDetailGrid.Margin = new Thickness(0, 0, 0, 5);
            AnimeDetailGrid.Height = 370;

            // create columndefenitions for grid
            ColumnDefinition C1 = new ColumnDefinition();
            C1.Width = new GridLength(285);
            ColumnDefinition C2 = new ColumnDefinition();

            // set Columndefenitions
            AnimeDetailGrid.ColumnDefinitions.Add(C1);
            AnimeDetailGrid.ColumnDefinitions.Add(C2);

            #region AnimeDetail Cover (LeftSide)
            // create Image for AnimeCover
            Image AnimeCoverImage = new Image();
            Grid.SetColumn(AnimeCoverImage, 0);
            AnimeCoverImage.HorizontalAlignment = HorizontalAlignment.Left;
            AnimeCoverImage.VerticalAlignment = VerticalAlignment.Top;
            AnimeCoverImage.Width = 275;
            AnimeCoverImage.Height = 360;
            AnimeCoverImage.Margin = new Thickness(0, 0, 5, 0);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeCoverImage.Name = "image_AnimeCover_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeCoverImage.Name = "image_AnimeCover_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeCoverImage.Name = "image_AnimeCover_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeCoverImage.Name = "image_AnimeCover_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeCoverImage.Name, AnimeCoverImage);
            #endregion

            #region AnimeDetail Details (RightSide)
            // create subgrid
            Grid AnimeDetailSubGrid = new Grid();
            Grid.SetColumn(AnimeDetailSubGrid, 1);

            // create rowdefinitions
            RowDefinition RS1 = new RowDefinition();
            RowDefinition RS2 = new RowDefinition();

            // set rowdefinitions
            AnimeDetailSubGrid.RowDefinitions.Add(RS1);
            AnimeDetailSubGrid.RowDefinitions.Add(RS2);

            #region AnimeDetail Description
            // create dockpanel for animedescription
            DockPanel AnimeDescriptionDockPanel = new DockPanel();
            Grid.SetRow(AnimeDescriptionDockPanel, 0);

            // create subdockpanel for animedescription
            DockPanel AnimeDescriptionSubDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeDescriptionSubDockPanel, Dock.Top);
            AnimeDescriptionSubDockPanel.LastChildFill = false;

            // create label
            Label AnimeDescriptionLabel = new Label();
            DockPanel.SetDock(AnimeDescriptionLabel, Dock.Left);
            AnimeDescriptionLabel.Content = languageDetail.LabelDescription;
            AnimeDescriptionLabel.Margin = new Thickness(5, 0, 5, -15);
            AnimeDescriptionLabel.Padding = new Thickness(0, 3, 3, 3);
            AnimeDescriptionLabel.Height = 26;
            AnimeDescriptionLabel.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeDescriptionLabel.Name = "label_AnimeDescription_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeDescriptionLabel.Name = "label_AnimeDescription_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeDescriptionLabel.Name = "label_AnimeDescription_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeDescriptionLabel.Name = "label_AnimeDescription_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeDescriptionLabel.Name, AnimeDescriptionLabel);

            // create combobox for change description language
            ComboBox AnimeDescriptionLanugageComboBox = new ComboBox();
            DockPanel.SetDock(AnimeDescriptionLanugageComboBox, Dock.Right);
            AnimeDescriptionLanugageComboBox.Width = 200;
            AnimeDescriptionLanugageComboBox.Height = 25;
            AnimeDescriptionLanugageComboBox.Margin = new Thickness(0, 0, 5, 5);
            AnimeDescriptionLanugageComboBox.SelectionChanged += OnSelectionChanged;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeDescriptionLanugageComboBox.Tag = "D_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeDescriptionLanugageComboBox.Tag = "D_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeDescriptionLanugageComboBox.Tag = "D_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeDescriptionLanugageComboBox.Tag = "D_" + animeCount + "_Right";
                    break;
            }

            // add elements to subdockpanel
            AnimeDescriptionSubDockPanel.Children.Add(AnimeDescriptionLabel);
            AnimeDescriptionSubDockPanel.Children.Add(AnimeDescriptionLanugageComboBox);

            // create textblock for animedescription
            TextBlock AnimeDescriptionTextBlock = new TextBlock();
            DockPanel.SetDock(AnimeDescriptionTextBlock, Dock.Top);
            Grid.SetRow(AnimeDescriptionTextBlock, 3);
            AnimeDescriptionTextBlock.TextWrapping = TextWrapping.Wrap;
            AnimeDescriptionTextBlock.Padding = new Thickness(5, 2, 0, 2);
            AnimeDescriptionTextBlock.Margin = new Thickness(5, 0, 5, 10);
            AnimeDescriptionTextBlock.Foreground = new SolidColorBrush(Colors.White);
            AnimeDescriptionTextBlock.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
            AnimeDescriptionTextBlock.MaxHeight = 320;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeDescriptionTextBlock.Name = "textblock_AnimeDescription_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeDescriptionTextBlock.Name = "textblock_AnimeDescription_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeDescriptionTextBlock.Name = "textblock_AnimeDescription_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeDescriptionTextBlock.Name = "textblock_AnimeDescription_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeDescriptionTextBlock.Name, AnimeDescriptionTextBlock);

            // set right AnimeDescription and Language and set comboboxItems
            for (int i = 0; i < _anime.Descriptions.Count; i++)
            {
                // create new ComboboxItems
                ComboBoxItem AnimeDescriptionCBI = new ComboBoxItem();

                // create new dockpanel
                DockPanel dockPanel = new DockPanel();

                // find right path
                string path = "";
                for (int j = 0; j < LanguageDetails.Count; j++)
                {
                    if (_anime.Descriptions[i].Name == LanguageDetails[j].Name)
                    {
                        path = LanguageDetails[j].PicturePath_Complete;
                    }
                }

                // create new image for icon
                Image icon = new Image()
                {
                    Width = 20,
                    Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + path))
                };
                DockPanel.SetDock(icon, Dock.Left);

                // create new label for language name
                Label text = new Label()
                {
                    Padding = new Thickness(0, -3, 0, 0),
                    Margin = new Thickness(5, 0, 0, 0),
                    Content = _anime.Descriptions[i].Name,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                DockPanel.SetDock(text, Dock.Left);

                // add icon and text to dockpanel
                dockPanel.Children.Add(icon);
                dockPanel.Children.Add(text);

                // add dockpanel to comboboxitem
                AnimeDescriptionCBI.Content = dockPanel;

                // add comboboxItem to Combobox
                AnimeDescriptionLanugageComboBox.Items.Add(AnimeDescriptionCBI);

                if (_anime.Descriptions[i].Name == Setting)
                {
                    AnimeDescriptionLanugageComboBox.SelectedIndex = i;
                    AnimeDescriptionTextBlock.Text = _anime.Descriptions[i].Value;
                }
                // last iteration and no name in the setting-language found
                if (i == _anime.Descriptions.Count - 1 && AnimeDescriptionTextBlock.Text == "")
                {
                    // set first name and language of the list
                    AnimeDescriptionLanugageComboBox.SelectedIndex = 0;
                    AnimeDescriptionTextBlock.Text = _anime.Descriptions[0].Value;
                }
            }

            // add elements to AnimeDetailDescriptionDockPanel
            AnimeDescriptionDockPanel.Children.Add(AnimeDescriptionSubDockPanel);
            AnimeDescriptionDockPanel.Children.Add(AnimeDescriptionTextBlock);
            #endregion

            #region AnimeDetail Details
            // Dockpanel for details
            DockPanel AnimeDetailsDockPanel = new DockPanel();
            Grid.SetRow(AnimeDetailsDockPanel, 1);
            AnimeDetailsDockPanel.LastChildFill = false;

            #region AnimeDetail Duration+EpisodeCount
            // create dockpanel for duration and episodecount
            DockPanel AnimeDurationEpisodeCountDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeDurationEpisodeCountDockPanel, Dock.Top);
            AnimeDurationEpisodeCountDockPanel.LastChildFill = false;
            AnimeDurationEpisodeCountDockPanel.Height = 30;

            // create label for Duration
            Label AnimeDurationLabel = new Label();
            DockPanel.SetDock(AnimeDurationLabel, Dock.Left);
            AnimeDurationLabel.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeDurationLabel.Name = "label_AnimeDuration_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeDurationLabel.Name = "label_AnimeDuration_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeDurationLabel.Name = "label_AnimeDuration_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeDurationLabel.Name = "label_AnimeDuration_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeDurationLabel.Name, AnimeDurationLabel);

            // strings for duration
            string hour;
            string minute;
            string second;
            // if hours are less than 10
            if (_animeList[animeCount].Duration_Hours < 10)
            {
                // add "0" before duration
                hour = "0" + _animeList[animeCount].Duration_Hours.ToString();
            }
            else
            {
                hour = _animeList[animeCount].Duration_Hours.ToString();
            }
            // if minutes are less than
            if (_animeList[animeCount].Duration_Minutes < 10)
            {
                // add "0" before duration
                minute = "0" + _animeList[animeCount].Duration_Minutes.ToString();
            }
            else
            {
                minute = _animeList[animeCount].Duration_Minutes.ToString();
            }
            // if seconds are less than 10
            if (_animeList[animeCount].Duration_Seconds < 10)
            {
                // add "0" before duration
                second = "0" + _animeList[animeCount].Duration_Seconds.ToString();
            }
            else
            {
                second = _animeList[animeCount].Duration_Seconds.ToString();
            }

            AnimeDurationLabel.Content = languageDetail.LabelDuration + " " + hour + ":" + minute + ":" + second;

            // create label for episodecount
            Label AnimeEpisodeCountLabel = new Label();
            DockPanel.SetDock(AnimeEpisodeCountLabel, Dock.Left);
            AnimeEpisodeCountLabel.Foreground = new SolidColorBrush(Colors.White);

            AnimeEpisodeCountLabel.Content = languageDetail.LabelEpisodeCount + " " + _animeList[animeCount].EpisodeCount;
            AnimeEpisodeCountLabel.Margin = new Thickness(50, 0, 0, 0);
            AnimeEpisodeCountLabel.HorizontalAlignment = HorizontalAlignment.Center;
            AnimeEpisodeCountLabel.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeEpisodeCountLabel.Name = "label_EpisodeCount_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeEpisodeCountLabel.Name = "label_EpisodeCount_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeEpisodeCountLabel.Name = "label_EpisodeCount_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeEpisodeCountLabel.Name = "label_EpisodeCount_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeEpisodeCountLabel.Name, AnimeEpisodeCountLabel);

            // add labels to dockpanel
            AnimeDurationEpisodeCountDockPanel.Children.Add(AnimeDurationLabel);
            AnimeDurationEpisodeCountDockPanel.Children.Add(AnimeEpisodeCountLabel);
            #endregion

            #region AnimeDetail Season
            // create dockpanel for Seasoms
            DockPanel AnimeSeasonsDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeSeasonsDockPanel, Dock.Top);
            AnimeSeasonsDockPanel.LastChildFill = false;
            AnimeSeasonsDockPanel.Height = 30;

            // create label for season
            Label AnimeSeasonLabel = new Label();
            DockPanel.SetDock(AnimeSeasonLabel, Dock.Left);
            AnimeSeasonLabel.Foreground = new SolidColorBrush(Colors.White);
            AnimeSeasonLabel.Content = languageDetail.LabelSeason;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeSeasonLabel.Name = "label_AnimeSeason_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeSeasonLabel.Name = "label_AnimeSeason_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeSeasonLabel.Name = "label_AnimeSeason_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeSeasonLabel.Name = "label_AnimeSeason_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeSeasonLabel.Name, AnimeSeasonLabel);

            // add label to dockpanel
            AnimeSeasonsDockPanel.Children.Add(AnimeSeasonLabel);

            // create buttons for each season
            for (int i = 0; i < _animeList[animeCount].Seasons.Count; i++)
            {
                // create Button
                Button SeasonButton = new Button();
                DockPanel.SetDock(SeasonButton, Dock.Left);
                SeasonButton.Content = _animeList[animeCount].Seasons[i].Name;
                SeasonButton.MinHeight = 23;
                SeasonButton.MinWidth = 25;
                SeasonButton.Padding = new Thickness(5, 0, 5, 0);
                SeasonButton.Margin = new Thickness(0, 0, 5, 0);
                SeasonButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D));
                SeasonButton.BorderBrush = SeasonButton.Background;
                SeasonButton.Foreground = new SolidColorBrush(Colors.White);
                SeasonButton.Click += OpenSeason;
                switch (_listTyp)
                {
                    case ListTyp.MAINWINDOW:
                        SeasonButton.Tag = animeCount + "_" + i;
                        break;
                    case ListTyp.MERGELEFT:
                        SeasonButton.Tag = animeCount + "_" + i + "_Left";
                        break;
                    case ListTyp.MERGENEW:
                        SeasonButton.Tag = animeCount + "_" + i + "_New";
                        break;
                    case ListTyp.MERGERIGHT:
                        SeasonButton.Tag = animeCount + "_" + i + "_Right";
                        break;
                }
                SeasonButton.Style = Resources["ButtonStyleOwn"] as Style;

                AnimeSeasonsDockPanel.Children.Add(SeasonButton);
            }
            #endregion

            #region AnimeDetail Genres
            // create dockpanel for genres
            DockPanel AnimeGenresDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeGenresDockPanel, Dock.Top);
            AnimeGenresDockPanel.LastChildFill = false;
            AnimeGenresDockPanel.Height = 30;

            // create label
            Label AnimeGenresLabel = new Label();
            DockPanel.SetDock(AnimeGenresLabel, Dock.Left);
            AnimeGenresLabel.Foreground = new SolidColorBrush(Colors.White);
            AnimeGenresLabel.Content = languageDetail.LabelGenre;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeGenresLabel.Name = "label_AnimeGenres_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeGenresLabel.Name = "label_AnimeGenres_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    AnimeGenresLabel.Name = "label_AnimeGenres_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeGenresLabel.Name = "label_AnimeGenres_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(AnimeGenresLabel.Name, AnimeGenresLabel);

            // add label to dockpanel
            AnimeGenresDockPanel.Children.Add(AnimeGenresLabel);

            // create label for each MainGenres
            for (int i = 0; i < _animeList[animeCount].MainGenres.Count; i++)
            {
                Label AnimeMainGenre = new Label();
                DockPanel.SetDock(AnimeMainGenre, Dock.Left);
                AnimeMainGenre.Content = _animeList[animeCount].MainGenres[i];
                AnimeMainGenre.MinWidth = 25;
                AnimeMainGenre.Height = 25;
                AnimeMainGenre.Padding = new Thickness(10, 4, 10, 4);
                AnimeMainGenre.Margin = new Thickness(0, 0, 5, 0);
                AnimeMainGenre.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x2D));
                AnimeMainGenre.Foreground = new SolidColorBrush(Colors.White);
                AnimeMainGenre.BorderThickness = new Thickness(1, 1, 1, 1);
                AnimeMainGenre.BorderBrush = new SolidColorBrush(Colors.OrangeRed);

                // add label to dockpanel
                AnimeGenresDockPanel.Children.Add(AnimeMainGenre);
            }
            // create label for each SubGenres
            for (int i = 0; i < _animeList[animeCount].SubGenres.Count; i++)
            {
                Label AnimeSubGenre = new Label();
                DockPanel.SetDock(AnimeSubGenre, Dock.Left);
                AnimeSubGenre.Content = _animeList[animeCount].SubGenres[i];
                AnimeSubGenre.MinWidth = 25;
                AnimeSubGenre.Height = 25;
                AnimeSubGenre.Padding = new Thickness(10, 4, 10, 4);
                AnimeSubGenre.Margin = new Thickness(0, 0, 5, 0);
                AnimeSubGenre.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x2D));
                AnimeSubGenre.Foreground = new SolidColorBrush(Colors.White);
                AnimeSubGenre.BorderThickness = new Thickness(1, 1, 1, 1);
                AnimeSubGenre.BorderBrush = new SolidColorBrush(Colors.DarkOrange);

                // add label to dockpanel
                AnimeGenresDockPanel.Children.Add(AnimeSubGenre);
            }

            #endregion

            #region AnimeDetail Dub
            // create dockpanel for dub
            DockPanel AnimeDubDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeDubDockPanel, Dock.Top);
            AnimeDubDockPanel.LastChildFill = false;
            AnimeDubDockPanel.Height = 50;

            // create label
            Label AnimeDubLabel = new Label();
            DockPanel.SetDock(AnimeDubLabel, Dock.Left);
            AnimeDubLabel.VerticalAlignment = VerticalAlignment.Center;
            AnimeDubLabel.Foreground = new SolidColorBrush(Colors.White);
            AnimeDubLabel.Content = "Dub:";

            // create dockpanel for falgs and lanugagenames
            DockPanel AnimeFlagsDubLanguagenameDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeFlagsDubLanguagenameDockPanel, Dock.Left);
            AnimeFlagsDubLanguagenameDockPanel.LastChildFill = false;
            AnimeFlagsDubLanguagenameDockPanel.Height = 50;

            // add image and label for each dub
            for (int i = 0; i < _animeList[animeCount].Dubs.Count; i++)
            {
                // create subdockpanel
                DockPanel AnimeDubSubDockPanel = new DockPanel();
                DockPanel.SetDock(AnimeDubSubDockPanel, Dock.Left);
                AnimeDubSubDockPanel.LastChildFill = false;
                AnimeDubSubDockPanel.Margin = new Thickness(5, 5, 0, 0);

                // create image
                Image AnimeDubImage = new Image();
                DockPanel.SetDock(AnimeDubImage, Dock.Top);
                AnimeDubImage.Height = 25;
                if (_animeList[animeCount].Dubs[i].Value == "Complete")
                {
                    for (int j = 0; j < LanguageDetails.Count; j++)
                    {
                        if (_animeList[animeCount].Dubs[i].Name == LanguageDetails[j].Name)
                        {
                            AnimeDubImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + LanguageDetails[j].PicturePath_Complete));
                        }
                    }
                }
                else if (_animeList[animeCount].Dubs[i].Value == "Part")
                {
                    for (int j = 0; j < LanguageDetails.Count; j++)
                    {
                        if (_animeList[animeCount].Dubs[i].Name == LanguageDetails[j].Name)
                        {
                            AnimeDubImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + LanguageDetails[j].PicturePath_Part));
                        }
                    }
                }

                // create labe for language name
                Label AnimeDubLanguagenameLabel = new Label();
                DockPanel.SetDock(AnimeDubLanguagenameLabel, Dock.Top);
                AnimeDubLanguagenameLabel.Content = _animeList[animeCount].Dubs[i].Name;
                AnimeDubLanguagenameLabel.Padding = new Thickness(5, 0, 5, 0);
                AnimeDubLanguagenameLabel.Foreground = new SolidColorBrush(Colors.White);

                // add elements to dockpanel
                AnimeDubSubDockPanel.Children.Add(AnimeDubImage);
                AnimeDubSubDockPanel.Children.Add(AnimeDubLanguagenameLabel);

                // add dockpanel to dockpanel
                AnimeFlagsDubLanguagenameDockPanel.Children.Add(AnimeDubSubDockPanel);
            }

            // add elements to dockpanel
            AnimeDubDockPanel.Children.Add(AnimeDubLabel);
            AnimeDubDockPanel.Children.Add(AnimeFlagsDubLanguagenameDockPanel);
            #endregion

            #region AnimeDetail Sub
            // create dockpanel for dub
            DockPanel AnimeSubDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeSubDockPanel, Dock.Top);
            AnimeSubDockPanel.LastChildFill = false;
            AnimeSubDockPanel.Height = 50;

            // create label
            Label AnimeSubLabel = new Label();
            DockPanel.SetDock(AnimeSubLabel, Dock.Left);
            AnimeSubLabel.VerticalAlignment = VerticalAlignment.Center;
            AnimeSubLabel.Foreground = new SolidColorBrush(Colors.White);
            AnimeSubLabel.Content = "Sub:";

            // create dockpanel for falgs and lanugagenames
            DockPanel AnimeFlagsSubLanguagenameDockPanel = new DockPanel();
            DockPanel.SetDock(AnimeFlagsSubLanguagenameDockPanel, Dock.Left);
            AnimeFlagsSubLanguagenameDockPanel.LastChildFill = false;
            AnimeFlagsSubLanguagenameDockPanel.Height = 50;

            // add image and label for each dub
            for (int i = 0; i < _animeList[animeCount].Subs.Count; i++)
            {
                // create subdockpanel
                DockPanel AnimeSubSubDockPanel = new DockPanel();
                DockPanel.SetDock(AnimeSubSubDockPanel, Dock.Left);
                AnimeSubSubDockPanel.LastChildFill = false;
                AnimeSubSubDockPanel.Margin = new Thickness(5, 5, 0, 0);

                // create image
                Image AnimeSubImage = new Image();
                DockPanel.SetDock(AnimeSubImage, Dock.Top);
                AnimeSubImage.Height = 25;
                if (_animeList[animeCount].Subs[i].Value == "Complete")
                {
                    for (int j = 0; j < LanguageDetails.Count; j++)
                    {
                        if (_animeList[animeCount].Subs[i].Name == LanguageDetails[j].Name)
                        {
                            AnimeSubImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + LanguageDetails[j].PicturePath_Complete));
                        }
                    }
                }
                else if (_animeList[animeCount].Subs[i].Value == "Part")
                {
                    for (int j = 0; j < LanguageDetails.Count; j++)
                    {
                        if (_animeList[animeCount].Subs[i].Name == LanguageDetails[j].Name)
                        {
                            AnimeSubImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + LanguageDetails[j].PicturePath_Part));
                        }
                    }
                }

                // create labe for language name
                Label AnimeSubLanguagenameLabel = new Label();
                DockPanel.SetDock(AnimeSubLanguagenameLabel, Dock.Top);
                AnimeSubLanguagenameLabel.Content = _animeList[animeCount].Subs[i].Name;
                AnimeSubLanguagenameLabel.Padding = new Thickness(5, 0, 5, 0);
                AnimeSubLanguagenameLabel.Foreground = new SolidColorBrush(Colors.White);

                // add elements to dockpanel
                AnimeSubSubDockPanel.Children.Add(AnimeSubImage);
                AnimeSubSubDockPanel.Children.Add(AnimeSubLanguagenameLabel);

                // add dockpanel to dockpanel
                AnimeFlagsSubLanguagenameDockPanel.Children.Add(AnimeSubSubDockPanel);
            }

            // add elements to dockpanel
            AnimeSubDockPanel.Children.Add(AnimeSubLabel);
            AnimeSubDockPanel.Children.Add(AnimeFlagsSubLanguagenameDockPanel);
            #endregion

            AnimeDetailsDockPanel.Children.Add(AnimeDurationEpisodeCountDockPanel);
            AnimeDetailsDockPanel.Children.Add(AnimeSeasonsDockPanel);
            AnimeDetailsDockPanel.Children.Add(AnimeGenresDockPanel);
            AnimeDetailsDockPanel.Children.Add(AnimeDubDockPanel);
            AnimeDetailsDockPanel.Children.Add(AnimeSubDockPanel);

            #endregion

            // add elements to AnimeDetailSubGrid
            AnimeDetailSubGrid.Children.Add(AnimeDescriptionDockPanel);
            AnimeDetailSubGrid.Children.Add(AnimeDetailsDockPanel);

            #endregion

            // add elements to animedetailgrid
            AnimeDetailGrid.Children.Add(AnimeCoverImage);
            AnimeDetailGrid.Children.Add(AnimeDetailSubGrid);

            #endregion

            #region AnimeDetail Bottom
            // create itemscontrol
            ItemsControl itemsControl = new ItemsControl();
            itemsControl.Name = "itemscontrol_" + animeCount;
            DockPanel.SetDock(itemsControl, Dock.Top);

            // set wrappanel as Template for ItemsControl
            FrameworkElementFactory fef = new FrameworkElementFactory(typeof(WrapPanel));
            ItemsPanelTemplate ipTemplate = new ItemsPanelTemplate().SetVisualTree(fef);
            itemsControl.ItemsPanel = ipTemplate;

            // create new binding for itemscontrol
            Binding binding = new Binding();
            binding.ElementName = "mainWindow";
            binding.Path = new PropertyPath(Window.WidthProperty);
            binding.Converter = new WidthConverter();

            // set binding to itemscontrol
            itemsControl.SetBinding(ItemsControl.WidthProperty, binding);

            #region AnimeDetail Bottom Productionyear
            // create dockpanel for productionyear
            DockPanel dockpanelProductionyear = new DockPanel();
            dockpanelProductionyear.Margin = new Thickness(0, 0, 20, 10);
            dockpanelProductionyear.LastChildFill = false;

            // create label for productionyear
            Label labelProductionyear = new Label();
            labelProductionyear.Content = languageDetail.LabelProductionyears;
            labelProductionyear.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    labelProductionyear.Name = "label_Productionyear_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    labelProductionyear.Name = "label_Productionyear_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    labelProductionyear.Name = "label_Productionyear_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    labelProductionyear.Name = "label_Productionyear_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(labelProductionyear.Name, labelProductionyear);

            // check produktionyear values
            string year1 = languageDetail.Unknown;
            string year2 = languageDetail.Unknown;

            if (_anime.Productionyear[0] != 0)
            {
                year1 = _anime.Productionyear[0].ToString();
            }
            if (_anime.Productionyear[1] != 0)
            {
                year2 = _anime.Productionyear[1].ToString();
            }

            // create label for productionyear values
            Label labelProductionyearValues = new Label();

            labelProductionyearValues.Content = year1 + " - " + year2;
            labelProductionyearValues.Foreground = new SolidColorBrush(Colors.White);
            labelProductionyearValues.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
            labelProductionyearValues.Padding = new Thickness(10, 4, 10, 4);
            labelProductionyearValues.Margin = new Thickness(0, 0, 0, 5);
            labelProductionyearValues.VerticalAlignment = VerticalAlignment.Top;
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    labelProductionyearValues.Name = "label_ProductionyearValues_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    labelProductionyearValues.Name = "label_ProductionyearValues_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    labelProductionyearValues.Name = "label_ProductionyearValues_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    labelProductionyearValues.Name = "label_ProductionyearValues_" + animeCount + "_Right";
                    break;
            }
            DockPanel.SetDock(labelProductionyearValues, Dock.Left);
            _window.RegisterName(labelProductionyearValues.Name, labelProductionyearValues);

            // add elements to dockpanelproduktionyear
            dockpanelProductionyear.Children.Add(labelProductionyear);
            dockpanelProductionyear.Children.Add(labelProductionyearValues);
            #endregion

            #region AnimeDetail Bottom MainActor
            // create dockpanel for MainActor
            DockPanel dockpanelMainActor = new DockPanel();
            dockpanelMainActor.Margin = new Thickness(0, 0, 20, 10);
            dockpanelMainActor.LastChildFill = false;

            // create label for MainActor
            Label labelMainActor = new Label();
            labelMainActor.Content = languageDetail.LabelMainActors;
            labelMainActor.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    labelMainActor.Name = "label_MainActor_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    labelMainActor.Name = "label_MainActor_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    labelMainActor.Name = "label_MainActor_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    labelMainActor.Name = "label_MainActor_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(labelMainActor.Name, labelMainActor);

            // create label for MainActor values
            DockPanel dockpanelMainActorValues = new DockPanel();
            dockpanelMainActorValues.LastChildFill = false;
            DockPanel.SetDock(dockpanelMainActorValues, Dock.Left);

            // if no MainActor is added
            if (_anime.MainActors.Count == 0)
            {
                // add default label
                Label labelMainActorValue = new Label();
                labelMainActorValue.Content = languageDetail.NoInformation;
                labelMainActorValue.Foreground = new SolidColorBrush(Colors.White);
                labelMainActorValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                labelMainActorValue.Padding = new Thickness(10, 4, 10, 4);
                labelMainActorValue.Margin = new Thickness(0, 0, 0, 5);
                labelMainActorValue.VerticalAlignment = VerticalAlignment.Top;
                switch (_listTyp)
                {
                    case ListTyp.MAINWINDOW:
                        labelMainActorValue.Name = "label_NoMainActors_" + animeCount;
                        break;
                    case ListTyp.MERGELEFT:
                        labelMainActorValue.Name = "label_NoMainActors_" + animeCount + "_Left";
                        break;
                    case ListTyp.MERGENEW:
                        labelMainActorValue.Name = "label_NoMainActors_" + animeCount + "_New";
                        break;
                    case ListTyp.MERGERIGHT:
                        labelMainActorValue.Name = "label_NoMainActors_" + animeCount + "_Right";
                        break;
                }
                DockPanel.SetDock(labelMainActorValue, Dock.Top);
                _window.RegisterName(labelMainActorValue.Name, labelMainActorValue);

                dockpanelMainActorValues.Children.Add(labelMainActorValue);
            }
            else
            {
                // otherwise go through the MainActorslist
                for (int i = 0; i < _anime.MainActors.Count; i++)
                {
                    // create label with right content and add it to the dockpanelMainActorValues
                    Label labelMainActorValue = new Label();
                    labelMainActorValue.Content = _anime.MainActors[i];
                    labelMainActorValue.Foreground = new SolidColorBrush(Colors.White);
                    labelMainActorValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                    labelMainActorValue.Padding = new Thickness(10, 4, 10, 4);
                    labelMainActorValue.Margin = new Thickness(0, 0, 0, 5);
                    labelMainActorValue.VerticalAlignment = VerticalAlignment.Top;
                    DockPanel.SetDock(labelMainActorValue, Dock.Top);

                    dockpanelMainActorValues.Children.Add(labelMainActorValue);
                }
            }

            // add elements to MainActor
            dockpanelMainActor.Children.Add(labelMainActor);
            dockpanelMainActor.Children.Add(dockpanelMainActorValues);
            #endregion

            #region AnimeDetail Bottom Producer
            // create dockpanel for Producer
            DockPanel dockpanelProducer = new DockPanel();
            dockpanelProducer.Margin = new Thickness(0, 0, 20, 10);
            dockpanelProducer.LastChildFill = false;

            // create label for Producer
            Label labelProducer = new Label();
            labelProducer.Content = languageDetail.LabelProducers;
            labelProducer.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    labelProducer.Name = "label_Producer_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    labelProducer.Name = "label_Producer_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    labelProducer.Name = "label_Producer_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    labelProducer.Name = "label_Producer_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(labelProducer.Name, labelProducer);

            // create label for Producer values
            DockPanel dockpanelProducerValues = new DockPanel();
            DockPanel.SetDock(dockpanelProducerValues, Dock.Left);

            // if no Producer is added
            if (_anime.Producers.Count == 0)
            {
                // add default label
                Label labelProducerValue = new Label();
                labelProducerValue.Content = languageDetail.NoInformation;
                labelProducerValue.Foreground = new SolidColorBrush(Colors.White);
                labelProducerValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                labelProducerValue.Padding = new Thickness(10, 4, 10, 4);
                labelProducerValue.Margin = new Thickness(0, 0, 0, 5);
                labelProducerValue.VerticalAlignment = VerticalAlignment.Top;
                switch (_listTyp)
                {
                    case ListTyp.MAINWINDOW:
                        labelProducerValue.Name = "label_NoProducers_" + animeCount;
                        break;
                    case ListTyp.MERGELEFT:
                        labelProducerValue.Name = "label_NoProducers_" + animeCount + "_Left";
                        break;
                    case ListTyp.MERGENEW:
                        labelProducerValue.Name = "label_NoProducers_" + animeCount + "_New";
                        break;
                    case ListTyp.MERGERIGHT:
                        labelProducerValue.Name = "label_NoProducers_" + animeCount + "_Right";
                        break;
                }
                DockPanel.SetDock(labelProducerValue, Dock.Top);
                _window.RegisterName(labelProducerValue.Name, labelProducerValue);

                dockpanelProducerValues.Children.Add(labelProducerValue);
            }
            else
            {
                // otherwise go through the Producerslist
                for (int i = 0; i < _anime.Producers.Count; i++)
                {
                    // create label with right content and add it to the dockpanelProducerValues
                    Label labelProducerValue = new Label();
                    labelProducerValue.Content = _anime.Producers[i];
                    labelProducerValue.Foreground = new SolidColorBrush(Colors.White);
                    labelProducerValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                    labelProducerValue.Padding = new Thickness(10, 4, 10, 4);
                    labelProducerValue.Margin = new Thickness(0, 0, 0, 5);
                    labelProducerValue.VerticalAlignment = VerticalAlignment.Top;
                    DockPanel.SetDock(labelProducerValue, Dock.Top);
                    dockpanelProducerValues.Children.Add(labelProducerValue);
                }
            }

            // add elements to dockpanelProducer
            dockpanelProducer.Children.Add(labelProducer);
            dockpanelProducer.Children.Add(dockpanelProducerValues);
            #endregion

            #region AnimeDetail Bottom Director
            // create dockpanel for Director
            DockPanel dockpanelDirector = new DockPanel();
            dockpanelDirector.Margin = new Thickness(0, 0, 20, 10);
            dockpanelDirector.LastChildFill = false;

            // create label for Director
            Label labelDirector = new Label();
            labelDirector.Content = languageDetail.LabelDirectors;
            labelDirector.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    labelDirector.Name = "label_Director_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    labelDirector.Name = "label_Director_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    labelDirector.Name = "label_Director_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    labelDirector.Name = "label_Director_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(labelDirector.Name, labelDirector);

            // create label for Director values
            DockPanel dockpanelDirectorValues = new DockPanel();
            DockPanel.SetDock(dockpanelDirectorValues, Dock.Left);

            // if no Director is added
            if (_anime.Directors.Count == 0)
            {
                // add default label
                Label labelDirectorValue = new Label();
                labelDirectorValue.Content = languageDetail.NoInformation;
                labelDirectorValue.Foreground = new SolidColorBrush(Colors.White);
                labelDirectorValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                labelDirectorValue.Padding = new Thickness(10, 4, 10, 4);
                labelDirectorValue.Margin = new Thickness(0, 0, 0, 5);
                labelDirectorValue.VerticalAlignment = VerticalAlignment.Top;
                switch (_listTyp)
                {
                    case ListTyp.MAINWINDOW:
                        labelDirectorValue.Name = "label_NoDirectors_" + animeCount;
                        break;
                    case ListTyp.MERGELEFT:
                        labelDirectorValue.Name = "label_NoDirectors_" + animeCount + "_Left";
                        break;
                    case ListTyp.MERGENEW:
                        labelDirectorValue.Name = "label_NoDirectors_" + animeCount + "_New";
                        break;
                    case ListTyp.MERGERIGHT:
                        labelDirectorValue.Name = "label_NoDirectors_" + animeCount + "_Right";
                        break;
                }
                DockPanel.SetDock(labelDirectorValue, Dock.Top);
                _window.RegisterName(labelDirectorValue.Name, labelDirectorValue);

                dockpanelDirectorValues.Children.Add(labelDirectorValue);
            }
            else
            {
                // otherwise go through the Directorslist
                for (int i = 0; i < _anime.Directors.Count; i++)
                {
                    // create label with right content and add it to the dockpanelDirectorValues
                    Label labelDirectorValue = new Label();
                    labelDirectorValue.Content = _anime.Directors[i];
                    labelDirectorValue.Foreground = new SolidColorBrush(Colors.White);
                    labelDirectorValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                    labelDirectorValue.Padding = new Thickness(10, 4, 10, 4);
                    labelDirectorValue.Margin = new Thickness(0, 0, 0, 5);
                    labelDirectorValue.VerticalAlignment = VerticalAlignment.Top;
                    DockPanel.SetDock(labelDirectorValue, Dock.Top);
                    dockpanelDirectorValues.Children.Add(labelDirectorValue);
                }
            }

            // add elements to dockpanelDirector
            dockpanelDirector.Children.Add(labelDirector);
            dockpanelDirector.Children.Add(dockpanelDirectorValues);
            #endregion

            #region AnimeDetail Bottom Author
            // create dockpanel for Author
            DockPanel dockpanelAuthor = new DockPanel();
            dockpanelAuthor.Margin = new Thickness(0, 0, 20, 10);
            dockpanelAuthor.LastChildFill = false;

            // create label for Author
            Label labelAuthor = new Label();
            labelAuthor.Content = languageDetail.LabelAuthors;
            labelAuthor.Foreground = new SolidColorBrush(Colors.White);
            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    labelAuthor.Name = "label_Author_" + animeCount;
                    break;
                case ListTyp.MERGELEFT:
                    labelAuthor.Name = "label_Author_" + animeCount + "_Left";
                    break;
                case ListTyp.MERGENEW:
                    labelAuthor.Name = "label_Author_" + animeCount + "_New";
                    break;
                case ListTyp.MERGERIGHT:
                    labelAuthor.Name = "label_Author_" + animeCount + "_Right";
                    break;
            }
            _window.RegisterName(labelAuthor.Name, labelAuthor);

            // create label for Author values
            DockPanel dockpanelAuthorValues = new DockPanel();
            DockPanel.SetDock(dockpanelAuthorValues, Dock.Left);

            // if no Author is added
            if (_anime.Directors.Count == 0)
            {
                // add default label
                Label labelAuthorValue = new Label();
                labelAuthorValue.Content = languageDetail.NoInformation;
                labelAuthorValue.Foreground = new SolidColorBrush(Colors.White);
                labelAuthorValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                labelAuthorValue.Padding = new Thickness(10, 4, 10, 4);
                labelAuthorValue.Margin = new Thickness(0, 0, 0, 5);
                labelAuthorValue.VerticalAlignment = VerticalAlignment.Top;
                switch (_listTyp)
                {
                    case ListTyp.MAINWINDOW:
                        labelAuthorValue.Name = "label_NoAuthors_" + animeCount;
                        break;
                    case ListTyp.MERGELEFT:
                        labelAuthorValue.Name = "label_NoAuthors_" + animeCount + "_Left";
                        break;
                    case ListTyp.MERGENEW:
                        labelAuthorValue.Name = "label_NoAuthors_" + animeCount + "_New";
                        break;
                    case ListTyp.MERGERIGHT:
                        labelAuthorValue.Name = "label_NoAuthors_" + animeCount + "_Right";
                        break;
                }
                DockPanel.SetDock(labelAuthorValue, Dock.Top);
                _window.RegisterName(labelAuthorValue.Name, labelAuthorValue);

                dockpanelAuthorValues.Children.Add(labelAuthorValue);
            }
            else
            {
                // otherwise go through the Authorslist
                for (int i = 0; i < _anime.Authors.Count; i++)
                {
                    // create label with right content and add it to the dockpanelAuthorValues
                    Label labelAuthorValue = new Label();
                    labelAuthorValue.Content = _anime.Authors[i];
                    labelAuthorValue.Foreground = new SolidColorBrush(Colors.White);
                    labelAuthorValue.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
                    labelAuthorValue.Padding = new Thickness(10, 4, 10, 4);
                    labelAuthorValue.Margin = new Thickness(0, 0, 0, 5);
                    labelAuthorValue.VerticalAlignment = VerticalAlignment.Top;
                    DockPanel.SetDock(labelAuthorValue, Dock.Top);
                    dockpanelAuthorValues.Children.Add(labelAuthorValue);
                }
            }

            // add elements to MainActor
            dockpanelAuthor.Children.Add(labelAuthor);
            dockpanelAuthor.Children.Add(dockpanelAuthorValues);
            #endregion

            // add elements to ItemsControl
            itemsControl.Items.Add(dockpanelProductionyear);
            itemsControl.Items.Add(dockpanelMainActor);
            itemsControl.Items.Add(dockpanelProducer);
            itemsControl.Items.Add(dockpanelDirector);
            itemsControl.Items.Add(dockpanelAuthor);

            if (_editButton)
            {
                EditAnime EA = new EditAnime
                {
                    Anime = _anime,
                    Index = animeCount,
                    Window = _window,
                    AnimeList = _animeList,
                    ListTyp = _listTyp
                };

                Button editButton = new Button()
                {
                    MinWidth = 90,
                    Height = 20,
                    Background = new SolidColorBrush(Colors.Green),
                    Foreground = new SolidColorBrush(Colors.Wheat),
                    BorderThickness = new Thickness(0),
                    Tag = EA
                };
                editButton.Click += OpenAddWindow;

                DockPanel dp = new DockPanel();

                dp.Children.Add(editButton);

                itemsControl.Items.Add(dp);
            }

            #endregion

            #endregion

            // add elements to MainAnimeDetailDockpanel
            MainAnimeDetailDockpanel.Children.Add(AnimeDetailGrid);
            MainAnimeDetailDockpanel.Children.Add(itemsControl);

            #endregion

            // add dockpanel to maingrid
            MainGrid.Children.Add(AnimeNameDockPanel);
            MainGrid.Children.Add(MainAnimeDetailDockpanel);

            // set visibility
            MainAnimeDetailDockpanel.Visibility = Visibility.Collapsed;

            DockPanel AnimeListDockPanel = null;

            switch (_listTyp)
            {
                case ListTyp.MAINWINDOW:
                    AnimeListDockPanel = (DockPanel)this.FindName("dockpanel_AnimeList");
                    // actiave saving animelist
                    saveAnimeList.IsEnabled = true;
                    break;
                case ListTyp.MERGELEFT:
                    AnimeListDockPanel = (DockPanel)mergeWindow.FindName("dockpanel_AnimeList" + "_Left");
                    break;
                case ListTyp.MERGENEW:
                    AnimeListDockPanel = (DockPanel)mergeWindow.FindName("dockpanel_AnimeList" + "_New");
                    break;
                case ListTyp.MERGERIGHT:
                    AnimeListDockPanel = (DockPanel)mergeWindow.FindName("dockpanel_AnimeList" + "_Right");
                    break;
            }

            // add maingrid to animelist
            AnimeListDockPanel.Children.Add(MainGrid);

        }

        private void OpenAddWindow(object sender, RoutedEventArgs e)
        {
            // get Anime and informations
            EditAnime ea = (EditAnime)((Button)sender).Tag;

            // create new addwindow
            AddWindow addWindow = new AddWindow();

            // set mainwindow reference
            addWindow.MainWindow = this;

            // set right language
            addWindow.SetLanguage(languageDetail);

            // fill genrechecklist in the add-window
            addWindow.FillGenreChecklist();

            // set animecount
            addWindow.SetAnimecount(ea.Index);

            // set Anime and informations referenc
            addWindow.EA = ea;

            // set Values ind add-window
            addWindow.FillValues(ea.Anime);

            // fill combobox of languages
            addWindow.FillLanguageCombobox("combobox_AnimeNameDescription");

            // open window
            addWindow.ShowDialog();
        }

        private void MenuItem_Add(object sender, RoutedEventArgs e)
        {
            // create new window
            AddWindow addWindow = new AddWindow();

            // set mainWindow reference
            addWindow.MainWindow = this;

            // set right language
            addWindow.SetLanguage(languageDetail);

            // find addSeason Label
            Label addSeason = (Label)addWindow.FindName("label_AddSeason");

            // add first season
            addWindow.AddSeason(addSeason, new RoutedEventArgs());

            // fill genrechecklist in the add-window
            addWindow.FillGenreChecklist();

            // fill combobox of Languages
            addWindow.FillLanguageCombobox("combobox_AnimeNameDescription");

            // set Animecount
            addWindow.SetAnimecount(Animes.Count);

            // show add window
            addWindow.ShowDialog();
        }

        public void MenuItem_OpenSaveLoadWindow(object sender, RoutedEventArgs e)
        {
            // create new window
            SaveLoadAnimeListWindow SaveLoadWindow = new SaveLoadAnimeListWindow();

            // string that contains keyword for listtyp
            string[] tmp;

            try
            {
                tmp = ((MenuItem)sender).Tag.ToString().Split("_");
            }
            catch
            {
                tmp = ((Button)sender).Tag.ToString().Split("_");
            }

            // listtyp with default setting
            ListTyp typ = ListTyp.MAINWINDOW;
            SaveLoadWindow.window = this;

            // if mergerwindow opend saveload-window set right listtyp and windowreference
            if (tmp.Length > 1)
            {
                switch (tmp[1])
                {
                    case "left":
                        typ = ListTyp.MERGELEFT;
                        SaveLoadWindow.window = mergeWindow;
                        break;
                    case "new":
                        typ = ListTyp.MERGENEW;
                        SaveLoadWindow.window = mergeWindow;
                        break;
                    case "right":
                        typ = ListTyp.MERGERIGHT;
                        SaveLoadWindow.window = mergeWindow;
                        break;
                }
            }

            // set windowtyp
            SaveLoadWindow.LoadSave = int.Parse(tmp[0]);

            // set mainwindow reference
            SaveLoadWindow.mainWindow = this;

            // set language
            SaveLoadWindow.SetLanguage(languageDetail);

            // get saved files
            SaveLoadWindow.GetSavedFiles(languageDetail, typ);

            // show window
            SaveLoadWindow.ShowDialog();
        }

        private void CreateDefaultLanguagesDetailData(string _filePath)
        {
            LanguageDetail languageDetail = new LanguageDetail();

            #region German
            // create new LanguageDetail for German
            languageDetail.Name = "Deutsch";
            languageDetail.PicturePath_Complete = "images\\German.png";
            languageDetail.PicturePath_Part = "images\\German_Part.png";

            languageDetail.LabelName = "Name";
            languageDetail.LabelAuthors = "Authoren";
            languageDetail.LabelDescription = "Beschreibung";
            languageDetail.LabelDirectors = "Regisseure";
            languageDetail.LabelMainActors = "Hauptdarsteller";
            languageDetail.LabelProducers = "Produzenten";
            languageDetail.LabelProductionyears = "Produktionsjahre";
            languageDetail.LabelDuration = "Dauer";
            languageDetail.LabelEpisode = "Folge";
            languageDetail.LabelEpisodeCount = "Folgenanzahl";
            languageDetail.LabelSeason = "Staffel";
            languageDetail.LabelGenre = "Genre";
            languageDetail.LabelFileName = "Dateiname";
            languageDetail.LabelNewList = "Neue Liste";

            languageDetail.MenuItemData = "Datei";
            languageDetail.MenuItemLanguages = "Sprachen";
            languageDetail.MenuItemLoad = "Laden";
            languageDetail.MenuItemNew = "Neu";
            languageDetail.MenuItemSave = "Speichern";
            languageDetail.MenuItemMerge = "Medienlisten zusammenfügen";

            languageDetail.ButtonAbort = "Abbrechen";
            languageDetail.ButtonChoose = "Auswählen";
            languageDetail.ButtonDone = "Fertig";
            languageDetail.ButtonAddEpisode = "Folge Hinzufügen";
            languageDetail.ButtonRemove = "Entfernen";
            languageDetail.ButtonAdd = "Hinzufügen";
            languageDetail.ButtonAll = "Alle";
            languageDetail.ButtonOk = "Ok";
            languageDetail.ButtonYes = "Ja";
            languageDetail.ButtonNo = "Nein";
            languageDetail.ButtonReplace = "Ersetzen";

            languageDetail.NoInformation = "Keine Angabe";
            languageDetail.Unknown = "Unbekannt";
            languageDetail.NoFileFound = "Keine Datei gefunden";

            languageDetail.MissingAnimeName = "Keinen Mediumnamen eingetragen!";
            languageDetail.MissingAnimeDescription = "Keine Beschreibung eingetragen!";
            languageDetail.MissingGenre = "Kein Genre eingetragen!";
            languageDetail.MissingSeasonName = "Keinen Seasonnamen eingetragen! \n Season: ";
            languageDetail.MissingEpisodeName = "Keinen Episodennamen eingetragen! \n Season: {0}\n Episode: {1}";

            languageDetail.OverwriteFile = "Möchten Sie die Datei wirklich überschreiben?";
            languageDetail.Warning = "Warnung";
            languageDetail.AnimeExists = "Medium existiert bereits in der Liste. Möchten sie trotzdem fortfahren?";

            // add to list
            LanguageDetails.Add(languageDetail);
            #endregion

            #region English
            // create new LanguageDetail for English
            languageDetail.Name = "English";
            languageDetail.PicturePath_Complete = "images\\English.png";
            languageDetail.PicturePath_Part = "images\\English_Part.png";

            languageDetail.LabelName = "Name";
            languageDetail.LabelAuthors = "Authors";
            languageDetail.LabelDescription = "Description";
            languageDetail.LabelDirectors = "Directors";
            languageDetail.LabelMainActors = "Mainactors";
            languageDetail.LabelProducers = "Producers";
            languageDetail.LabelProductionyears = "Produktionyear";
            languageDetail.LabelDuration = "Duration";
            languageDetail.LabelEpisode = "Episode";
            languageDetail.LabelEpisodeCount = "Episodecount";
            languageDetail.LabelSeason = "Season";
            languageDetail.LabelGenre = "Genre";
            languageDetail.LabelFileName = "Filename";
            languageDetail.LabelNewList = "New List";

            languageDetail.MenuItemData = "Data";
            languageDetail.MenuItemLanguages = "Languages";
            languageDetail.MenuItemLoad = "Load";
            languageDetail.MenuItemNew = "New";
            languageDetail.MenuItemSave = "Save";
            languageDetail.MenuItemMerge = "Merge media-lists";

            languageDetail.ButtonAbort = "Abort";
            languageDetail.ButtonChoose = "Choose";
            languageDetail.ButtonDone = "Done";
            languageDetail.ButtonAddEpisode = "Add Episode";
            languageDetail.ButtonRemove = "Remove";
            languageDetail.ButtonAdd = "Add";
            languageDetail.ButtonAll = "All";
            languageDetail.ButtonOk = "Ok";
            languageDetail.ButtonYes = "Yes";
            languageDetail.ButtonNo = "No";
            languageDetail.ButtonReplace = "Replace";

            languageDetail.NoInformation = "No information";
            languageDetail.Unknown = "Unknown";
            languageDetail.NoFileFound = "No file found";

            languageDetail.MissingAnimeName = "No medium name entered!";
            languageDetail.MissingAnimeDescription = "No description entered!";
            languageDetail.MissingGenre = "No genre entered!";
            languageDetail.MissingSeasonName = "No season name entered! \n Season: ";
            languageDetail.MissingEpisodeName = "No episode name entered! \n Season: {0} \n Episode: {1}";

            languageDetail.OverwriteFile = "Are you sure to overwrite the file?";
            languageDetail.Warning = "Warning";
            languageDetail.AnimeExists = "Medium already exists in this list. Do you want to continue?";

            // add to list
            LanguageDetails.Add(languageDetail);
            #endregion

            #region Japanese
            // create new LanguageDetail for Japanese
            languageDetail.Name = "日本語";
            languageDetail.PicturePath_Complete = "images\\Japan.png";
            languageDetail.PicturePath_Part = "images\\Japan_Part.png";

            languageDetail.LabelName = "名前";
            languageDetail.LabelAuthors = "著者";
            languageDetail.LabelDescription = "叙述";
            languageDetail.LabelDirectors = "理事";
            languageDetail.LabelMainActors = "主演";
            languageDetail.LabelProducers = "製作者";
            languageDetail.LabelProductionyears = "製造年";
            languageDetail.LabelDuration = "期限";
            languageDetail.LabelEpisode = "挿話";
            languageDetail.LabelEpisodeCount = "エピソード数";
            languageDetail.LabelSeason = "時期";
            languageDetail.LabelGenre = "ジャンル";
            languageDetail.LabelFileName = "メディアリスト";
            languageDetail.LabelNewList = "新しいリスト";

            languageDetail.MenuItemData = "素子";
            languageDetail.MenuItemLanguages = "言語";
            languageDetail.MenuItemLoad = "負荷";
            languageDetail.MenuItemNew = "新";
            languageDetail.MenuItemSave = "除いて";
            languageDetail.MenuItemMerge = "除いて";

            languageDetail.ButtonAbort = "アボート";
            languageDetail.ButtonChoose = "選び出す";
            languageDetail.ButtonDone = "終了";
            languageDetail.ButtonAddEpisode = "エピソードを追加";
            languageDetail.ButtonRemove = "取り除く";
            languageDetail.ButtonAdd = "追加";
            languageDetail.ButtonAll = "全";
            languageDetail.ButtonOk = "Ok";
            languageDetail.ButtonYes = "はい";
            languageDetail.ButtonNo = "番号";
            languageDetail.ButtonReplace = "交換";

            languageDetail.NoInformation = "情報なし";
            languageDetail.Unknown = "不明";
            languageDetail.NoFileFound = "ファイルが見つかりません";

            languageDetail.MissingAnimeName = "メディア名が入力されていません！";
            languageDetail.MissingAnimeDescription = "説明が入力されていません!";
            languageDetail.MissingGenre = "ジャンルが入力されていません!";
            languageDetail.MissingSeasonName = "シーズン名が入力されていません! \n シーズン: ";
            languageDetail.MissingEpisodeName = "エピソード名が入力されていません! \n シーズン: {0} \n 挿話: {1}";

            languageDetail.OverwriteFile = "ファイルを上書きしてもよろしいですか？";
            languageDetail.Warning = "警告";
            languageDetail.AnimeExists = "中はすでにリストに存在します。続けますか？";

            // add to list
            LanguageDetails.Add(languageDetail);
            #endregion

            // create default languages file
            // create formatter
            BinaryFormatter formatter = new BinaryFormatter();

            // create filestream
            FileStream stream = new FileStream(_filePath, FileMode.Create);

            // save the file
            formatter.Serialize(stream, LanguageDetails);

            stream.Close();
        }

        private void CreateDefaultGenreData(string _filePath)
        {
            // create default languages file
            // create formatter
            BinaryFormatter formatter = new BinaryFormatter();

            // create filestream
            FileStream stream = new FileStream(_filePath, FileMode.Create);

            // create new file
            // MainGenreList
            List<string> genres = new List<string>()
            {
                "Action",
                "Adventure",
                "Comedy",
                "Drama",
                "Slice of Life",
                "Fantasy",
                "Magic",
                "Supernatural",
                "Horror",
                "Mystery",
                "Psychological",
                "Romance",
                "Sci-Fi"
            };
            genres.Sort();

            GenreList.Main = genres;

            // SubGenreList
            genres = new List<string>()
            {
                "Cyberpunk",
                "Game",
                "Ecchi",
                "Demons",
                "Harem",
                "Josei",
                "Martial Arts",
                "Kids",
                "Historical",
                "Hentai",
                "Isekai",
                "Military",
                "Mecha",
                "Music",
                "Parody",
                "Police",
                "Post-Apocalyptic",
                "Reverse Harem",
                "School",
                "Seinen",
                "Shoujo-ai",
                "Shounen",
                "Shounen-ai",
                "Space",
                "Sports",
                "Super Power",
                "Tragedy",
                "Vampire",
                "Yuri",
                "Yaoi"
            };
            genres.Sort();

            GenreList.Sub = genres;

            // save the file
            formatter.Serialize(stream, GenreList);

            stream.Close();
        }

        private void CreateDefaultSettingData(string _filePath)
        {
            // create default languages file
            SaveSetting("Deutsch", _filePath, true);
        }

        private void SaveSetting(string _language, string _filePath, bool _new)
        {
            // create formatter
            BinaryFormatter formatter = new BinaryFormatter();

            // set setting
            Setting = _language;

            // create new file
            if (_new)
            {
                // create filestream
                FileStream stream = new FileStream(_filePath, FileMode.Create);

                // save the file
                formatter.Serialize(stream, Setting);

                stream.Close();
            }
            // open existing file
            else
            {
                // open filestream
                FileStream stream = new FileStream(_filePath, FileMode.Open);

                // save the file
                formatter.Serialize(stream, Setting);

                stream.Close();
            }
        }

        private void CreateLanguageMenuItem(LanguageDetail _languageDetail)
        {
            MenuItem menuItem = new MenuItem
            {
                Header = _languageDetail.Name,
                Foreground = new SolidColorBrush(Colors.White)
            };
            menuItem.Click += ChangeLanguage;

            Image image = new Image();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + _languageDetail.PicturePath_Complete))
            {
                image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + _languageDetail.PicturePath_Complete));
            }

            menuItem.Icon = image;

            // add menuitem to languagemenuitem
            ((MenuItem)this.FindName("menuitem_Languages")).Items.Add(menuItem);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            string[] tmp = comboBox.Tag.ToString().Split("_");

            // get ComboBoxTyp
            string typ = tmp[0];

            // get ComboBoxIndex
            int index = int.Parse(tmp[1]);

            // set default listtyp string
            string listtyp = "";
            if (tmp.Length == 3)
            {
                listtyp = tmp[2];
            }

            // listtyp with default setting
            ListTyp listTyp = ListTyp.MAINWINDOW;

            // animelist with default list
            List<Anime> animes = Animes;

            // get right TextBlock
            TextBlock textBlock = new TextBlock();
            if (typ == "D")
            {
                textBlock = (TextBlock)this.FindName("textblock_AnimeDescription_" + index);

                switch (listtyp)
                {
                    case "Left":
                        listTyp = ListTyp.MERGELEFT;
                        animes = mergeWindow.leftAnimeList;
                        textBlock = (TextBlock)mergeWindow.FindName("textblock_AnimeDescription_" + index + "_Left");
                        break;
                    case "New":
                        listTyp = ListTyp.MERGENEW;
                        animes = mergeWindow.newAnimeList;
                        textBlock = (TextBlock)mergeWindow.FindName("textblock_AnimeDescription_" + index + "_New");
                        break;
                    case "Right":
                        listTyp = ListTyp.MERGERIGHT;
                        animes = mergeWindow.rightAnimeList;
                        textBlock = (TextBlock)mergeWindow.FindName("textblock_AnimeDescription_" + index + "_Right");
                        break;
                }
            }
            else if (typ == "N")
            {
                textBlock = (TextBlock)this.FindName("textblock_AnimeName_" + index);

                switch (listtyp)
                {
                    case "Left":
                        listTyp = ListTyp.MERGELEFT;
                        animes = mergeWindow.leftAnimeList;
                        textBlock = (TextBlock)mergeWindow.FindName("textblock_AnimeName_" + index + "_Left");
                        break;
                    case "New":
                        listTyp = ListTyp.MERGENEW;
                        animes = mergeWindow.newAnimeList;
                        textBlock = (TextBlock)mergeWindow.FindName("textblock_AnimeName_" + index + "_New");
                        break;
                    case "Right":
                        listTyp = ListTyp.MERGERIGHT;
                        animes = mergeWindow.rightAnimeList;
                        textBlock = (TextBlock)mergeWindow.FindName("textblock_AnimeName_" + index + "_Right");
                        break;
                }
            }

            OnSelectionChanged(comboBox, index, typ, listTyp, animes, textBlock);
        }

        private void OnSelectionChanged(ComboBox _comboBox, int _index, string _typ, ListTyp _listTyp, List<Anime> _animes, TextBlock _textBlock)
        {
            // set right text
            _textBlock.Text = _animes[_index].Names[_comboBox.SelectedIndex].Value;

        }

        private void MenuItem_OpenMergeWindow(object sender, RoutedEventArgs e)
        {
            MedialistMerger mergerWindow = new MedialistMerger();

            // set reference to mainwindow
            mergerWindow.mainWindow = this;

            // set language and clickevents
            mergerWindow.SetLanguage(languageDetail);

            // set reference to mergerWindow
            mergeWindow = mergerWindow;

            mergerWindow.ShowDialog();
        }

        private void OpenSeason(object sender, RoutedEventArgs e)
        {

        }

        private void ShowHideDetail(object sender, RoutedEventArgs e)
        {
            // get button
            Button ShowHideButton = (Button)sender;
            // get tag
            string[] typ = ShowHideButton.Tag.ToString().Split("_");
            // get index
            int index = int.Parse(typ[1]);

            // get right grid
            DockPanel DetailDockpanel = (DockPanel)this.FindName("dockpanel_MainAnimeDetailDockpanel_" + index);
            // get right dockpanel
            DockPanel NameDockPanel = (DockPanel)this.FindName("dockpanel_AnimeName_" + index);

            string restTag = index.ToString();

            if (typ.Length == 3)
            {
                switch (typ[2])
                {
                    case "Left":
                        restTag += "_Left";
                        DetailDockpanel = (DockPanel)mergeWindow.FindName("dockpanel_MainAnimeDetailDockpanel_" + index + "_Left");
                        NameDockPanel = (DockPanel)mergeWindow.FindName("dockpanel_AnimeName_" + index + "_Left");
                        break;
                    case "New":
                        restTag += "_New";
                        DetailDockpanel = (DockPanel)mergeWindow.FindName("dockpanel_MainAnimeDetailDockpanel_" + index + "_New");
                        NameDockPanel = (DockPanel)mergeWindow.FindName("dockpanel_AnimeName_" + index + "_New");
                        break;
                    case "Right":
                        restTag += "_Right";
                        DetailDockpanel = (DockPanel)mergeWindow.FindName("dockpanel_MainAnimeDetailDockpanel_" + index + "_Right");
                        NameDockPanel = (DockPanel)mergeWindow.FindName("dockpanel_AnimeName_" + index + "_Right");
                        break;
                }
            }

            if (typ[0] == "Hide")
            {
                // set new content
                ShowHideButton.Content = "V";

                // set new tag
                ShowHideButton.Tag = "Show_" + restTag;

                // hide detail
                DetailDockpanel.Visibility = Visibility.Collapsed;
                // set new margin for NameDockpanel
                NameDockPanel.Margin = new Thickness(0, 0, 0, 5);
            }
            else if (typ[0] == "Show")
            {
                // set new content
                ShowHideButton.Content = "Λ";

                // set new tag
                ShowHideButton.Tag = "Hide_" + restTag;

                // show detail
                DetailDockpanel.Visibility = Visibility.Visible;
                // set new margin for NameDockpanel
                NameDockPanel.Margin = new Thickness(0, 0, 0, 10);
            }
        }

        private void SetLanguage(LanguageDetail _languageDetail)
        {
            // get all elements for change language
            MenuItem menuitem_Data = (MenuItem)this.FindName("menuitem_Data");
            MenuItem menuitem_New = (MenuItem)this.FindName("menuitem_New");
            MenuItem menuitem_Load = (MenuItem)this.FindName("menuitem_Load");
            MenuItem menuitem_Save = (MenuItem)this.FindName("menuitem_Save");
            MenuItem menuitem_Languages = (MenuItem)this.FindName("menuitem_Languages");
            MenuItem menuitem_Merge = (MenuItem)this.FindName("menuitem_Merge");

            // set right texts
            menuitem_Data.Header = _languageDetail.MenuItemData;
            menuitem_New.Header = _languageDetail.MenuItemNew;
            menuitem_Load.Header = _languageDetail.MenuItemLoad;
            menuitem_Save.Header = _languageDetail.MenuItemSave;
            menuitem_Languages.Header = _languageDetail.MenuItemLanguages;
            menuitem_Merge.Header = _languageDetail.MenuItemMerge;

            languageDetail = _languageDetail;

            // save changes
            SaveSetting(_languageDetail.Name, AppDomain.CurrentDomain.BaseDirectory + "data\\settings.dat", false);

            // go through loaded animelist
            for (int i = 0; i < Animes.Count; i++)
            {
                // if anime isnt deleted
                if (Animes[i] != null)
                {
                    Label label;
                    #region get labels and translate them
                    // description
                    label = (Label)this.FindName("label_AnimeDescription_" + i);
                    label.Content = _languageDetail.LabelDescription;

                    // duration
                    label = (Label)this.FindName("label_AnimeDuration_" + i);
                    // strings for duration
                    string hour;
                    string minute;
                    string second;
                    // if hours are less than 10
                    if (Animes[i].Duration_Hours < 10)
                    {
                        // add "0" before duration
                        hour = "0" + Animes[i].Duration_Hours.ToString();
                    }
                    else
                    {
                        hour = Animes[i].Duration_Hours.ToString();
                    }
                    // if minutes are less than
                    if (Animes[i].Duration_Minutes < 10)
                    {
                        // add "0" before duration
                        minute = "0" + Animes[i].Duration_Minutes.ToString();
                    }
                    else
                    {
                        minute = Animes[i].Duration_Minutes.ToString();
                    }
                    // if seconds are less than 10
                    if (Animes[i].Duration_Seconds < 10)
                    {
                        // add "0" before duration
                        second = "0" + Animes[i].Duration_Seconds.ToString();
                    }
                    else
                    {
                        second = Animes[i].Duration_Seconds.ToString();
                    }

                    label.Content = _languageDetail.LabelDuration + " " + hour + ":" + minute + ":" + second;

                    // episodecount
                    label = (Label)this.FindName("label_EpisodeCount_" + i);
                    label.Content = _languageDetail.LabelEpisodeCount + " " + Animes[i].EpisodeCount;

                    // season
                    label = (Label)this.FindName("label_AnimeSeason_" + i);
                    label.Content = _languageDetail.LabelSeason;

                    // genres
                    label = (Label)this.FindName("label_AnimeGenres_" + i);
                    label.Content = _languageDetail.LabelGenre;

                    // productionyear
                    label = (Label)this.FindName("label_Productionyear_" + i);
                    label.Content = _languageDetail.LabelProductionyears;

                    // mainactors
                    label = (Label)this.FindName("label_MainActor_" + i);
                    label.Content = _languageDetail.LabelMainActors;

                    // producers
                    label = (Label)this.FindName("label_Producer_" + i);
                    label.Content = _languageDetail.LabelProducers;

                    // directors
                    label = (Label)this.FindName("label_Director_" + i);
                    label.Content = _languageDetail.LabelDirectors;

                    // authors
                    label = (Label)this.FindName("label_Author_" + i);
                    label.Content = _languageDetail.LabelAuthors;

                    // productionyearvalue
                    label = (Label)this.FindName("label_ProductionyearValues_" + i);

                    string year1 = languageDetail.Unknown;
                    string year2 = languageDetail.Unknown;

                    if (Animes[i].Productionyear[0] != 0)
                    {
                        year1 = Animes[i].Productionyear[0].ToString();
                    }
                    if (Animes[i].Productionyear[1] != 0)
                    {
                        year2 = Animes[i].Productionyear[1].ToString();
                    }

                    label.Content = _languageDetail.LabelDuration + " " + year1 + ":" + year2;

                    if (Animes[i].MainActors.Count == 0)
                    {
                        label = (Label)this.FindName("label_NoMainActors_" + i);
                        label.Content = _languageDetail.LabelAuthors;
                    }
                    if (Animes[i].Producers.Count == 0)
                    {
                        label = (Label)this.FindName("label_NoProducers_" + i);
                        label.Content = _languageDetail.LabelAuthors;
                    }
                    if (Animes[i].Directors.Count == 0)
                    {
                        label = (Label)this.FindName("label_NoDirectors_" + i);
                        label.Content = _languageDetail.LabelAuthors;
                    }
                    if (Animes[i].Authors.Count == 0)
                    {
                        label = (Label)this.FindName("label_NoAuthors_" + i);
                        label.Content = _languageDetail.LabelAuthors;
                    }
                    #endregion
                }
            }
        }

        private void ChangeLanguage(object sender, RoutedEventArgs e)
        {
            string language = ((MenuItem)sender).Header.ToString();

            if (language != languageDetail.Name)
            {
                foreach (LanguageDetail ld in LanguageDetails)
                {
                    if (ld.Name == language)
                    {
                        SetLanguage(ld);
                        languageDetail = ld;
                        break;
                    }
                }
            }
        }

        public void ClearDockpanel(List<Anime> _animes, ListTyp _listTyp, Window _window)
        {
            if (_listTyp == ListTyp.MAINWINDOW)
            {
                // loop for anime.Count to get all registered Elements and Unregister them to re-register for new/loaded animelist
                for (int i = 0; i < _animes.Count; i++)
                {
                    UnregisterElement(_listTyp, _window, _animes[i], i);
                }

                // reset AnimeCount
                AnimeCount = 0;

                // disable saving animelist
                saveAnimeList.IsEnabled = false;
            }
            else if (_listTyp == ListTyp.MERGELEFT)
            {
                // loop for anime.Count to get all registered Elements and Unregister them to re-register for new/loaded animelist
                for (int i = _animes.Count - 1; i >= 0; i--)
                {
                    UnregisterElement(_listTyp, _window, _animes[i], i);
                }
            }
            else if (_listTyp == ListTyp.MERGENEW)
            {
                // loop for anime.Count to get all registered Elements and Unregister them to re-register for new/loaded animelist
                for (int i = _animes.Count - 1; i >= 0; i--)
                {
                    UnregisterElement(_listTyp, _window, _animes[i], i);
                }
            }
            else if (_listTyp == ListTyp.MERGERIGHT)
            {
                // loop for anime.Count to get all registered Elements and Unregister them to re-register for new/loaded animelist
                for (int i = _animes.Count - 1; i >= 0; i--)
                {
                    UnregisterElement(_listTyp, _window, _animes[i], i);
                }
            }
        }

        public void UnregisterElement(ListTyp _listTyp, Window _window, Anime _anime, int _index)
        {
            if (_listTyp == ListTyp.MAINWINDOW)
            {
                // unregister all register elements
                _window.UnregisterName("gird_Anime_" + _index);
                _window.UnregisterName("dockpanel_AnimeName_" + _index);
                _window.UnregisterName("textblock_AnimeName_" + _index);
                _window.UnregisterName("dockpanel_MainAnimeDetailDockpanel_" + _index);
                _window.UnregisterName("image_AnimeCover_" + _index);
                _window.UnregisterName("textblock_AnimeDescription_" + _index);

                _window.UnregisterName("label_AnimeDescription_" + _index);
                _window.UnregisterName("label_AnimeDuration_" + _index);
                _window.UnregisterName("label_EpisodeCount_" + _index);
                _window.UnregisterName("label_AnimeSeason_" + _index);
                _window.UnregisterName("label_AnimeGenres_" + _index);
                _window.UnregisterName("label_Productionyear_" + _index);
                _window.UnregisterName("label_ProductionyearValues_" + _index);
                _window.UnregisterName("label_MainActor_" + _index);
                _window.UnregisterName("label_Producer_" + _index);
                _window.UnregisterName("label_Director_" + _index);
                _window.UnregisterName("label_Author_" + _index);

                // unregister if they are registered
                if (_anime.MainActors.Count == 0)
                    _window.UnregisterName("label_NoMainActors_" + _index);
                if (_anime.Producers.Count == 0)
                    _window.UnregisterName("label_NoProducers_" + _index);
                if (_anime.Directors.Count == 0)
                    _window.UnregisterName("label_NoDirectors_" + _index);
                if (_anime.Authors.Count == 0)
                    _window.UnregisterName("label_NoAuthors_" + _index);

                // get dockpanel
                DockPanel dockPanel = (DockPanel)_window.FindName("dockpanel_AnimeList");

                dockPanel.Children.RemoveAt(_index);
            }
            else if (_listTyp == ListTyp.MERGELEFT)
            {
                // unregister all register elements
                _window.UnregisterName("gird_Anime_" + _index + "_Left");
                _window.UnregisterName("dockpanel_AnimeName_" + _index + "_Left");
                _window.UnregisterName("textblock_AnimeName_" + _index + "_Left");
                _window.UnregisterName("dockpanel_MainAnimeDetailDockpanel_" + _index + "_Left");
                _window.UnregisterName("image_AnimeCover_" + _index + "_Left");
                _window.UnregisterName("textblock_AnimeDescription_" + _index + "_Left");

                _window.UnregisterName("label_AnimeDescription_" + _index + "_Left");
                _window.UnregisterName("label_AnimeDuration_" + _index + "_Left");
                _window.UnregisterName("label_EpisodeCount_" + _index + "_Left");
                _window.UnregisterName("label_AnimeSeason_" + _index + "_Left");
                _window.UnregisterName("label_AnimeGenres_" + _index + "_Left");
                _window.UnregisterName("label_Productionyear_" + _index + "_Left");
                _window.UnregisterName("label_ProductionyearValues_" + _index + "_Left");
                _window.UnregisterName("label_MainActor_" + _index + "_Left");
                _window.UnregisterName("label_Producer_" + _index + "_Left");
                _window.UnregisterName("label_Director_" + _index + "_Left");
                _window.UnregisterName("label_Author_" + _index + "_Left");

                // unregister if they are registered
                if (_anime.MainActors.Count == 0)
                    _window.UnregisterName("label_NoMainActors_" + _index + "_Left");
                if (_anime.Producers.Count == 0)
                    _window.UnregisterName("label_NoProducers_" + _index + "_Left");
                if (_anime.Directors.Count == 0)
                    _window.UnregisterName("label_NoDirectors_" + _index + "_Left");
                if (_anime.Authors.Count == 0)
                    _window.UnregisterName("label_NoAuthors_" + _index + "_Left");

                // get dockpanel
                DockPanel dockPanel = (DockPanel)_window.FindName("dockpanel_AnimeList_Left");

                dockPanel.Children.RemoveAt(_index);
            }
            else if (_listTyp == ListTyp.MERGENEW)
            {
                // unregister all register elements
                _window.UnregisterName("gird_Anime_" + _index + "_New");
                _window.UnregisterName("dockpanel_AnimeName_" + _index + "_New");
                _window.UnregisterName("textblock_AnimeName_" + _index + "_New");
                _window.UnregisterName("dockpanel_MainAnimeDetailDockpanel_" + _index + "_New");
                _window.UnregisterName("image_AnimeCover_" + _index + "_New");
                _window.UnregisterName("textblock_AnimeDescription_" + _index + "_New");

                _window.UnregisterName("label_AnimeDescription_" + _index + "_New");
                _window.UnregisterName("label_AnimeDuration_" + _index + "_New");
                _window.UnregisterName("label_EpisodeCount_" + _index + "_New");
                _window.UnregisterName("label_AnimeSeason_" + _index + "_New");
                _window.UnregisterName("label_AnimeGenres_" + _index + "_New");
                _window.UnregisterName("label_Productionyear_" + _index + "_New");
                _window.UnregisterName("label_ProductionyearValues_" + _index + "_New");
                _window.UnregisterName("label_MainActor_" + _index + "_New");
                _window.UnregisterName("label_Producer_" + _index + "_New");
                _window.UnregisterName("label_Director_" + _index + "_New");
                _window.UnregisterName("label_Author_" + _index + "_New");

                // unregister if they are registered
                if (_anime.MainActors.Count == 0)
                    _window.UnregisterName("label_NoMainActors_" + _index + "_New");
                if (_anime.Producers.Count == 0)
                    _window.UnregisterName("label_NoProducers_" + _index + "_New");
                if (_anime.Directors.Count == 0)
                    _window.UnregisterName("label_NoDirectors_" + _index + "_New");
                if (_anime.Authors.Count == 0)
                    _window.UnregisterName("label_NoAuthors_" + _index + "_New");

                // get dockpanel
                DockPanel dockPanel = (DockPanel)_window.FindName("dockpanel_AnimeList_New");

                dockPanel.Children.RemoveAt(_index);
            }
            else if (_listTyp == ListTyp.MERGERIGHT)
            {
                // unregister all register elements
                _window.UnregisterName("gird_Anime_" + _index + "_Right");
                _window.UnregisterName("dockpanel_AnimeName_" + _index + "_Right");
                _window.UnregisterName("textblock_AnimeName_" + _index + "_Right");
                _window.UnregisterName("dockpanel_MainAnimeDetailDockpanel_" + _index + "_Right");
                _window.UnregisterName("image_AnimeCover_" + _index + "_Right");
                _window.UnregisterName("textblock_AnimeDescription_" + _index + "_Right");

                _window.UnregisterName("label_AnimeDescription_" + _index + "_Right");
                _window.UnregisterName("label_AnimeDuration_" + _index + "_Right");
                _window.UnregisterName("label_EpisodeCount_" + _index + "_Right");
                _window.UnregisterName("label_AnimeSeason_" + _index + "_Right");
                _window.UnregisterName("label_AnimeGenres_" + _index + "_Right");
                _window.UnregisterName("label_Productionyear_" + _index + "_Right");
                _window.UnregisterName("label_ProductionyearValues_" + _index + "_Right");
                _window.UnregisterName("label_MainActor_" + _index + "_Right");
                _window.UnregisterName("label_Producer_" + _index + "_Right");
                _window.UnregisterName("label_Director_" + _index + "_Right");
                _window.UnregisterName("label_Author_" + _index + "_Right");

                // unregister if they are registered
                if (_anime.MainActors.Count == 0)
                    _window.UnregisterName("label_NoMainActors_" + _index + "_Right");
                if (_anime.Producers.Count == 0)
                    _window.UnregisterName("label_NoProducers_" + _index + "_Right");
                if (_anime.Directors.Count == 0)
                    _window.UnregisterName("label_NoDirectors_" + _index + "_Right");
                if (_anime.Authors.Count == 0)
                    _window.UnregisterName("label_NoAuthors_" + _index + "_Right");

                // get dockpanel
                DockPanel dockPanel = (DockPanel)_window.FindName("dockpanel_AnimeList_Right");

                dockPanel.Children.RemoveAt(_index);
            }
        }

        public void StartDragEvent(object sender, MouseButtonEventArgs e)
        {
            // get dependencyObject
            TextBlock animeName = (TextBlock)sender;

            // get values to figure out which anime of which list is draged
            string[] tmps = animeName.Name.Split("_");

            string listSide = tmps[3];
            int index = int.Parse(tmps[2]);

            // create new data object
            DataObject dataObject = new DataObject();
            // set dependencyobject as data
            dataObject.SetData("DependencyObject", animeName);
            // and fill with right anime
            switch (listSide)
            {
                case "Left":
                    dataObject.SetData("Anime", mergeWindow.leftAnimeList[index]);
                    dataObject.SetData("List", listSide);
                    mergeWindow.label_AnimeList_New.AllowDrop = true;
                    mergeWindow.button_RemoveAnimeNewList.AllowDrop = false;
                    break;
                case "New":
                    dataObject.SetData("Anime", mergeWindow.newAnimeList[index]);
                    dataObject.SetData("List", listSide);
                    mergeWindow.label_AnimeList_New.AllowDrop = false;
                    mergeWindow.button_RemoveAnimeNewList.AllowDrop = true;
                    break;
                case "Right":
                    dataObject.SetData("Anime", mergeWindow.rightAnimeList[index]);
                    dataObject.SetData("List", listSide);
                    mergeWindow.label_AnimeList_New.AllowDrop = true;
                    mergeWindow.button_RemoveAnimeNewList.AllowDrop = false;
                    break;
            }

            DragDrop.DoDragDrop(animeName, dataObject, DragDropEffects.Move);
        }

        public void TargetDrop(object sender, DragEventArgs e)
        {
            // get values from dragobject
            Anime dragedAnime = e.Data.GetData("Anime") as Anime;
            TextBlock animeName = e.Data.GetData("DependencyObject") as TextBlock;
            string listSide = e.Data.GetData("List") as string;

            // set TextBlock visual effect to show that this anime is already draged
            animeName.IsEnabled = false;
            animeName.Foreground = new SolidColorBrush(Colors.LightSlateGray);

            // get index of anime
            int index = int.Parse(animeName.Tag.ToString());

            // set indexer
            Indexer indexer = new Indexer
            {
                Key = mergeWindow.newAnimeList.Count,
                Value = index
            };

            switch (listSide)
            {
                case "Left":
                    // check for existing anime
                    if (CheckForExistingAnime(dragedAnime, mergeWindow.newAnimeList))
                    {
                        OwnMessageBox mb = new OwnMessageBox();
                        mb.SetElements(OwnMessageboxTyp.WARNING_REPLACE_ADD_ABORT, languageDetail.AnimeExists);
                        mb.SetLanguage(languageDetail);
                        mb.ShowDialog();

                        if (mb.MessageboxResult == OwnMessageboxResult.ADD)
                        {
                            // add anime to correct list and correct dockpanel
                            AddAnime(dragedAnime, mergeWindow.newAnimeList, ListTyp.MERGENEW, mergeWindow, true, null);
                            mergeWindow.label_AnimeList_New.AllowDrop = false;
                            mergeWindow.button_SaveNewList.IsEnabled = true;
                            mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                            // add index to indeceslist
                            mergeWindow.IndecesLeft.Add(indexer);

                            if (mergeWindow.IndecesLeft.Count == mergeWindow.dockpanel_AnimeList_Left.Children.Count)
                            {
                                mergeWindow.button_AllLeft.IsEnabled = false;
                            }

                            break;
                        }
                        else if (mb.MessageboxResult == OwnMessageboxResult.REPLACE)
                        {
                            ReEnableDragedAnime(IndexOfMatchedAnime);

                            // remove matched anime
                            mergeWindow.newAnimeList[IndexOfMatchedAnime] = null;
                            mergeWindow.dockpanel_AnimeList_New.Children[IndexOfMatchedAnime].Visibility = Visibility.Collapsed;

                            // add anime to correct list and correct dockpanel
                            AddAnime(dragedAnime, mergeWindow.newAnimeList, ListTyp.MERGENEW, mergeWindow, true, null);
                            mergeWindow.label_AnimeList_New.AllowDrop = false;
                            mergeWindow.button_SaveNewList.IsEnabled = true;
                            mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                            // add index to indeceslist
                            mergeWindow.IndecesLeft.Add(indexer);

                            if (mergeWindow.IndecesLeft.Count == mergeWindow.dockpanel_AnimeList_Left.Children.Count)
                            {
                                mergeWindow.button_AllLeft.IsEnabled = false;
                            }

                            break;
                        }

                        animeName.IsEnabled = true;
                        animeName.Foreground = new SolidColorBrush(Colors.White);

                        break;
                    }
                    else
                    {
                        // add anime to correct list and correct dockpanel
                        AddAnime(dragedAnime, mergeWindow.newAnimeList, ListTyp.MERGENEW, mergeWindow, true, null);
                        mergeWindow.label_AnimeList_New.AllowDrop = false;
                        mergeWindow.button_SaveNewList.IsEnabled = true;
                        mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                        // add index to indeceslist
                        mergeWindow.IndecesLeft.Add(indexer);

                        if (mergeWindow.IndecesLeft.Count == mergeWindow.dockpanel_AnimeList_Left.Children.Count)
                        {
                            mergeWindow.button_AllLeft.IsEnabled = false;
                        }

                        break;
                    }
                case "New":
                    // remove anime from newList
                    mergeWindow.newAnimeList[index] = null;
                    // remove anime from dockpanle
                    mergeWindow.dockpanel_AnimeList_New.Children[index].Visibility = Visibility.Collapsed;
                    mergeWindow.button_RemoveAnimeNewList.AllowDrop = false;

                    ReEnableDragedAnime(index);

                    bool isVisible = false;
                    for (int i = 0; i < mergeWindow.dockpanel_AnimeList_New.Children.Count; i++)
                    {
                        if (mergeWindow.dockpanel_AnimeList_New.Children[i].Visibility == Visibility.Visible)
                        {
                            mergeWindow.button_SaveNewList.IsEnabled = true;
                            mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                            isVisible = true;
                            break;
                        }
                    }

                    if (!isVisible)
                    {
                        mergeWindow.button_SaveNewList.IsEnabled = false;
                        mergeWindow.button_RemoveAnimeNewList.IsEnabled = false;
                    }

                    break;
                case "Right":
                    // check for existing anime
                    if (CheckForExistingAnime(dragedAnime, mergeWindow.newAnimeList))
                    {
                        OwnMessageBox mb = new OwnMessageBox();
                        mb.SetElements(OwnMessageboxTyp.WARNING_REPLACE_ADD_ABORT, languageDetail.AnimeExists);
                        mb.SetLanguage(languageDetail);
                        mb.ShowDialog();

                        if (mb.MessageboxResult == OwnMessageboxResult.ADD)
                        {
                            // add anime to correct list and correct dockpanel
                            AddAnime(dragedAnime, mergeWindow.newAnimeList, ListTyp.MERGENEW, mergeWindow, true, null);
                            mergeWindow.label_AnimeList_New.AllowDrop = false;
                            mergeWindow.button_SaveNewList.IsEnabled = true;
                            mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                            // add index to indeceslist
                            mergeWindow.IndecesRight.Add(indexer);

                            if (mergeWindow.IndecesRight.Count == mergeWindow.dockpanel_AnimeList_Right.Children.Count)
                            {
                                mergeWindow.button_AllRight.IsEnabled = false;
                            }

                            break;
                        }
                        else if (mb.MessageboxResult == OwnMessageboxResult.REPLACE)
                        {
                            ReEnableDragedAnime(IndexOfMatchedAnime);

                            // remove matched anime
                            mergeWindow.newAnimeList[IndexOfMatchedAnime] = null;
                            mergeWindow.dockpanel_AnimeList_New.Children[IndexOfMatchedAnime].Visibility = Visibility.Collapsed;

                            // add anime to correct list and correct dockpanel
                            AddAnime(dragedAnime, mergeWindow.newAnimeList, ListTyp.MERGENEW, mergeWindow, true, null);
                            mergeWindow.label_AnimeList_New.AllowDrop = false;
                            mergeWindow.button_SaveNewList.IsEnabled = true;
                            mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                            // add index to indeceslist
                            mergeWindow.IndecesRight.Add(indexer);

                            if (mergeWindow.IndecesRight.Count == mergeWindow.dockpanel_AnimeList_Right.Children.Count)
                            {
                                mergeWindow.button_AllRight.IsEnabled = false;
                            }

                            break;
                        }

                        break;
                    }
                    else
                    {
                        // add anime to correct list and correct dockpanel
                        AddAnime(dragedAnime, mergeWindow.newAnimeList, ListTyp.MERGENEW, mergeWindow, true, null);
                        mergeWindow.label_AnimeList_New.AllowDrop = false;
                        mergeWindow.button_SaveNewList.IsEnabled = true;
                        mergeWindow.button_RemoveAnimeNewList.IsEnabled = true;
                        // add index to indeceslist
                        mergeWindow.IndecesRight.Add(indexer);

                        if (mergeWindow.IndecesRight.Count == mergeWindow.dockpanel_AnimeList_Right.Children.Count)
                        {
                            mergeWindow.button_AllRight.IsEnabled = false;
                        }

                        animeName.IsEnabled = true;
                        animeName.Foreground = new SolidColorBrush(Colors.White);

                        break;
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="_left">true = left, false = right</param>
        public void ReEnableDragedAnime(int _index)
        {
            bool found = false;
            // check the anime for re-enable label of the anime which was dragged into new list
            for (int i = 0; i < mergeWindow.IndecesLeft.Count; i++)
            {
                if (mergeWindow.IndecesLeft[i].Key == _index)
                {
                    found = true;
                    TextBlock tb = (TextBlock)mergeWindow.FindName("textblock_AnimeName_" + mergeWindow.IndecesLeft[i].Value + "_Left");
                    tb.IsEnabled = true;
                    tb.Foreground = new SolidColorBrush(Colors.White);
                    mergeWindow.button_AllLeft.IsEnabled = true;
                    mergeWindow.IndecesLeft.RemoveAt(i);
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i < mergeWindow.IndecesRight.Count; i++)
                {
                    if (mergeWindow.IndecesRight[i].Key == _index)
                    {
                        TextBlock tb = (TextBlock)mergeWindow.FindName("textblock_AnimeName_" + mergeWindow.IndecesRight[i].Value + "_Right");
                        tb.IsEnabled = true;
                        tb.Foreground = new SolidColorBrush(Colors.White);
                        mergeWindow.button_AllRight.IsEnabled = true;
                        mergeWindow.IndecesRight.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if anime was founded
        /// </summary>
        /// <param name="_animeToCheck">Anime to check</param>
        /// <param name="_animeList">List to check</param>
        /// <returns></returns>
        public bool CheckForExistingAnime(Anime _animeToCheck, List<Anime> _animeList)
        {
            // if animelist isnt empty
            if (_animeList.Count > 0)
            {
                // go through every anime in the list
                for (int i = 0; i < _animeList.Count; i++)
                {
                    if (_animeList[i] == null)
                    {
                        continue;
                    }
                    // check animename accurat because it is possible that the original name is translated differently
                    // go through all languages of the given anime
                    for (int j = 0; j < _animeToCheck.Names.Count; j++)
                    {
                        // go through all languages of the given animelist
                        for (int k = 0; k < _animeList[i].Names.Count; k++)
                        {
                            // if language matched
                            if (_animeToCheck.Names[j].Name == _animeList[i].Names[k].Name)
                            {
                                // if name matched
                                if (_animeToCheck.Names[j].Value == _animeList[i].Names[k].Value)
                                {
                                    // set index of checked list
                                    IndexOfMatchedAnime = i;
                                    // set matched anime
                                    MatchedAnime = _animeToCheck;

                                    // anime exist in the list
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            // anime isnt existing in the giving list
            return false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // set right sizes for label in the statusbar
            label_StatusbarRemoveAnime.Width = e.NewSize.Width;
        }
    }

    [System.Serializable]
    public class Genre
    {
        /// <summary>
        /// List of all MainGenre
        /// </summary>
        public List<string> Main = new List<string>();

        /// <summary>
        /// List of all SubGenre
        /// </summary>
        public List<string> Sub = new List<string>();
    }

    [System.Serializable]
    public class Anime
    {
        /// <summary>
        /// List of names in different languages
        /// </summary>
        public List<Information> Names = new List<Information>();

        /// <summary>
        /// List of descriptions in different languages
        /// </summary>
        public List<Information> Descriptions = new List<Information>();

        /// <summary>
        /// List of all MainGenres of the Anime
        /// </summary>
        public List<string> MainGenres = new List<string>();

        /// <summary>
        /// List of all MainGenres of the Anime
        /// </summary>
        public List<string> SubGenres = new List<string>();

        /// <summary>
        /// List of all Dubs
        /// </summary>
        public List<Information> Dubs = new List<Information>();

        /// <summary>
        /// List of all Subs
        /// </summary>
        public List<Information> Subs = new List<Information>();

        /// <summary>
        /// Cover-ImageSource of the Anime
        /// </summary>
        public string CoverSource;

        /// <summary>
        /// Duration of the Anime in hour
        /// </summary>
        public int Duration_Hours;

        /// <summary>
        /// Duration of the Anime in minutes
        /// </summary>
        public int Duration_Minutes;

        /// <summary>
        /// Duration of the Anime in seconds
        /// </summary>
        public int Duration_Seconds;

        /// <summary>
        /// List of all Seasons of the Anime
        /// </summary>
        public List<Season> Seasons = new List<Season>();

        /// <summary>
        /// Episodecount of this Anime
        /// </summary>
        public int EpisodeCount;

        /// <summary>
        /// Productionyear of this anime
        /// </summary>
        public int[] Productionyear = new int[] { 0, 0 };

        /// <summary>
        /// list of all Author of this anime
        /// </summary>
        public List<string> Authors = new List<string>();

        /// <summary>
        /// List of all Directors of this anime
        /// </summary>
        public List<string> Directors = new List<string>();

        /// <summary>
        /// List of all Producers of this anime
        /// </summary>
        public List<string> Producers = new List<string>();

        /// <summary>
        /// List of all MainActors of this anime
        /// </summary>
        public List<string> MainActors = new List<string>();
    }

    [System.Serializable]
    public class Season
    {
        /// <summary>
        /// List of names in different languages
        /// </summary>
        public string Name;

        /// <summary>
        /// List of all Epiodes of the Season
        /// </summary>
        public List<Episode> Episodes = new List<Episode>();

        /// <summary>
        /// List of all Dubs
        /// </summary>
        public List<Information> Dubs = new List<Information>();

        /// <summary>
        /// List of all Subs
        /// </summary>
        public List<Information> Subs = new List<Information>();

        /// <summary>
        /// Duration of the Season in hour
        /// </summary>
        public int Duration_Hours;

        /// <summary>
        /// Duration of the Season in minutes
        /// </summary>
        public int Duration_Minutes;

        /// <summary>
        /// Duration of the Season in seconds
        /// </summary>
        public int Duration_Seconds;
    }

    [System.Serializable]
    public class Episode
    {
        /// <summary>
        /// List of names in different languages
        /// </summary>
        public List<Information> Names = new List<Information>();

        /// <summary>
        /// List of descriptions in different languages
        /// </summary>
        public List<Information> Descriptions = new List<Information>();

        /// <summary>
        /// List with all Dubs
        /// </summary>
        public List<string> Dubs = new List<string>();

        /// <summary>
        /// List with all Subs
        /// </summary>
        public List<string> Subs = new List<string>();

        /// <summary>
        /// Duration of the Episode in hour
        /// </summary>
        public int Duration_Hours;

        /// <summary>
        /// Duration of the Episode in minutes
        /// </summary>
        public int Duration_Minutes;

        /// <summary>
        /// Duration of the Episode in seconds
        /// </summary>
        public int Duration_Seconds;
    }

    [System.Serializable]
    public struct Information
    {
        public string Name;
        public string Value;
    }

    [System.Serializable]
    public class EditAnime
    {
        public Anime Anime;
        public int Index;
        public Window Window;
        public List<Anime> AnimeList;
        public ListTyp ListTyp;
    }

    [System.Serializable]
    public struct LanguageDetail
    {
        /// <summary>
        /// Languagename
        /// </summary>
        public string Name;
        public string PicturePath_Complete;
        public string PicturePath_Part;

        public string MenuItemData;
        public string MenuItemNew;
        public string MenuItemLoad;
        public string MenuItemSave;
        public string MenuItemLanguages;
        public string MenuItemMerge;

        public string LabelName;
        public string LabelDescription;
        public string LabelEpisode;
        public string LabelDuration;
        public string LabelProductionyears;
        public string LabelMainActors;
        public string LabelProducers;
        public string LabelDirectors;
        public string LabelAuthors;
        public string LabelEpisodeCount;
        public string LabelSeason;
        public string LabelGenre;
        public string LabelFileName;
        public string LabelNewList;

        public string ButtonChoose;
        public string ButtonAddEpisode;
        public string ButtonDone;
        public string ButtonAbort;
        public string ButtonRemove;
        public string ButtonAdd;
        public string ButtonAll;
        public string ButtonOk;
        public string ButtonYes;
        public string ButtonNo;
        public string ButtonReplace;

        public string NoInformation;
        public string Unknown;
        public string NoFileFound;

        #region Message
        public string MissingAnimeName;
        public string MissingAnimeDescription;
        public string MissingGenre;
        public string MissingSeasonName;
        public string MissingEpisodeName;

        public string OverwriteFile;
        public string Warning;
        public string AnimeExists;
        #endregion

        #region ToolTips

        #endregion
    }

    public enum ListTyp
    {
        MAINWINDOW,
        MERGELEFT,
        MERGERIGHT,
        MERGENEW
    }
}
