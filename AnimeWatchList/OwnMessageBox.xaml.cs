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
    /// Interaktionslogik für OwnMessageBox.xaml
    /// </summary>
    public partial class OwnMessageBox : Window
    {
        /// <summary>
        /// result of the messagebox
        /// </summary>
        public OwnMessageboxResult MessageboxResult;

        /// <summary>
        /// list with all buttons of the messagebox
        /// </summary>
        private List<Button> buttons = new List<Button>();

        private OwnMessageboxTyp typ;

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

        public OwnMessageBox()
        {
            InitializeComponent();

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\Warning.png";

            if (File.Exists(filePath))
                image_Warningimage.Source = new BitmapImage(new Uri(filePath));
        }

        public void SetElements(OwnMessageboxTyp _messageBoxTyp, string _message)
        {
            switch (_messageBoxTyp)
            {
                case OwnMessageboxTyp.WARNING:
                    Button buttonOK = new Button
                    {
                        Width = 90,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5),
                        Tag = "OK"
                    };
                    buttonOK.Click += Result;

                    buttons.Add(buttonOK);
                    dockpanel_Buttons.Children.Add(buttonOK);
                    break;
                case OwnMessageboxTyp.WARNING_YES_NO:
                    Button buttonYES = new Button
                    {
                        Width = 90,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5),
                        Tag = "YES"
                    };
                    buttonYES.Click += Result;
                    Button buttonNO = new Button
                    {
                        Width = 90,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5),
                        Tag = "NO"
                    };
                    buttonNO.Click += Result;

                    buttons.Add(buttonYES);
                    dockpanel_Buttons.Children.Add(buttonYES);
                    buttons.Add(buttonNO);
                    dockpanel_Buttons.Children.Add(buttonNO);
                    break;
                case OwnMessageboxTyp.WARNING_REPLACE_ADD_ABORT:
                    Button buttonREPLACE = new Button
                    {
                        Width = 90,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5),
                        Tag = "REPLACE"
                    };
                    buttonREPLACE.Click += Result;
                    Button buttonADD = new Button
                    {
                        Width = 90,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5),
                        Tag = "ADD"
                    };
                    buttonADD.Click += Result;
                    Button buttonABORT = new Button
                    {
                        Width = 90,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0x4D, 0x4D)),
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5),
                        Tag = "ABORT"
                    };
                    buttonABORT.Click += Result;

                    buttons.Add(buttonREPLACE);
                    dockpanel_Buttons.Children.Add(buttonREPLACE);
                    buttons.Add(buttonADD);
                    dockpanel_Buttons.Children.Add(buttonADD);
                    buttons.Add(buttonABORT);
                    dockpanel_Buttons.Children.Add(buttonABORT);
                    break;
            }

            typ = _messageBoxTyp;

            textblock_Value.Text = _message;

            // set buttonpictures for titlebar
            // close button
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "images\\CloseButton.png";

            if (File.Exists(filePath))
                image_Close.Source = new BitmapImage(new Uri(filePath));

            minWidth = (int)MinWidth;
            minHeight = (int)MinHeight;

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            button_Close.Click += (sender, e) => Close();
        }

        public void SetElements(OwnMessageboxTyp _messageboxTyp, string _message, string _title)
        {
            SetElements(_messageboxTyp, _message);
            window_Messagebox.Title = _title;
        }

        public void SetLanguage(LanguageDetail _languageDetail)
        {
            switch (typ)
            {
                case OwnMessageboxTyp.WARNING:
                    buttons[0].Content = _languageDetail.ButtonOk;
                    break;
                case OwnMessageboxTyp.WARNING_YES_NO:
                    buttons[0].Content = _languageDetail.ButtonYes;
                    buttons[1].Content = _languageDetail.ButtonNo;
                    break;
                case OwnMessageboxTyp.WARNING_REPLACE_ADD_ABORT:
                    buttons[0].Content = _languageDetail.ButtonReplace;
                    buttons[1].Content = _languageDetail.ButtonAdd;
                    buttons[2].Content = _languageDetail.ButtonAbort;
                    break;
            }

            if (window_Messagebox.Title == "")
                window_Messagebox.Title = _languageDetail.Warning;
        }

        private void Result(object sender, RoutedEventArgs e)
        {
            string result = ((Button)sender).Tag.ToString();

            switch (result)
            {
                case "OK":
                    MessageboxResult = OwnMessageboxResult.YES;
                    break;
                case "YES":
                    MessageboxResult = OwnMessageboxResult.YES;
                    break;
                case "NO":
                    MessageboxResult = OwnMessageboxResult.NO;
                    break;
                case "ADD":
                    MessageboxResult = OwnMessageboxResult.ADD;
                    break;
                case "REPLACE":
                    MessageboxResult = OwnMessageboxResult.REPLACE;
                    break;
            }

            Close();
        }
    }

    public enum OwnMessageboxTyp
    {
        WARNING,
        WARNING_YES_NO,
        WARNING_REPLACE_ADD_ABORT
    }

    public enum OwnMessageboxResult
    {
        YES,
        NO,
        ADD,
        REPLACE
    }
}
