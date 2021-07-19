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
using System.Threading;

namespace AnimeWatchList
{
    /// <summary>
    /// Interaktionslogik für AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        /// <summary>
        /// reference of mainwindow
        /// </summary>
        public MainWindow MainWindow = null;

        /// <summary>
        /// All information that need to edit anime
        /// </summary>
        public EditAnime EA = null;

        private ImageBrush textbox_EpisodeNameImageBrush = new ImageBrush();
        private ImageBrush textbox_EpisodeDurationHourImageBrush = new ImageBrush();
        private ImageBrush textbox_EpisodeDurationMinuteImageBrush = new ImageBrush();
        private ImageBrush textbox_EpisodeDurationSecondImageBrush = new ImageBrush();

        private Anime anime = new Anime
        {
            Names = new List<Information>(),
            Descriptions = new List<Information>(),
            MainGenres = new List<string>(),
            SubGenres = new List<string>(),
            Dubs = new List<Information>(),
            Subs = new List<Information>(),
            Seasons = new List<Season>(),
            Productionyear = new int[] { 0, 0 },
            Authors = new List<string>(),
            Directors = new List<string>(),
            Producers = new List<string>(),
            MainActors = new List<string>()
        };

        /// <summary>
        /// counter for seasons
        /// </summary>
        private int seasonCount;

        /// <summary>
        /// texts in right language
        /// </summary>
        private LanguageDetail languageDetail;

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

        public AddWindow()
        {
            InitializeComponent();

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\Textbox_Background_Episodename.png";
            // Create an ImageBrush for empty Episodename-textbox.
            if (File.Exists(filePath))
            {
                textbox_EpisodeNameImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri(filePath, UriKind.Relative)
                    );
            }
            textbox_EpisodeNameImageBrush.AlignmentX = AlignmentX.Left;
            textbox_EpisodeNameImageBrush.Stretch = Stretch.None;

            filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\Textbox_Background_Hour.png";
            // Create an ImageBrush for empty EpisodDurationHour-textbox.
            if (File.Exists(filePath))
            {
                textbox_EpisodeDurationHourImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri(filePath, UriKind.Relative)
                    );
            }
            textbox_EpisodeDurationHourImageBrush.AlignmentX = AlignmentX.Left;
            textbox_EpisodeDurationHourImageBrush.Stretch = Stretch.None;

            filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\Textbox_Background_Minute.png";
            // Create an ImageBrush for empty EpisodDurationMinute-textbox.
            if (File.Exists(filePath))
            {
                textbox_EpisodeDurationMinuteImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri(filePath, UriKind.Relative)
                    );
            }
            textbox_EpisodeDurationMinuteImageBrush.AlignmentX = AlignmentX.Left;
            textbox_EpisodeDurationMinuteImageBrush.Stretch = Stretch.None;

            filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\Textbox_Background_Second.png";
            // Create an ImageBrush for empty EpisodDurationSecond-textbox.
            if (File.Exists(filePath))
            {
                textbox_EpisodeDurationSecondImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri(filePath, UriKind.Relative)
                    );
            }
            textbox_EpisodeDurationSecondImageBrush.AlignmentX = AlignmentX.Left;
            textbox_EpisodeDurationSecondImageBrush.Stretch = Stretch.None;

            seasonCount = 0;

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

        public bool CheckValues()
        {
            bool result = true;

            bool missingAnimeName = true;
            bool missingAnimeDescription = true;
            bool missingGenre = true;

            // check for AnimeName
            for (int i = 0; i < anime.Names.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(anime.Names[i].Value))
                {
                    missingAnimeName = false;
                    break;
                }
            }

            // check for AnimeDescription
            for (int i = 0; i < anime.Descriptions.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(anime.Descriptions[i].Value))
                {
                    missingAnimeDescription = false;
                    break;
                }
            }

            // check for Main
            if (anime.MainGenres.Count > 0 || anime.SubGenres.Count > 0)
            {
                missingGenre = false;
            }

            // check for missing SeasonName
            for (int i = 0; i < anime.Seasons.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(anime.Seasons[i].Name))
                {
                    // show message
                    OwnMessageBox mb = new OwnMessageBox();
                    mb.SetElements(OwnMessageboxTyp.WARNING, languageDetail.MissingSeasonName + i);
                    mb.SetLanguage(languageDetail);
                    mb.ShowDialog();

                    result = false;
                }
            }

            // check for missing EpisodeName
            for (int i = 0; i < anime.Seasons.Count; i++)
            {
                for (int j = 0; j < anime.Seasons[i].Episodes.Count; j++)
                {
                    for (int k = 0; k < anime.Seasons[i].Episodes[j].Names.Count; k++)
                    {
                        if (string.IsNullOrWhiteSpace(anime.Seasons[i].Episodes[j].Names[k].Value))
                        {
                            string message = string.Format(languageDetail.MissingEpisodeName, i, j);

                            // show message
                            OwnMessageBox mb = new OwnMessageBox();
                            mb.SetElements(OwnMessageboxTyp.WARNING, message);
                            mb.SetLanguage(languageDetail);
                            mb.Show();

                            result = false;
                        }
                    }
                }
            }

            if (missingAnimeName)
            {
                // show message
                OwnMessageBox mb = new OwnMessageBox();
                mb.SetElements(OwnMessageboxTyp.WARNING, languageDetail.MissingAnimeName);
                mb.SetLanguage(languageDetail);
                mb.ShowDialog();

                result = false;
            }
            if (missingAnimeDescription)
            {
                // show message
                OwnMessageBox mb = new OwnMessageBox();
                mb.SetElements(OwnMessageboxTyp.WARNING, languageDetail.MissingAnimeDescription);
                mb.SetLanguage(languageDetail);
                mb.ShowDialog();

                result = false;
            }
            if (missingGenre)
            {
                // show message
                OwnMessageBox mb = new OwnMessageBox();
                mb.SetElements(OwnMessageboxTyp.WARNING, languageDetail.MissingGenre);
                mb.SetLanguage(languageDetail);
                mb.ShowDialog();

                result = false;
            }

            return result;
        }

        #region Prepare-Functions

        public void SetLanguage(LanguageDetail _languageDetail)
        {
            // get all elements for change Language
            Label label_AnimeName = (Label)this.FindName("label_AnimeName");
            Label label_Description = (Label)this.FindName("label_Description");
            Label label_Productionyear = (Label)this.FindName("label_Productionyears");
            Label label_MainActors = (Label)this.FindName("label_MainActors");
            Label label_Producers = (Label)this.FindName("label_Producers");
            Label label_Directors = (Label)this.FindName("label_Directors");
            Label label_Authors = (Label)this.FindName("label_Authors");
            Button button_Choose = (Button)this.FindName("button_Choose");
            Button button_Done = (Button)this.FindName("button_Done");
            Button button_Abort = (Button)this.FindName("button_Abort");
            Button button_AddMainActor = (Button)this.FindName("button_AddMainActor");
            Button button_AddProducer = (Button)this.FindName("button_AddProducer");
            Button button_AddDirector = (Button)this.FindName("button_AddDirector");
            Button button_AddAuthor = (Button)this.FindName("button_AddAuthor");

            // set right texts
            label_AnimeName.Content = _languageDetail.LabelName;
            label_Description.Content = _languageDetail.LabelDescription;
            label_Productionyear.Content = _languageDetail.LabelProductionyears;
            label_MainActors.Content = _languageDetail.LabelMainActors;
            label_Producers.Content = _languageDetail.LabelProducers;
            label_Directors.Content = _languageDetail.LabelDirectors;
            label_Authors.Content = _languageDetail.LabelAuthors;
            button_Choose.Content = _languageDetail.ButtonChoose;
            button_Done.Content = _languageDetail.ButtonDone;
            button_Abort.Content = _languageDetail.ButtonAbort;
            button_AddMainActor.Content = _languageDetail.ButtonAdd;
            button_AddProducer.Content = _languageDetail.ButtonAdd;
            button_AddDirector.Content = _languageDetail.ButtonAdd;
            button_AddAuthor.Content = _languageDetail.ButtonAdd;

            languageDetail = _languageDetail;
        }

        public void FillLanguageCombobox(string _name)
        {
            ComboBox comboBox = (ComboBox)this.FindName(_name);

            string[] names = _name.Split("_");

            // for each language in this system
            for (int i = 0; i < MainWindow.LanguageDetails.Count; i++)
            {
                // create new combobox item
                ComboBoxItem cbItem = new ComboBoxItem();

                // create new dockpanel
                DockPanel dockPanel = new DockPanel();

                string path = AppDomain.CurrentDomain.BaseDirectory + MainWindow.LanguageDetails[i].PicturePath_Complete;

                // create new image for icon
                Image icon = new Image()
                {
                    Width = 20,
                };
                if (File.Exists(path))
                {
                    icon.Source = new BitmapImage(new Uri(path));
                }
                DockPanel.SetDock(icon, Dock.Left);

                // create new label for language name
                Label text = new Label()
                {
                    Padding = new Thickness(0, -3, 0, 0),
                    Margin = new Thickness(5, 0, 0, 0),
                    Content = MainWindow.LanguageDetails[i].Name,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                DockPanel.SetDock(text, Dock.Left);

                // add icon and text to dockpanel
                dockPanel.Children.Add(icon);
                dockPanel.Children.Add(text);

                // add dockpanel to comboboxitem
                cbItem.Content = dockPanel;

                // add comboboxItem to Combobox
                comboBox.Items.Add(cbItem);

                // set language
                if (names[1] != "Dub" && names[1] != "Sub")
                    if (text.Content.ToString() == MainWindow.Setting)
                    {
                        comboBox.SelectedIndex = comboBox.Items.Count - 1;
                    }
            }
        }

        public void FillGenreChecklist()
        {
            // fill maingenre checklist
            for (int i = 0; i < MainWindow.GenreList.Main.Count; i++)
            {
                // create new CheckBox
                CheckBox GenreCheckbox = new CheckBox();
                GenreCheckbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                GenreCheckbox.Click += OnSelectGenre;
                GenreCheckbox.Tag = "MainGenre";

                // set Dock on DockPanel
                DockPanel.SetDock(GenreCheckbox, Dock.Top);

                // set right name
                GenreCheckbox.Content = MainWindow.GenreList.Main[i];

                // if its the last checkbox
                if (i == MainWindow.GenreList.Main.Count - 1)
                {
                    // no margin
                    GenreCheckbox.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    // 5 margin at the bottom
                    GenreCheckbox.Margin = new Thickness(0, 0, 0, 5);
                }
                // add to dockpanel at maingenre chocklist
                dockpanel_MainGenre.Children.Add(GenreCheckbox);
            }

            // fill subgenre checklist
            for (int i = 0; i < MainWindow.GenreList.Sub.Count; i++)
            {
                // create new CheckBox
                CheckBox GenreCheckbox = new CheckBox();
                GenreCheckbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                GenreCheckbox.Click += OnSelectGenre;
                GenreCheckbox.Tag = "SubGenre";

                // set Dock on DockPanel
                DockPanel.SetDock(GenreCheckbox, Dock.Top);

                // set right name
                GenreCheckbox.Content = MainWindow.GenreList.Sub[i];

                // if its the last checkbox
                if (i == MainWindow.GenreList.Sub.Count - 1)
                {
                    // no margin
                    GenreCheckbox.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    // 5 margin at the bottom
                    GenreCheckbox.Margin = new Thickness(0, 0, 0, 5);
                }
                // add to dockpanel at maingenre chocklist
                dockpanel_SubGenre.Children.Add(GenreCheckbox);
            }
        }

        public void SetAnimecount(int _count)
        {
            Button b = (Button)this.FindName("button_Done");
            b.Tag = _count.ToString();
        }

        public void FillValues(Anime _anime)
        {
            // set anime as editing anime
            anime = _anime;

            // genres
            for (int i = 0; i < anime.MainGenres.Count; i++)
            {
                for (int j = 0; j < dockpanel_MainGenre.Children.Count; j++)
                {
                    CheckBox cb = (CheckBox)dockpanel_MainGenre.Children[j];
                    if (anime.MainGenres[i] == cb.Content.ToString())
                    {
                        cb.IsChecked = true;
                    }
                }
            }
            for (int i = 0; i < anime.SubGenres.Count; i++)
            {
                for (int j = 0; j < dockpanel_SubGenre.Children.Count; j++)
                {
                    CheckBox cb = (CheckBox)dockpanel_SubGenre.Children[j];
                    if (anime.SubGenres[i] == cb.Content.ToString())
                    {
                        cb.IsChecked = true;
                    }
                }
            }

            // seasons
        }
        #endregion

        #region Add-Functions
        public void AddSeason(object sender, RoutedEventArgs e)
        {
            // get index
            int index = int.Parse(((Label)sender).Tag.ToString());

            // set new Tag for addseason label
            ((Label)sender).Tag = (index + 1).ToString();

            // create label for tabname
            Label label = new Label
            {
                Content = seasonCount + 1,
                Foreground = new SolidColorBrush(Colors.White)
            };

            // set register name of label
            RegisterName("label_SeasonName_" + index, label);

            // create dockpanel
            DockPanel dockPanel = new DockPanel
            {
                LastChildFill = false,
                Margin = new Thickness(5)
            };

            // set register name of dockpanel
            RegisterName("dockpanel_Season_" + index, dockPanel);

            // create AddEpisodeButton
            Button button = new Button
            {
                Tag = index.ToString() + "_0",
                Content = languageDetail.ButtonAddEpisode,
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x2D)),
                Foreground = new SolidColorBrush(Colors.White)
            };

            // set register name of button
            RegisterName("button_AddEpisode_" + index, button);

            // add click event to the button
            button.Click += AddEpisode;

            // set dockpanel.dock of button
            DockPanel.SetDock(button, Dock.Top);

            // create new textbox
            TextBox textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 5),
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                Foreground = new SolidColorBrush(Colors.White),
                Tag = index,
                Text = (seasonCount + 1).ToString()
            };
            // add textchangedeventhandler
            textBox.TextChanged += OnChangeSeasonName;

            // set register name of textbox
            RegisterName("textbox_SeasonName_" + index, textBox);

            // set dockpanel.dock of textbox
            DockPanel.SetDock(textBox, Dock.Top);

            // add textbox and button to dockpanel
            dockPanel.Children.Add(textBox);
            dockPanel.Children.Add(button);

            // Create new grid
            Grid gridSeason = new Grid();

            // create new columndefinitions
            ColumnDefinition c1 = new ColumnDefinition();
            ColumnDefinition c2 = new ColumnDefinition();

            //add columndefinition to the grid
            gridSeason.ColumnDefinitions.Add(c1);
            gridSeason.ColumnDefinitions.Add(c2);

            // create new Button
            Button buttonRemoveSeason = new Button()
            {
                Content = "X",
                Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF)),
                Foreground = new SolidColorBrush(Colors.Wheat),
                Tag = index,
                BorderThickness = new Thickness(0),
                Width = 19,
                Height = 19,
                Margin = new Thickness(5, 0, 5, 0)
            };
            buttonRemoveSeason.Click += RemoveSeason;
            Grid.SetColumn(buttonRemoveSeason, 1);

            // add laben and button to the grid
            gridSeason.Children.Add(label);
            gridSeason.Children.Add(buttonRemoveSeason);

            // create new tabitem
            TabItem tabItem = new TabItem
            {
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Header = gridSeason,
                Content = dockPanel
            };
            RegisterName("tabitem_Season_" + index, tabItem);

            tabcontrol_EpisodeList.Items.Insert(tabcontrol_EpisodeList.Items.Count - 1, tabItem);

            // create new season
            Season season = new Season();
            season.Dubs = new List<Information>();
            season.Subs = new List<Information>();
            // add new list of episodes into the list
            season.Episodes = new List<Episode>();
            // add season to the anime
            anime.Seasons.Add(season);

            // increase seasoncounter
            seasonCount++;

            // set seasonname
            anime.Seasons[anime.Seasons.Count - 1].Name = textBox.Text;
        }

        private void AddEpisode(object sender, RoutedEventArgs e)
        {
            // get index from button-tag
            string[] index = ((Button)sender).Tag.ToString().Split("_");

            // create new Episode
            int indexSeason = int.Parse(index[0]);
            int indexEpisode = int.Parse(index[1]);

            // create new episode
            Episode episode = new Episode();
            episode.Subs = new List<string>();
            episode.Dubs = new List<string>();

            // find right dockpanel
            DockPanel dockPanel = (DockPanel)this.FindName("dockpanel_Season_" + indexSeason);

            // find right addbutton
            Button addbutton = (Button)this.FindName("button_AddEpisode_" + indexSeason);

            #region Create Episode-Edit-Block
            // create maingrid with right settings                                                                                                     
            Grid episodeBlock = new Grid();
            episodeBlock.Margin = new Thickness(0, 0, 0, 5);
            episodeBlock.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x2D));
            DockPanel.SetDock(episodeBlock, Dock.Top);
            RegisterName("grid_Episode_" + indexSeason + "_" + indexEpisode, episodeBlock);

            #region MainDockPanel                                                                                                                      
            // create maindockpanel                                                                                                                    
            DockPanel dockPanelEpisode = new DockPanel();
            dockPanelEpisode.Margin = new Thickness(5);
            RegisterName("dockpanel_Episode_" + indexSeason + "_" + indexEpisode, dockPanelEpisode);


            episodeBlock.Children.Add(dockPanelEpisode);

            #region DockPanel for EpisodeName
            // create dockpanel for episode name                                                                                                       
            DockPanel dockPanelEpisodeName = new DockPanel();
            DockPanel.SetDock(dockPanelEpisodeName, Dock.Top);
            dockPanelEpisodeName.LastChildFill = true;


            Label labelEpisodeName = new Label();
            labelEpisodeName.Content = languageDetail.LabelEpisode;
            labelEpisodeName.Foreground = new SolidColorBrush(Colors.White);
            DockPanel.SetDock(labelEpisodeName, Dock.Left);


            Grid gridEpisodeLanguage = new Grid();
            gridEpisodeLanguage.Height = 35;
            DockPanel.SetDock(gridEpisodeLanguage, Dock.Left);
            // create all Grid.ColumnDefenitions                                                                                                       
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(210, GridUnitType.Pixel);
            // add all columndefenitions to the grid                                                                                                   
            gridEpisodeLanguage.ColumnDefinitions.Add(c1);
            gridEpisodeLanguage.ColumnDefinitions.Add(c2);

            #region helpergrid for backgroundcolor behind textbox-background-picture

            // create helpergrid for backgroundcolor behind textbox-background-picture                                                                  
            Grid helperGrid = new Grid();
            Grid.SetColumn(helperGrid, 0);
            helperGrid.Height = 20;
            helperGrid.Margin = new Thickness(0, 0, 5, 0);
            helperGrid.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));


            TextBox textBoxEpisodeName = new TextBox();
            textBoxEpisodeName.Tag = index[0] + "_" + index[1];
            textBoxEpisodeName.Background = textbox_EpisodeNameImageBrush;
            textBoxEpisodeName.Height = 20;
            textBoxEpisodeName.TextChanged += OnEpisodenameChanged;
            textBoxEpisodeName.Foreground = new SolidColorBrush(Colors.White);
            RegisterName("textbox_EpisodeName_" + indexSeason + "_" + indexEpisode, textBoxEpisodeName);

            helperGrid.Children.Add(textBoxEpisodeName);

            #endregion

            // create Combobox for EpisodeName for all languages
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 200;
            comboBox.Height = 25;
            comboBox.SelectionChanged += OnSelectionChanged;
            comboBox.Name = "combobox_EpisodeName_" + indexSeason + "_" + indexEpisode;
            Grid.SetColumn(comboBox, 1);
            RegisterName("combobox_EpisodeName_" + indexSeason + "_" + indexEpisode, comboBox);

            gridEpisodeLanguage.Children.Add(helperGrid);
            gridEpisodeLanguage.Children.Add(comboBox);

            // add all subelements to dockpanel for episodename
            dockPanelEpisodeName.Children.Add(labelEpisodeName);
            dockPanelEpisodeName.Children.Add(gridEpisodeLanguage);

            #endregion

            #region DockPanel for EpisodeDescription

            // create dockpanel for episode description                                                                                                
            DockPanel dockPanelEpisodeDescription = new DockPanel();
            DockPanel.SetDock(dockPanelEpisodeDescription, Dock.Top);
            dockPanelEpisodeDescription.Margin = new Thickness(0, 5, 0, 5);
            dockPanelEpisodeDescription.Height = 150;


            Label labelEpisodeDescription = new Label();
            labelEpisodeDescription.Foreground = new SolidColorBrush(Colors.White);
            labelEpisodeDescription.Content = languageDetail.LabelDescription;


            TextBox textBoxEpisodeDescription = new TextBox();
            textBoxEpisodeDescription.Tag = indexSeason + "_" + indexEpisode;
            textBoxEpisodeDescription.AcceptsReturn = true;
            textBoxEpisodeDescription.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
            textBoxEpisodeDescription.Foreground = new SolidColorBrush(Colors.White);
            textBoxEpisodeDescription.TextChanged += OnEpisodeDescriptionChanged;
            RegisterName("textbox_EpisodeDescription_" + indexSeason + "_" + indexEpisode, textBoxEpisodeDescription);


            dockPanelEpisodeDescription.Children.Add(labelEpisodeDescription);
            dockPanelEpisodeDescription.Children.Add(textBoxEpisodeDescription);

            #endregion

            #region DockPanel for sub and dub
            // . .
            // create dockpanel for sub and dub                                                                                                        
            DockPanel dockPanelSubDub = new DockPanel();
            dockPanelSubDub.MinHeight = 32;
            dockPanelSubDub.Margin = new Thickness(0, 5, 0, 5);
            dockPanelSubDub.LastChildFill = false;
            DockPanel.SetDock(dockPanelSubDub, Dock.Top);

            Label labelDub = new Label();
            DockPanel.SetDock(labelDub, Dock.Left);
            labelDub.Content = "Dub:";
            labelDub.Foreground = new SolidColorBrush(Colors.White);

            // create DockPanel for DubList
            DockPanel dockPanelDub = new DockPanel();
            dockPanelDub.Width = 210;
            dockPanelDub.Name = "dockpanel_Dub_" + indexSeason + "_" + indexEpisode;
            dockPanelDub.LastChildFill = false;
            DockPanel.SetDock(dockPanelDub, Dock.Left);
            RegisterName("dockpanel_Dub_" + indexSeason + "_" + indexEpisode, dockPanelDub);

            // create Combobox for all languages
            ComboBox comboBoxDub = new ComboBox();
            comboBoxDub.Width = 200;
            comboBoxDub.Height = 25;
            comboBoxDub.SelectionChanged += OnSelectionChanged;
            comboBoxDub.Name = "combobox_Dub_" + indexSeason + "_" + indexEpisode;
            DockPanel.SetDock(comboBoxDub, Dock.Bottom);
            RegisterName("combobox_Dub_" + indexSeason + "_" + indexEpisode, comboBoxDub);

            // fill this combobox
            FillLanguageCombobox("combobox_Dub_" + indexSeason + "_" + indexEpisode);

            // add comboboxDub to dockPanelDub
            dockPanelDub.Children.Add(comboBoxDub);

            Label labelSub = new Label();
            DockPanel.SetDock(labelSub, Dock.Left);
            labelSub.Content = "Sub:";
            labelSub.Foreground = new SolidColorBrush(Colors.White);
            labelSub.Margin = new Thickness(50, 0, 0, 0);

            // create DockPanel for SubList
            DockPanel dockPanelSub = new DockPanel();
            dockPanelSub.Width = 210;
            dockPanelSub.Name = "dockpanel_Sub_" + indexSeason + "_" + indexEpisode;
            dockPanelSub.LastChildFill = false;
            DockPanel.SetDock(dockPanelSub, Dock.Left);
            RegisterName("dockpanel_Sub_" + indexSeason + "_" + indexEpisode, dockPanelSub);

            // create Combobox for all languages
            ComboBox comboBoxSub = new ComboBox();
            comboBoxSub.Width = 200;
            comboBoxSub.Height = 25;
            comboBoxSub.SelectionChanged += OnSelectionChanged;
            comboBoxSub.Name = "combobox_Sub_" + indexSeason + "_" + indexEpisode;
            DockPanel.SetDock(comboBoxSub, Dock.Bottom);
            RegisterName("combobox_Sub_" + indexSeason + "_" + indexEpisode, comboBoxSub);

            // fill this combobox
            FillLanguageCombobox("combobox_Sub_" + indexSeason + "_" + indexEpisode);

            // add comboboxSub to dockpanelSub
            dockPanelSub.Children.Add(comboBoxSub);

            dockPanelSubDub.Children.Add(labelDub);
            dockPanelSubDub.Children.Add(dockPanelDub);
            dockPanelSubDub.Children.Add(labelSub);
            dockPanelSubDub.Children.Add(dockPanelSub);

            #endregion

            #region Duration and Delete

            DockPanel dockPanelDurationDelete = new DockPanel();
            dockPanelDurationDelete.LastChildFill = false;
            dockPanelDurationDelete.Margin = new Thickness(0, 5, 0, 0);
            DockPanel.SetDock(dockPanelDurationDelete, Dock.Top);

            Label labelDuration = new Label();
            labelDuration.Content = languageDetail.LabelDuration;
            labelDuration.Foreground = new SolidColorBrush(Colors.White);
            labelDuration.Margin = new Thickness(0, 0, 10, 0);
            DockPanel.SetDock(labelDuration, Dock.Left);

            TextBox textBoxDurationHour = new TextBox();
            textBoxDurationHour.MaxLength = 2;
            textBoxDurationHour.Width = 25;
            textBoxDurationHour.Height = 20;
            textBoxDurationHour.Background = textbox_EpisodeDurationHourImageBrush;
            textBoxDurationHour.Foreground = new SolidColorBrush(Colors.White);
            textBoxDurationHour.Tag = "H_" + indexSeason + "_" + indexEpisode;
            textBoxDurationHour.PreviewTextInput += OnDurationChanged;
            textBoxDurationHour.TextChanged += OnDurationChanged;
            RegisterName("textbox_EpisodeDuration_" + textBoxDurationHour.Tag.ToString(), textBoxDurationHour);

            Label labelHM = new Label();
            labelHM.Content = ":";
            labelHM.Foreground = new SolidColorBrush(Colors.White);
            labelHM.Margin = new Thickness(-4, 0, -4, 0);
            DockPanel.SetDock(labelHM, Dock.Left);

            TextBox textBoxDurationMinute = new TextBox();
            textBoxDurationMinute.MaxLength = 2;
            textBoxDurationMinute.Width = 25;
            textBoxDurationMinute.Height = 20;
            textBoxDurationMinute.Background = textbox_EpisodeDurationMinuteImageBrush;
            textBoxDurationMinute.Foreground = new SolidColorBrush(Colors.White);
            textBoxDurationMinute.Tag = "M_" + indexSeason + "_" + indexEpisode;
            textBoxDurationMinute.PreviewTextInput += OnDurationChanged;
            textBoxDurationMinute.TextChanged += OnDurationChanged;
            RegisterName("textbox_EpisodeDuration_" + textBoxDurationMinute.Tag.ToString(), textBoxDurationMinute);

            Label labelMS = new Label();
            labelMS.Content = ":";
            labelMS.Foreground = new SolidColorBrush(Colors.White);
            labelMS.Margin = new Thickness(-4, 0, -4, 0);
            DockPanel.SetDock(labelMS, Dock.Left);

            TextBox textBoxDurationSecond = new TextBox();
            textBoxDurationSecond.MaxLength = 2;
            textBoxDurationSecond.Width = 25;
            textBoxDurationSecond.Height = 20;
            textBoxDurationSecond.Background = textbox_EpisodeDurationSecondImageBrush;
            textBoxDurationSecond.Foreground = new SolidColorBrush(Colors.White);
            textBoxDurationSecond.Tag = "S_" + indexSeason + "_" + indexEpisode;
            textBoxDurationSecond.PreviewTextInput += OnDurationChanged;
            textBoxDurationSecond.TextChanged += OnDurationChanged;
            RegisterName("textbox_EpisodeDuration_" + textBoxDurationSecond.Tag.ToString(), textBoxDurationSecond);

            Button buttonRemoveEpisode = new Button();
            buttonRemoveEpisode.Content = languageDetail.ButtonRemove;
            buttonRemoveEpisode.Background = new SolidColorBrush(Colors.Red);
            buttonRemoveEpisode.Foreground = new SolidColorBrush(Colors.Wheat);
            buttonRemoveEpisode.Tag = indexSeason + "_" + indexEpisode;
            buttonRemoveEpisode.Click += RemoveEpisode;
            DockPanel.SetDock(buttonRemoveEpisode, Dock.Right);

            dockPanelDurationDelete.Children.Add(labelDuration);
            dockPanelDurationDelete.Children.Add(textBoxDurationHour);
            dockPanelDurationDelete.Children.Add(labelHM);
            dockPanelDurationDelete.Children.Add(textBoxDurationMinute);
            dockPanelDurationDelete.Children.Add(labelMS);
            dockPanelDurationDelete.Children.Add(textBoxDurationSecond);
            dockPanelDurationDelete.Children.Add(buttonRemoveEpisode);

            #endregion

            // add dockpanel for episodename to maindockpanel of episodeblock                                                                           
            dockPanelEpisode.Children.Add(dockPanelEpisodeName);
            dockPanelEpisode.Children.Add(dockPanelEpisodeDescription);
            dockPanelEpisode.Children.Add(dockPanelSubDub);
            dockPanelEpisode.Children.Add(dockPanelDurationDelete);
            #endregion

            #endregion

            // add new episode
            dockPanel.Children.Insert(dockPanel.Children.IndexOf(addbutton), episodeBlock);

            indexEpisode++;

            string newTag = indexSeason + "_" + indexEpisode;

            ((Button)sender).Tag = newTag;

            // add episode to Anime
            anime.Seasons[indexSeason].Episodes.Add(episode);

            // fill this combobox
            FillLanguageCombobox(comboBox.Name);
        }

        private void AddMoreDetails(object sender, RoutedEventArgs e)
        {
            // get tag splited
            string[] tmps = ((Button)sender).Tag.ToString().Split("_");

            string typ = tmps[0];
            int index = int.Parse(tmps[1]);

            // get right textbox
            TextBox textbox = (TextBox)this.FindName("textbox_" + typ);

            // create new dockpanel that contains removebutton and label with right value
            DockPanel dockpanel = new DockPanel
            {
                // set name
                Name = "dockpanel_" + typ + "_" + index
            };
            DockPanel.SetDock(dockpanel, Dock.Top);
            RegisterName(dockpanel.Name, dockpanel);

            // create removebutton
            Button removebutton = new Button
            {
                Name = "button_Remove_" + typ + "_" + index,
                Width = 20,
                Height = 20,
                Content = "-",
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0x1D, 0x1D)),
                BorderThickness = new Thickness(0, 0, 0, 0),
                Tag = typ + "_" + index
            };
            removebutton.Click += RemoveMoreDetails;
            DockPanel.SetDock(removebutton, Dock.Left);
            RegisterName(removebutton.Name, removebutton);

            // create Label
            Label label = new Label
            {
                Name = "label_" + typ + "_" + index,
                Foreground = new SolidColorBrush(Colors.White),
                Content = textbox.Text,
                Padding = new Thickness(5, 0, 5, 2)
            };
            RegisterName(label.Name, label);
            DockPanel.SetDock(label, Dock.Left);

            // add removebutton and label to dockpanel
            dockpanel.Children.Add(removebutton);
            dockpanel.Children.Add(label);

            // get right dockpanel
            DockPanel motherDockpanel = (DockPanel)this.FindName("dockpanel_" + typ + "s");

            // add dockpanel to motherDockpanel
            motherDockpanel.Children.Add(dockpanel);

            // increase index
            index++;

            // set new tag for current button
            ((Button)sender).Tag = typ + "_" + index.ToString();

            // reset text from textbox
            textbox.Text = "";

            // add value for right object
            if (typ == "MainActor")
            {
                anime.MainActors.Add(label.Content.ToString());
            }
            if (typ == "Producer")
            {
                anime.Producers.Add(label.Content.ToString());
            }
            if (typ == "Director")
            {
                anime.Directors.Add(label.Content.ToString());
            }
            if (typ == "Author")
            {
                anime.Authors.Add(label.Content.ToString());
            }
        }

        private void AddAnime(object sender, RoutedEventArgs e)
        {

            int durationHour = 0;
            int durationMinute = 0;
            int durationSecond = 0;
            int tmp;

            // create list for subs and dubs
            List<Information> AnimeDubs = new List<Information>();
            List<Information> AnimeSubs = new List<Information>();

            List<Season> seasonToRemove = new List<Season>();

            // go thorught every season
            foreach (Season s in anime.Seasons)
            {
                if (s != null)
                {
                    // create list for subs and dubs
                    List<Information> SeasonDubs = new List<Information>();
                    List<Information> SeasonSubs = new List<Information>();

                    // list of all indeces with deleted episode
                    List<int> indeces = new List<int>();

                    int seasonDurationHour = 0;
                    int seasonDurationMinute = 0;
                    int seasonDurationSecond = 0;

                    for (int i = 0; i < s.Episodes.Count; i++)
                    {
                        if (s.Episodes[i] == null)
                        {
                            // add index
                            indeces.Add(i);
                        }
                        else
                        {
                            // list of all indeces with deletet dubs
                            List<int> indecesDub = new List<int>();
                            // list of all indeces with deletet subs
                            List<int> indecesSub = new List<int>();

                            // go through all dubs
                            for (int j = 0; j < s.Episodes[i].Dubs.Count; j++)
                            {
                                // if deleted dub found add index to list
                                if (s.Episodes[i].Dubs[j] == "0")
                                    indecesDub.Add(j);
                                else
                                {
                                    // if dubslist is empty
                                    if (SeasonDubs.Count == 0)
                                    {
                                        // create new dubinformation
                                        Information dub = new Information();
                                        dub.Name = s.Episodes[i].Dubs[j];
                                        dub.Value = "1";

                                        //ad dubinformation
                                        SeasonDubs.Add(dub);
                                    }
                                    else
                                    {
                                        // go through Informationlist of Dubs
                                        for (int a = 0; a < SeasonDubs.Count; a++)
                                        {
                                            // if dub is already in the list
                                            if (SeasonDubs[a].Name == s.Episodes[i].Dubs[j])
                                            {
                                                // create new dubinformation
                                                Information dub = new Information();
                                                dub.Name = SeasonDubs[a].Name;
                                                // increase counter
                                                dub.Value = (int.Parse(SeasonDubs[a].Value) + 1).ToString();

                                                // replace dubinformaion
                                                SeasonDubs[a] = dub;
                                                break;
                                            }
                                            // if last iteration
                                            if (a == SeasonDubs.Count - 1)
                                            {
                                                // create new dubinformation
                                                Information dub = new Information();
                                                dub.Name = s.Episodes[i].Dubs[j];
                                                dub.Value = "1";

                                                // add dubinformation
                                                SeasonDubs.Add(dub);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            // go through all subs
                            for (int k = 0; k < s.Episodes[i].Subs.Count; k++)
                            {
                                // if deleted sub found add index to list
                                if (s.Episodes[i].Subs[k] == "0")
                                    indecesSub.Add(k);
                                else
                                {
                                    // if subslist is empty
                                    if (SeasonSubs.Count == 0)
                                    {
                                        // create new subinformation
                                        Information sub = new Information();
                                        sub.Name = s.Episodes[i].Subs[k];
                                        sub.Value = "1";

                                        //ad subinformation
                                        SeasonSubs.Add(sub);
                                    }
                                    else
                                    {
                                        // go through Informationlist of Subs
                                        for (int a = 0; a < SeasonSubs.Count; a++)
                                        {
                                            // if sub is already in the list
                                            if (SeasonSubs[a].Name == s.Episodes[i].Subs[k])
                                            {
                                                // create new subinformation
                                                Information sub = new Information();
                                                sub.Name = SeasonSubs[a].Name;
                                                // increase counter
                                                sub.Value = (int.Parse(SeasonSubs[a].Value) + 1).ToString();

                                                // replace dubinformaion
                                                SeasonSubs[a] = sub;
                                                break;
                                            }
                                            // if last iteration
                                            if (a == SeasonSubs.Count - 1)
                                            {
                                                // create new dubinformation
                                                Information sub = new Information();
                                                sub.Name = s.Episodes[i].Subs[k];
                                                sub.Value = "1";

                                                // add dubinformation
                                                SeasonSubs.Add(sub);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            // go through all deleted dubs
                            for (int l = indecesDub.Count - 1; l > -1; l--)
                            {
                                // remove sub
                                s.Episodes[i].Dubs.RemoveAt(indecesDub[l]);
                            }
                            // go through all deleted subs
                            for (int m = indecesSub.Count - 1; m > -1; m--)
                            {
                                // remove dub
                                s.Episodes[i].Subs.RemoveAt(indecesSub[m]);
                            }

                            // sort sub and dub lists
                            s.Episodes[i].Dubs.Sort();
                            s.Episodes[i].Subs.Sort();

                            // increase duration
                            seasonDurationHour += s.Episodes[i].Duration_Hours;
                            seasonDurationMinute += s.Episodes[i].Duration_Minutes;
                            seasonDurationSecond += s.Episodes[i].Duration_Seconds;
                        }
                    }

                    for (int j = 0; j < SeasonDubs.Count; j++)
                    {
                        // count of Dubs of current language
                        int count = int.Parse(SeasonDubs[j].Value);

                        // information for animedub
                        Information animedub = new Information();
                        animedub.Name = SeasonDubs[j].Name;

                        // information for seasondub
                        Information dub = new Information();
                        dub.Name = SeasonDubs[j].Name;

                        int value;

                        // if every episode is dubed in current language
                        if (count == s.Episodes.Count)
                        {
                            dub.Value = "Complete";
                            value = 1;
                        }
                        else
                        {
                            dub.Value = "Part";
                            value = 0;
                        }

                        // add dubinformation to season
                        s.Dubs.Add(dub);

                        // if no dubs in anime
                        if (AnimeDubs.Count == 0)
                        {
                            // set value
                            animedub.Value = value.ToString();
                            // add dub to anime
                            AnimeDubs.Add(animedub);
                        }
                        else
                        {
                            // go through all animedubs
                            for (int d = 0; d < AnimeDubs.Count; d++)
                            {
                                // if animedub with current language already exist
                                if (AnimeDubs[d].Name == animedub.Name)
                                {
                                    // create new informatiion
                                    Information newdub = new Information();
                                    newdub.Name = AnimeDubs[d].Name;
                                    newdub.Value = (int.Parse(AnimeDubs[d].Value) + value).ToString();

                                    // to replace old information with new value
                                    AnimeDubs[d] = newdub;
                                    break;
                                }

                                // last iteration language didnt found
                                if (d == AnimeDubs.Count - 1)
                                {
                                    // set value
                                    animedub.Value = value.ToString();
                                    // add dub to anime
                                    AnimeDubs.Add(animedub);
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < SeasonSubs.Count; j++)
                    {
                        // count of Subs of current language
                        int count = int.Parse(SeasonSubs[j].Value);

                        // information for animesub
                        Information animesub = new Information();
                        animesub.Name = SeasonSubs[j].Name;

                        Information sub = new Information();
                        sub.Name = SeasonSubs[j].Name;

                        int value;

                        // if every episode is dubed in current language
                        if (count == s.Episodes.Count)
                        {
                            sub.Value = "Complete";
                            value = 1;
                        }
                        else
                        {
                            sub.Value = "Part";
                            value = 0;
                        }

                        // add subinformation to season
                        s.Subs.Add(sub);

                        // if no subs in anime
                        if (AnimeSubs.Count == 0)
                        {
                            // set value
                            animesub.Value = value.ToString();
                            // add sub to anime
                            AnimeSubs.Add(animesub);
                        }
                        else
                        {
                            // go through all animesubs
                            for (int d = 0; d < AnimeSubs.Count; d++)
                            {
                                // if animesub with current language already exist
                                if (AnimeSubs[d].Name == animesub.Name)
                                {
                                    // create new informatiion
                                    Information newdub = new Information();
                                    newdub.Name = AnimeSubs[d].Name;
                                    newdub.Value = (int.Parse(AnimeSubs[d].Value) + value).ToString();

                                    // to replace old information with new value
                                    AnimeSubs[d] = newdub;
                                    break;
                                }

                                // last iteration language didnt found
                                if (d == AnimeSubs.Count - 1)
                                {
                                    // set value
                                    animesub.Value = value.ToString();
                                    // add sub to anime
                                    AnimeSubs.Add(animesub);
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = indeces.Count - 1; j > 0; j--)
                    {
                        // remove all deleted episodes
                        s.Episodes.RemoveAt(indeces[j]);
                    }

                    // recalculation duration
                    tmp = 0;
                    while (seasonDurationSecond > 59)
                    {
                        seasonDurationSecond -= 60;
                        tmp++;
                    }
                    seasonDurationMinute += tmp;
                    tmp = 0;
                    while (seasonDurationMinute > 59)
                    {
                        seasonDurationMinute -= 60;
                        tmp++;
                    }
                    seasonDurationHour += tmp;

                    // set duration of season
                    s.Duration_Hours = seasonDurationHour;
                    s.Duration_Minutes = seasonDurationMinute;
                    s.Duration_Seconds = seasonDurationSecond;

                    durationHour += seasonDurationHour;
                    durationMinute += seasonDurationMinute;
                    durationSecond += seasonDurationSecond;
                }
                else
                {
                    // add season to removing-season-list
                    seasonToRemove.Add(s);
                }
            }

            // remove all deleted seasons
            for (int i = 0; i < seasonToRemove.Count; i++)
            {
                anime.Seasons.Remove(seasonToRemove[i]);
            }

            if (CheckValues())
            {
                // reset increasing values and adding items into lists for checking
                anime.EpisodeCount = 0;
                anime.Subs.Clear();
                anime.Dubs.Clear();

                foreach (Season s in anime.Seasons)
                {
                    for (int i = 0; i < s.Episodes.Count; i++)
                    {
                        // increase episodecount
                        anime.EpisodeCount++;
                    }
                }

                // remove unused AnimeNameLanguages
                for (int i = anime.Names.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(anime.Names[i].Value))
                    {
                        anime.Names.RemoveAt(i);
                    }
                }

                // remove unused AnimeDescriptionLanguages
                for (int i = anime.Descriptions.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(anime.Descriptions[i].Value))
                    {
                        anime.Descriptions.RemoveAt(i);
                    }
                }

                // remove unused EpisodeNameLanguages
                for (int i = anime.Seasons.Count - 1; i >= 0; i--)
                {
                    for (int j = anime.Seasons[i].Episodes.Count - 1; j >= 0; j--)
                    {
                        for (int k = anime.Seasons[i].Episodes[j].Names.Count - 1; k >= 0; k--)
                        {
                            if (string.IsNullOrWhiteSpace(anime.Seasons[i].Episodes[j].Names[k].Value))
                            {
                                anime.Seasons[i].Episodes[j].Names.RemoveAt(k);
                            }
                        }
                    }
                }

                // remove unused EpisodeDescriptionsLanguages
                for (int i = anime.Seasons.Count - 1; i >= 0; i--)
                {
                    for (int j = anime.Seasons[i].Episodes.Count - 1; j >= 0; j--)
                    {
                        for (int k = anime.Seasons[i].Episodes[j].Names.Count - 1; k >= 0; k--)
                        {
                            if (string.IsNullOrWhiteSpace(anime.Seasons[i].Episodes[j].Names[k].Value))
                            {
                                anime.Seasons[i].Episodes[j].Names.RemoveAt(k);
                            }
                        }
                    }
                }

                // check for fulldub
                for (int d = 0; d < AnimeDubs.Count; d++)
                {
                    Information animedub = new Information();
                    animedub.Name = AnimeDubs[d].Name;

                    // if every season is dubed
                    if (AnimeDubs[d].Value == anime.Seasons.Count.ToString())
                    {
                        animedub.Value = "Complete";
                    }
                    // otherwise
                    else
                    {
                        animedub.Value = "Part";
                    }

                    // add dub to anime
                    anime.Dubs.Add(animedub);
                }

                // check for fullsub
                for (int d = 0; d < AnimeSubs.Count; d++)
                {
                    Information animesub = new Information();
                    animesub.Name = AnimeSubs[d].Name;

                    // if every season is dubed
                    if (AnimeSubs[d].Value == anime.Seasons.Count.ToString())
                    {
                        animesub.Value = "Complete";
                    }
                    // otherwise
                    else
                    {
                        animesub.Value = "Part";
                    }

                    // add dub to anime
                    anime.Subs.Add(animesub);
                }

                // recalculation duration
                tmp = 0;
                while (durationSecond > 59)
                {
                    durationSecond -= 60;
                    tmp++;
                }
                durationMinute += tmp;
                tmp = 0;
                while (durationMinute > 59)
                {
                    durationMinute -= 60;
                    tmp++;
                }
                durationHour += tmp;

                // set duration
                anime.Duration_Hours = durationHour;
                anime.Duration_Minutes = durationMinute;
                anime.Duration_Seconds = durationSecond;

                // get index
                int index = int.Parse((((Button)sender).Tag).ToString());

                #region Sort Sub and Dublists of anime
                // create stringlists to sort by name
                List<string> dubSort = new List<string>();
                List<string> subSort = new List<string>();
                // go through all Dubs
                for (int i = 0; i < anime.Dubs.Count; i++)
                {
                    // add name of dub
                    dubSort.Add(anime.Dubs[i].Name);
                }
                // go through all Subs
                for (int i = 0; i < anime.Subs.Count; i++)
                {
                    // add name of sub
                    subSort.Add(anime.Subs[i].Name);
                }
                // sort names
                dubSort.Sort();
                subSort.Sort();
                // create new Informatinlists for dub and sub
                List<Information> Dub = new List<Information>();
                List<Information> Sub = new List<Information>();
                for (int i = 0; i < dubSort.Count; i++)
                {
                    // create new information
                    Information newDub = new Information();
                    // set sorted right name
                    newDub.Name = dubSort[i];

                    // go through all dubs of anime
                    for (int j = 0; j < anime.Dubs.Count; j++)
                    {
                        // if name matched
                        if (anime.Dubs[j].Name == newDub.Name)
                        {
                            // set value of current dub
                            newDub.Value = anime.Dubs[j].Value;
                            // add to new Informationlist
                            Dub.Add(newDub);
                            break;
                        }
                    }
                }
                for (int i = 0; i < subSort.Count; i++)
                {
                    // create new information
                    Information newSub = new Information();
                    // set sorted right name
                    newSub.Name = subSort[i];

                    // go through all subs of anime
                    for (int j = 0; j < anime.Subs.Count; j++)
                    {
                        // if name matched
                        if (anime.Subs[j].Name == newSub.Name)
                        {
                            // set value of current sub
                            newSub.Value = anime.Subs[j].Value;
                            // add to new Informationlist
                            Sub.Add(newSub);
                            break;
                        }
                    }
                }
                // replace sub/dublists of anime with sorted lists
                anime.Dubs = Dub;
                anime.Subs = Sub;
                #endregion

                List<string> authors = new List<string>();
                List<string> directors = new List<string>();
                List<string> producers = new List<string>();
                List<string> mainActors = new List<string>();

                foreach (string s in anime.Authors)
                {
                    if (s != "-1")
                    {
                        authors.Add(s);
                    }
                }
                foreach (string s in anime.Directors)
                {
                    if (s != "-1")
                    {
                        directors.Add(s);
                    }
                }
                foreach (string s in anime.Producers)
                {
                    if (s != "-1")
                    {
                        producers.Add(s);
                    }
                }
                foreach (string s in anime.MainActors)
                {
                    if (s != "-1")
                    {
                        mainActors.Add(s);
                    }
                }

                // replace lists
                anime.Authors = authors;
                anime.Directors = directors;
                anime.Producers = producers;
                anime.MainActors = mainActors;

                // get productionyears
                TextBox productionYear1 = (TextBox)this.FindName("textbox_Productionyear1");
                TextBox productionYear2 = (TextBox)this.FindName("textbox_Productionyear2");

                // check and set value
                int productionyear;
                if (int.TryParse(productionYear1.Text, out productionyear))
                {
                    anime.Productionyear[0] = productionyear;
                }
                else
                {
                    anime.Productionyear[0] = 0;
                }
                if (int.TryParse(productionYear2.Text, out productionyear))
                {
                    anime.Productionyear[1] = productionyear;
                }
                else
                {
                    anime.Productionyear[1] = 0;
                }


                if (!MainWindow.CheckForExistingAnime(anime, MainWindow.Animes))
                {
                    if (EA == null)
                    {
                        // add anime to MainWindow
                        MainWindow.AddAnime(anime);
                    }
                    else
                    {
                        MainWindow.UnregisterElement(EA.ListTyp, EA.Window, EA.Anime, EA.Index);
                        MainWindow.AddAnime(EA.Anime, EA.AnimeList, EA.ListTyp, EA.Window, true, EA.Index);
                    }

                    // close addAnimeWindow
                    Close();
                }
                else
                {
                    OwnMessageBox mb = new OwnMessageBox();
                    mb.SetElements(OwnMessageboxTyp.WARNING_YES_NO, languageDetail.AnimeExists);
                    mb.SetLanguage(languageDetail);
                    mb.ShowDialog();

                    if (mb.MessageboxResult == OwnMessageboxResult.YES)
                    {
                        // add anime to MainWindow
                        MainWindow.AddAnime(anime);

                        // close addAnimeWindow
                        Close();
                    }
                }
            }
        }
        #endregion

        #region Onchange-Functions
        private void OnChangeSeasonName(object sender, TextChangedEventArgs e)
        {
            // get index
            int index = int.Parse(((TextBox)sender).Tag.ToString());

            // find right TextBox
            TextBox textBox = (TextBox)this.FindName("textbox_SeasonName_" + index);

            // find right label
            Label label = (Label)this.FindName("label_SeasonName_" + index);

            // change label content
            label.Content = textBox.Text;

            // set seasonname
            anime.Seasons[index].Name = textBox.Text;
        }

        private void OnTabSelected(object sender, RoutedEventArgs e)
        {
            // irgendwas machen, damit das letzte tabitem nicht selected ist, sondern das vorletzte aber ka wie
        }

        private void OnEpisodenameChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)this.FindName("textbox_EpisodeName_" + ((TextBox)sender).Tag);

            if (textBox.Text == "")
            {
                textBox.Background = textbox_EpisodeNameImageBrush;
            }
            else
            {
                textBox.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3D, 0x3D));
            }

            // get index
            string[] index = ((TextBox)sender).Tag.ToString().Split("_");
            // for season
            int indexSeason = int.Parse(index[0]);
            // for episode
            int indexEpisode = int.Parse(index[1]);

            // get right combobox
            // find combobox
            ComboBox cb_language = (ComboBox)this.FindName("combobox_EpisodeName_" + indexSeason + "_" + indexEpisode);

            // get right item
            ComboBoxItem cb_item = (ComboBoxItem)cb_language.Items[cb_language.SelectedIndex];

            // get dockpanel
            DockPanel dockPanel = (DockPanel)cb_item.Content;

            // get label
            Label label = (Label)dockPanel.Children[1];

            // go through all episodename-lanugages
            for (int i = 0; i < anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Count; i++)
            {
                // if right language found
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Names[i].Name == label.Content.ToString())
                {
                    // create new Information for name
                    Information info = new Information()
                    {
                        Name = label.Content.ToString(),
                        Value = ((TextBox)sender).Text
                    };

                    // remove current information
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Names.RemoveAt(i);
                    // add new information at the same index
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Insert(i, info);
                    break;
                }
            }
        }

        private void OnEpisodeDescriptionChanged(object sender, TextChangedEventArgs e)
        {
            // get index
            string[] index = ((TextBox)sender).Tag.ToString().Split("_");
            // for season
            int indexSeason = int.Parse(index[0]);
            // for episode
            int indexEpisode = int.Parse(index[1]);

            // get right combobox
            // find combobox
            ComboBox cb_language = (ComboBox)this.FindName("combobox_EpisodeName_" + indexSeason + "_" + indexEpisode);

            // get right item
            ComboBoxItem cb_item = (ComboBoxItem)cb_language.Items[cb_language.SelectedIndex];

            // get dockpanel
            DockPanel dockPanel = (DockPanel)cb_item.Content;

            // get label
            Label label = (Label)dockPanel.Children[1];

            // go through all episodename-lanugages
            for (int i = 0; i < anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Count; i++)
            {
                // if right language found
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions[i].Name == label.Content.ToString())
                {
                    // create new Information for name
                    Information info = new Information()
                    {
                        Name = label.Content.ToString(),
                        Value = ((TextBox)sender).Text
                    };

                    // remove current information
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.RemoveAt(i);
                    // add new information at the same index
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Insert(i, info);
                    break;
                }
            }
        }

        private void OnDurationChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tmp = (TextBox)sender;
            string index = tmp.Tag.ToString();

            if (((TextBox)sender).Text == "" || ((TextBox)sender).Text == null)
            {
                if (index[0] == 'H')
                    tmp.Background = textbox_EpisodeDurationHourImageBrush;
                else if (index[0] == 'M')
                    tmp.Background = textbox_EpisodeDurationMinuteImageBrush;
                else
                    tmp.Background = textbox_EpisodeDurationSecondImageBrush;
            }
        }

        private void OnDurationChanged(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(((TextBox)sender).Text + e.Text, sender);
        }

        private void OnChangeProduktionyear(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(((TextBox)sender).Text + e.Text, sender);
        }

        private bool IsNumeric(string str, object sender)
        {
            int i;
            TextBox tmp = (TextBox)sender;
            string[] stringtmp = tmp.Tag.ToString().Split("_");

            string indexTyp = stringtmp[0];

            if (indexTyp == "Year")
            {
                int indexYear = int.Parse(stringtmp[1]);

                if (int.TryParse(str, out i))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                int indexSeason = int.Parse(stringtmp[1]);
                int indexEpisode = int.Parse(stringtmp[2]);

                if (str == "" || str == null)
                {
                    if (indexTyp == "H")
                        tmp.Background = textbox_EpisodeDurationHourImageBrush;
                    else if (indexTyp == "M")
                        tmp.Background = textbox_EpisodeDurationMinuteImageBrush;
                    else
                        tmp.Background = textbox_EpisodeDurationSecondImageBrush;

                    return false;
                }

                if (int.TryParse(str, out i))
                {
                    // if its hourtextbox
                    if (indexTyp == "H")
                    {
                        if (i > 24)
                        {
                            tmp.Text = "24";
                            i = 24;
                        }

                        tmp.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4F, 0x4D, 0x4D));

                        // set duration
                        anime.Seasons[indexSeason].Episodes[indexEpisode].Duration_Hours = i;
                    }
                    // if its minute or seconds textbox
                    else
                    {
                        if (i >= 60)
                        {
                            tmp.Text = "59";
                            i = 59;
                        }

                        tmp.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4F, 0x4D, 0x4D));

                        if (indexTyp == "M")
                        {
                            // set duration
                            anime.Seasons[indexSeason].Episodes[indexEpisode].Duration_Minutes = i;
                        }

                        if (indexTyp == "S")
                        {
                            // set duration
                            anime.Seasons[indexSeason].Episodes[indexEpisode].Duration_Seconds = i;
                        }
                    }

                    return true;
                }
                else
                {
                    string[] tmps = ((TextBox)sender).Text.Split();

                    if (!int.TryParse(tmps[0], out i))
                    {
                        if (indexTyp == "H")
                            tmp.Background = textbox_EpisodeDurationHourImageBrush;
                        else if (indexTyp == "M")
                            tmp.Background = textbox_EpisodeDurationMinuteImageBrush;
                        else
                            tmp.Background = textbox_EpisodeDurationSecondImageBrush;
                    }

                    return false;
                }
            }
        }

        private void OnAnimeNameChanged(object sender, TextCompositionEventArgs e)
        {
            // find combobox
            ComboBox cb_language = (ComboBox)this.FindName("combobox_AnimeNameDescription");

            // get right item
            ComboBoxItem cb_item = (ComboBoxItem)cb_language.Items[cb_language.SelectedIndex];

            // get dockpanel
            DockPanel dockPanel = (DockPanel)cb_item.Content;

            // get label
            Label label = (Label)dockPanel.Children[1];

            if (anime.Names == null)
            {
                anime.Names = new List<Information>();
            }

            // if this anime has Names
            if (anime.Names.Count > 0)
            {
                for (int i = 0; i < anime.Names.Count; i++)
                {
                    // if right language found
                    if (anime.Names[i].Name == label.Content.ToString())
                    {
                        // create new Information for name
                        Information info = new Information()
                        {
                            Name = label.Content.ToString(),
                            Value = ((TextBox)sender).Text + e.Text
                        };

                        // remove current information
                        anime.Names.RemoveAt(i);
                        // add new information at the same index
                        anime.Names.Insert(i, info);
                        break;
                    }
                    // if last iteration and language not found
                    else if (i == anime.Names.Count - 1)
                    {
                        // create new Information for name
                        Information info = new Information()
                        {
                            Name = label.Content.ToString(),
                            Value = ((TextBox)sender).Text + e.Text
                        };

                        // add new information
                        anime.Names.Add(info);
                    }
                }
            }
            // otherwise
            else
            {
                // create new Information for name
                Information info = new Information()
                {
                    Name = label.Content.ToString(),
                    Value = ((TextBox)sender).Text + e.Text
                };
                // add information to anime
                anime.Names.Add(info);
            }
        }

        private void OnAnimeDescriptionChanged(object sender, TextCompositionEventArgs e)
        {
            // find combobox
            ComboBox cb_language = (ComboBox)this.FindName("combobox_AnimeNameDescription");

            // get right item
            ComboBoxItem cb_item = (ComboBoxItem)cb_language.Items[cb_language.SelectedIndex];

            // get dockpanel
            DockPanel dockPanel = (DockPanel)cb_item.Content;

            // get label
            Label label = (Label)dockPanel.Children[1];

            if (anime.Descriptions == null)
            {
                anime.Descriptions = new List<Information>();
            }

            // if this anime has Names
            if (anime.Descriptions.Count > 0)
            {
                for (int i = 0; i < anime.Descriptions.Count; i++)
                {
                    // if right language found
                    if (anime.Descriptions[i].Name == label.Content.ToString())
                    {
                        // create new Information for name
                        Information info = new Information()
                        {
                            Name = label.Content.ToString(),
                            Value = ((TextBox)sender).Text + e.Text
                        };

                        // remove current information
                        anime.Descriptions.RemoveAt(i);
                        // add new information at the same index
                        anime.Descriptions.Insert(i, info);
                        break;
                    }
                    // if last iteration and language not found
                    else if (i == anime.Descriptions.Count - 1)
                    {
                        // create new Information for name
                        Information info = new Information()
                        {
                            Name = label.Content.ToString(),
                            Value = ((TextBox)sender).Text + e.Text
                        };

                        // add new information
                        anime.Descriptions.Add(info);
                    }
                }
            }
            // otherwise
            else
            {
                // create new Information for name
                Information info = new Information()
                {
                    Name = label.Content.ToString(),
                    Value = ((TextBox)sender).Text + e.Text
                };
                // add information to anime
                anime.Descriptions.Add(info);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // find combobox
            ComboBox cb_language = (ComboBox)sender;

            // get index
            string[] elementName = (cb_language.Name.ToString()).Split('_');

            int indexSeason = 0;
            int indexEpisode = 0;
            if (elementName.Length > 2)
            {
                indexSeason = int.Parse(elementName[2]);
            }
            if (elementName.Length > 3)
            {
                indexEpisode = int.Parse(elementName[3]);
            }

            // get right item
            ComboBoxItem cb_item = (ComboBoxItem)cb_language.Items[cb_language.SelectedIndex];

            // get dockpanel
            DockPanel dockPanel = (DockPanel)cb_item.Content;

            // get label
            Label label = (Label)dockPanel.Children[1];

            // check index
            #region AnimeName and AnimeDescription
            if (elementName[1] == "AnimeNameDescription")
            {
                #region Anime Descriptions
                if (anime.Descriptions == null)
                {
                    anime.Descriptions = new List<Information>();
                }

                // if this anime has Descriptions
                if (anime.Descriptions.Count > 0)
                {
                    for (int i = 0; i < anime.Descriptions.Count; i++)
                    {
                        // if right language found
                        if (anime.Descriptions[i].Name == label.Content.ToString())
                        {
                            TextBox tb = (TextBox)this.FindName("textbox_AnimeDescription");
                            tb.Text = anime.Descriptions[i].Value;
                            break;
                        }
                        // if last iteration and language not found
                        else if (i == anime.Descriptions.Count - 1)
                        {
                            // create new Information for description
                            Information info = new Information()
                            {
                                Name = label.Content.ToString(),
                                Value = ""
                            };

                            // add new information
                            anime.Descriptions.Add(info);
                        }
                    }
                }
                // otherwise
                else
                {
                    // create new Information for description
                    Information info = new Information()
                    {
                        Name = label.Content.ToString(),
                        Value = ""
                    };
                    // add information to anime
                    anime.Descriptions.Add(info);
                }
                #endregion

                #region Anime Names
                if (anime.Names == null)
                {
                    anime.Names = new List<Information>();
                }

                // if this anime has Names
                if (anime.Names.Count > 0)
                {
                    for (int i = 0; i < anime.Names.Count; i++)
                    {
                        // if right language found
                        if (anime.Names[i].Name == label.Content.ToString())
                        {
                            TextBox tb = (TextBox)this.FindName("textbox_AnimeName");
                            tb.Text = anime.Names[i].Value;
                            break;
                        }
                        // if last iteration and language not found
                        else if (i == anime.Names.Count - 1)
                        {
                            // create new Information for name
                            Information info = new Information()
                            {
                                Name = label.Content.ToString(),
                                Value = ""
                            };

                            // add new information
                            anime.Names.Add(info);
                        }
                    }
                }
                // otherwise
                else
                {
                    // create new Information for name
                    Information info = new Information()
                    {
                        Name = label.Content.ToString(),
                        Value = ""
                    };
                    // add information to anime
                    anime.Names.Add(info);
                }
                #endregion
            }
            #endregion
            #region EpisodeName and EpisodeDescription
            else if (elementName[1] == "EpisodeName")
            {
                #region Episode Descriptions
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions == null)
                {
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions = new List<Information>();
                }

                // if this episode has Descriptions
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Count > 0)
                {
                    for (int i = 0; i < anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Count; i++)
                    {
                        // if right language found
                        if (anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions[i].Name == label.Content.ToString())
                        {
                            TextBox tb = (TextBox)this.FindName("textbox_EpisodeDescription_" + indexSeason + "_" + indexEpisode);
                            tb.Text = anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions[i].Value;
                            break;
                        }
                        // if last iteration and language not found
                        else if (i == anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Count - 1)
                        {
                            // create new Information for name
                            Information info = new Information()
                            {
                                Name = label.Content.ToString(),
                                Value = ""
                            };

                            // add information to episode
                            anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Add(info);
                        }
                    }
                }
                // otherwise
                else
                {
                    // create new Information for name
                    Information info = new Information()
                    {
                        Name = label.Content.ToString(),
                        Value = ""
                    };
                    // add information to episode
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Descriptions.Add(info);
                }
                #endregion

                #region Episode Names
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Names == null)
                {
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Names = new List<Information>();
                }

                // if this episode has Names
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Count > 0)
                {
                    for (int i = 0; i < anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Count; i++)
                    {
                        // if right language found
                        if (anime.Seasons[indexSeason].Episodes[indexEpisode].Names[i].Name == label.Content.ToString())
                        {
                            TextBox tb = (TextBox)this.FindName("textbox_EpisodeName_" + indexSeason + "_" + indexEpisode);
                            tb.Text = anime.Seasons[indexSeason].Episodes[indexEpisode].Names[i].Value;
                            break;
                        }
                        // if last iteration and language not found
                        else if (i == anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Count - 1)
                        {
                            // create new Information for name
                            Information info = new Information()
                            {
                                Name = label.Content.ToString(),
                                Value = ""
                            };

                            // add information to episode
                            anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Add(info);
                        }
                    }
                }
                // otherwise
                else
                {
                    // create new Information for name
                    Information info = new Information()
                    {
                        Name = label.Content.ToString(),
                        Value = ""
                    };
                    // add information to episode
                    anime.Seasons[indexSeason].Episodes[indexEpisode].Names.Add(info);
                }
                #endregion
            }
            #endregion
            #region Dub
            else if (elementName[1] == "Dub")
            {
                // if dub already exist
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Dubs.Contains(label.Content.ToString()))
                {
                    // skip adding
                    return;
                }

                // get imagesource of selected language
                ImageSource source = ((Image)dockPanel.Children[0]).Source;

                // create new dockpanel
                DockPanel dockPanelDub = new DockPanel();
                dockPanelDub.Margin = new Thickness(5, 0, 0, 5);
                dockPanelDub.Width = 210;
                dockPanelDub.LastChildFill = false;
                dockPanelDub.Name = "dockPanelDub_" + indexSeason + "_" + indexEpisode + "_" + anime.Seasons[indexSeason].Episodes[indexEpisode].Dubs.Count;
                RegisterName(dockPanelDub.Name, dockPanelDub);

                // create new image for icon
                Image icon = new Image()
                {
                    Width = 20,
                    Source = source
                };
                DockPanel.SetDock(icon, Dock.Left);

                // create new label for language name
                Label text = new Label()
                {
                    Padding = new Thickness(0, -3, 0, 0),
                    Margin = new Thickness(5, 0, 0, 0),
                    Content = label.Content.ToString(),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                DockPanel.SetDock(text, Dock.Left);

                // create button to remove Dub
                Button buttonRemoveDub = new Button();
                buttonRemoveDub.Width = 20;
                buttonRemoveDub.Height = 20;
                buttonRemoveDub.Content = "-";
                buttonRemoveDub.Margin = new Thickness(0, 0, 5, 0);
                buttonRemoveDub.BorderThickness = new Thickness(0, 0, 0, 0);
                buttonRemoveDub.Foreground = new SolidColorBrush(Colors.White);
                buttonRemoveDub.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x4D, 0x4D, 0x4D));
                buttonRemoveDub.Tag = "Dub_" + indexSeason + "_" + indexEpisode + "_" + anime.Seasons[indexSeason].Episodes[indexEpisode].Dubs.Count;
                buttonRemoveDub.Click += RemoveSubDub;
                DockPanel.SetDock(buttonRemoveDub, Dock.Left);

                // add icon and text to dockpanel
                dockPanelDub.Children.Add(buttonRemoveDub);
                dockPanelDub.Children.Add(icon);
                dockPanelDub.Children.Add(text);
                DockPanel.SetDock(dockPanelDub, Dock.Top);

                // get right dockpanel
                DockPanel dockPanelMainDub = (DockPanel)this.FindName("dockpanel_Dub_" + indexSeason + "_" + indexEpisode);

                // add dockPanelDub to dockpanelmaindub
                dockPanelMainDub.Children.Add(dockPanelDub);

                // add dub to episode
                anime.Seasons[indexSeason].Episodes[indexEpisode].Dubs.Add(label.Content.ToString());
            }
            #endregion
            #region Sub
            else if (elementName[1] == "Sub")
            {
                // if sub already exist
                if (anime.Seasons[indexSeason].Episodes[indexEpisode].Subs.Contains(label.Content.ToString()))
                {
                    // skip adding
                    return;
                }

                // get imagesource of selected language
                ImageSource source = ((Image)dockPanel.Children[0]).Source;

                // create new dockpanel
                DockPanel dockPanelSub = new DockPanel();
                dockPanelSub.Margin = new Thickness(5, 0, 0, 5);
                dockPanelSub.Width = 210;
                dockPanelSub.LastChildFill = false;
                dockPanelSub.Name = "dockPanelSub_" + indexSeason + "_" + indexEpisode + "_" + anime.Seasons[indexSeason].Episodes[indexEpisode].Subs.Count;
                RegisterName("dockPanelSub_" + indexSeason + "_" + indexEpisode + "_" + anime.Seasons[indexSeason].Episodes[indexEpisode].Subs.Count, dockPanelSub);

                // create new image for icon
                Image icon = new Image()
                {
                    Width = 20,
                    Source = source
                };
                DockPanel.SetDock(icon, Dock.Left);

                // create new label for language name
                Label text = new Label()
                {
                    Padding = new Thickness(0, -3, 0, 0),
                    Margin = new Thickness(5, 0, 0, 0),
                    Content = label.Content.ToString(),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                DockPanel.SetDock(text, Dock.Left);

                // create button to remove Dub
                Button buttonRemoveSub = new Button();
                buttonRemoveSub.Width = 20;
                buttonRemoveSub.Height = 20;
                buttonRemoveSub.Content = "-";
                buttonRemoveSub.Margin = new Thickness(0, 0, 5, 0);
                buttonRemoveSub.BorderThickness = new Thickness(0, 0, 0, 0);
                buttonRemoveSub.Foreground = new SolidColorBrush(Colors.White);
                buttonRemoveSub.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x4D, 0x4D, 0x4D));
                buttonRemoveSub.Tag = "Sub_" + indexSeason + "_" + indexEpisode + "_" + anime.Seasons[indexSeason].Episodes[indexEpisode].Subs.Count;
                buttonRemoveSub.Click += RemoveSubDub;
                DockPanel.SetDock(buttonRemoveSub, Dock.Left);

                // add icon and text to dockpanel
                dockPanelSub.Children.Add(buttonRemoveSub);
                dockPanelSub.Children.Add(icon);
                dockPanelSub.Children.Add(text);
                DockPanel.SetDock(dockPanelSub, Dock.Top);

                // get right dockpanel
                DockPanel dockPanelMainSub = (DockPanel)this.FindName("dockpanel_Sub_" + indexSeason + "_" + indexEpisode);

                // add dockPanelDub to dockpanelmaindub
                dockPanelMainSub.Children.Add(dockPanelSub);

                // add int to SeasonEpisodeDubList
                anime.Seasons[indexSeason].Episodes[indexEpisode].Subs.Add(label.Content.ToString());
            }
            #endregion
        }

        private void OnSelectGenre(object sender, RoutedEventArgs e)
        {
            // get checkbox
            CheckBox checkBox = (CheckBox)sender;

            // get index
            string index = checkBox.Tag.ToString();

            if (index == "MainGenre")
            {
                // if checked
                if (checkBox.IsChecked == true)
                {
                    // add to maingenrelist
                    anime.MainGenres.Add(checkBox.Content.ToString());
                }
                // if unchecked
                else if (checkBox.IsChecked == false)
                {
                    // remove from maingenrelist
                    anime.MainGenres.Remove(checkBox.Content.ToString());
                }
            }
            else if (index == "SubGenre")
            {
                // if checked
                if (checkBox.IsChecked == true)
                {
                    // add to maingenrelist
                    anime.SubGenres.Add(checkBox.Content.ToString());
                }
                // if unchecked
                else if (checkBox.IsChecked == false)
                {
                    // add to maingenrelist
                    anime.SubGenres.Remove(checkBox.Content.ToString());
                }
            }
        }
        #endregion

        #region Remove-Functions
        private void RemoveEpisode(object sender, RoutedEventArgs e)
        {
            // get indeces
            string[] index = ((Button)sender).Tag.ToString().Split("_");

            // find right grid
            Grid grid = (Grid)this.FindName("grid_Episode_" + ((Button)sender).Tag);
            // hide episode
            grid.Visibility = Visibility.Collapsed;

            int index1 = int.Parse(index[0]);
            int index2 = int.Parse(index[1]);
            anime.Seasons[index1].Episodes[index2] = null;
        }

        private void RemoveSeason(object sender, RoutedEventArgs e)
        {
            // get index
            int index = int.Parse(((Button)sender).Tag.ToString());

            // delete season from list
            anime.Seasons[index] = null;

            // find right tabItem
            TabItem tabItem = (TabItem)this.FindName("tabitem_Season_" + index);

            // hide this tabitem
            tabItem.Visibility = Visibility.Collapsed;

            // if there are a minimum of 2 seasons
            if (anime.Seasons.Count > 1)
            {
                if (tabItem.IsSelected)
                {
                    // new index for searching
                    int newIndex = index;

                    // did we reached the end of the seasonlist?
                    bool isBackEnd = false;
                    // are we going backward? yes after hitting the end of the seasonlist
                    bool goingBackward = false;

                    // go through all temporary registered season
                    for (int i = seasonCount - 2; i >= 0; i--)
                    {
                        if (!isBackEnd && newIndex == seasonCount - 1)
                        {
                            // reached end of seasonlist
                            isBackEnd = true;

                            // we are going backward
                            goingBackward = true;

                            // set newIndex
                            newIndex = index;
                        }

                        if (goingBackward)
                        {
                            newIndex--;
                        }
                        else
                        {
                            newIndex++;
                        }

                        // find next tabitem
                        tabItem = (TabItem)this.FindName("tabitem_Season_" + newIndex);

                        if (tabItem.Visibility == Visibility.Collapsed)
                        {
                            // if we reached last element
                            if (i == 0)
                            {
                                // create new Label for argument
                                Label season = new Label
                                {
                                    Tag = seasonCount
                                };

                                // add new Season
                                AddSeason(season, e);

                                // find right tabItem
                                tabItem = (TabItem)this.FindName("tabitem_Season_" + (seasonCount - 1));

                                // set correct tabItem as selected
                                tabItem.IsSelected = true;

                                Label addSeason = (Label)this.FindName("label_AddSeason");
                                addSeason.Tag = seasonCount;
                            }
                            continue;
                        }
                        else
                        {
                            // set correct tabItem as selected
                            tabItem.IsSelected = true;
                            // end loop
                            break;
                        }
                    }
                }
            }
            else
            {
                // create new Label for argument
                Label season = new Label
                {
                    Tag = seasonCount
                };

                // add new Season
                AddSeason(season, e);

                // find right tabItem
                tabItem = (TabItem)this.FindName("tabitem_Season_" + (seasonCount - 1));
                tabItem.IsSelected = true;

                Label addSeason = (Label)this.FindName("label_AddSeason");
                addSeason.Tag = seasonCount;
            }
        }

        private void RemoveSubDub(object sender, RoutedEventArgs e)
        {
            string[] index = ((Button)sender).Tag.ToString().Split("_");

            int indexSeason = int.Parse(index[1]);
            int indexEpisode = int.Parse(index[2]);
            int indexSubDub = int.Parse(index[3]);

            if (index[0] == "Dub")
            {
                // find right dockpanel
                DockPanel dockPanel = (DockPanel)this.FindName("dockPanelDub_" + indexSeason + "_" + indexEpisode + "_" + indexSubDub);
                // hide dockpanel
                dockPanel.Visibility = Visibility.Collapsed;
                // delete dub from list
                anime.Seasons[indexSeason].Episodes[indexEpisode].Dubs[indexSubDub] = "0";
            }
            else if (index[0] == "Sub")
            {
                // find right dockpanel
                DockPanel dockPanel = (DockPanel)this.FindName("dockPanelSub_" + indexSeason + "_" + indexEpisode + "_" + indexSubDub);
                // hide dockpanel
                dockPanel.Visibility = Visibility.Collapsed;
                // delete sub from list
                anime.Seasons[indexSeason].Episodes[indexEpisode].Subs[indexSubDub] = "0";
            }
        }
        public void RemoveMoreDetails(object sender, RoutedEventArgs e)
        {
            // get tag splited
            string[] tmps = ((Button)sender).Tag.ToString().Split("_");

            string typ = tmps[0];
            int index = int.Parse(tmps[1]);

            // get right dockpanel
            DockPanel dockpanel = (DockPanel)this.FindName("dockpanel_" + typ + "_" + index);

            // hide dockpanel
            dockpanel.Visibility = Visibility.Collapsed;

            // set delete value for right object
            if (typ == "MainActor")
            {
                anime.MainActors[index] = "-1";
            }
            if (typ == "Producer")
            {
                anime.Producers[index] = "-1";
            }
            if (typ == "Director")
            {
                anime.Directors[index] = "-1";
            }
            if (typ == "Author")
            {
                anime.Authors[index] = "-1";
            }
        }
        #endregion

        private void AbortAnime(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}