﻿using System;
using System.Collections.Generic;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using WebView2 = Microsoft.Web.WebView2.WinForms.WebView2;
using System.IO;
using SharpDX.XInput;
using System.Threading;
using PromptHandle;
using System.Diagnostics;

namespace WebOverlay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(Keys vKey);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        private static string WINDOW_NAME = "";
        private const int GWL_STYLE = -16;
        private const uint WS_BORDER = 0x00800000;
        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_SYSMENU = 0x00080000;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const uint WS_OVERLAPPED = 0x00000000;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_TABSTOP = 0x00010000;
        private const uint WS_VISIBLE = 0x10000000;
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int x, y;
        private static ThreadStart threadstart;
        private static Thread thread;
        public WebView2 webView21CreditsWebcamController = new WebView2();
        private static int width = Screen.PrimaryScreen.Bounds.Width;
        private static int height = Screen.PrimaryScreen.Bounds.Height;
        private static string page;
        public static WebView2 webView21Chat = new WebView2();
        private static string apikey, channelid, windowtitle;
        public static Image img;
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private VideoCapabilities[] videoCapabilities;
        private static bool getstate = false;
        private static double ratio;
        private static string base64image;
        private ImageCodecInfo jpegEncoder;
        private EncoderParameters encoderParameters;
        public static bool closed = false;
        public static List<double> List_A = new List<double>(), List_B = new List<double>(), List_X = new List<double>(), List_Y = new List<double>(), List_LB = new List<double>(), List_RB = new List<double>(), List_LT = new List<double>(), List_RT = new List<double>(), List_MAP = new List<double>(), List_MENU = new List<double>(), List_LSTICK = new List<double>(), List_RSTICK = new List<double>(), List_DU = new List<double>(), List_DD = new List<double>(), List_DL = new List<double>(), List_DR = new List<double>(), List_XBOX = new List<double>();
        public static bool Controller_A, Controller_B, Controller_X, Controller_Y, Controller_LB, Controller_RB, Controller_MAP, Controller_MENU, Controller_LSTICK, Controller_RSTICK, Controller_DU, Controller_DD, Controller_DL, Controller_DR, Controller_XBOX;
        public static double Controller_LT, Controller_RT, Controller_LX, Controller_LY, Controller_RX, Controller_RY;
        private static Controller[] controller = new Controller[] { null };
        public static int xnum;
        private static SharpDX.XInput.State state;
        public static bool Controller1ButtonAPressed;
        public static bool Controller1ButtonBPressed;
        public static bool Controller1ButtonXPressed;
        public static bool Controller1ButtonYPressed;
        public static bool Controller1ButtonStartPressed;
        public static bool Controller1ButtonBackPressed;
        public static bool Controller1ButtonDownPressed;
        public static bool Controller1ButtonUpPressed;
        public static bool Controller1ButtonLeftPressed;
        public static bool Controller1ButtonRightPressed;
        public static bool Controller1ButtonShoulderLeftPressed;
        public static bool Controller1ButtonShoulderRightPressed;
        public static bool Controller1ThumbpadLeftPressed;
        public static bool Controller1ThumbpadRightPressed;
        public static double Controller1TriggerLeftPosition;
        public static double Controller1TriggerRightPosition;
        public static double Controller1ThumbLeftX;
        public static double Controller1ThumbLeftY;
        public static double Controller1ThumbRightX;
        public static double Controller1ThumbRightY;
        public static int[] wd = { 2, 2, 2 };
        public static int[] wu = { 2, 2, 2 };
        public static bool[] ws = { false, false, false };
        static void valchanged(int n, bool val)
        {
            if (val)
            {
                if (wd[n] <= 1)
                {
                    wd[n] = wd[n] + 1;
                }
                wu[n] = 0;
            }
            else
            {
                if (wu[n] <= 1)
                {
                    wu[n] = wu[n] + 1;
                }
                wd[n] = 0;
            }
            ws[n] = val;
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            Task.Run(() => StartWindowTitleRemover());
            this.Size = new Size(width, height);
            this.Location = new Point(0, 0);
            using (System.IO.StreamReader file = new System.IO.StreamReader("params.txt"))
            {
                file.ReadLine();
                page = file.ReadLine();
                file.ReadLine();
                apikey = file.ReadLine();
                file.ReadLine();
                channelid = file.ReadLine();
                file.ReadLine();
                windowtitle = file.ReadLine();
            }
            List<string> listrecords = new List<string>();
            listrecords = GetWindowTitles();
            string record = windowtitle;
            windowtitle = await Form2.ShowDialog("Window Titles", "What should be the window to handle capture?", record, listrecords);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        }
        private void StartWindowTitleRemover()
        {
            while (true)
            {
                valchanged(1, GetAsyncKeyState(Keys.PageDown));
                if (wu[1] == 1)
                {
                    int width = Screen.PrimaryScreen.Bounds.Width;
                    int height = Screen.PrimaryScreen.Bounds.Height;
                    IntPtr window = GetForegroundWindow();
                    SetWindowLong(window, GWL_STYLE, WS_SYSMENU);
                    SetWindowPos(window, -2, 0, 0, width, height, 0x0040);
                    DrawMenuBar(window);
                }
                valchanged(2, GetAsyncKeyState(Keys.PageUp));
                if (wu[2] == 1)
                {
                    IntPtr window = GetForegroundWindow();
                    SetWindowLong(window, GWL_STYLE, WS_CAPTION | WS_POPUP | WS_BORDER | WS_SYSMENU | WS_TABSTOP | WS_VISIBLE | WS_OVERLAPPED | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
                    DrawMenuBar(window);
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        public List<string> GetWindowTitles()
        {
            List<string> titles = new List<string>();
            foreach (Process proc in Process.GetProcesses())
            {
                string title = proc.MainWindowTitle;
                if (title != null & title != "")
                    titles.Add(title);
            }
            return titles;
        }
        private async void Start()
        {
            try
            {
                var controllers = new[] { new Controller(UserIndex.One) };
                xnum = 0;
                foreach (var selectControler in controllers)
                {
                    if (selectControler.IsConnected)
                    {
                        controller[xnum] = selectControler;
                        xnum++;
                        if (xnum > 0)
                        {
                            break;
                        }
                    }
                }
            }
            catch { }
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disable-web-security --allow-file-access-from-files --allow-file-access", "en");
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView21Chat.EnsureCoreWebView2Async(environment);
            webView21Chat.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets", "assets", CoreWebView2HostResourceAccessKind.DenyCors);
            webView21Chat.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView21Chat.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView21Chat.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());
            webView21Chat.Source = new Uri("https://appassets/chat.html");
            webView21Chat.Dock = DockStyle.Fill;
            webView21Chat.DefaultBackgroundColor = Color.Transparent;
            webView21Chat.NavigationCompleted += WebView21Chat_NavigationCompleted;
            this.Controls.Add(webView21Chat);
            webView21Chat.Focus();
            if (File.Exists(Application.StartupPath + @"\WebOverlay.exe.WebView2\EBWebView\Default\IndexedDB\https_www.youtube.com_0.indexeddb.leveldb/LOG.old"))
            {
                await webView21CreditsWebcamController.EnsureCoreWebView2Async(environment);
                webView21CreditsWebcamController.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets", "assets", CoreWebView2HostResourceAccessKind.DenyCors);
                webView21CreditsWebcamController.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView21CreditsWebcamController.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView21CreditsWebcamController.Source = new Uri("https://appassets/" + page);
                webView21CreditsWebcamController.Dock = DockStyle.Fill;
                webView21CreditsWebcamController.DefaultBackgroundColor = Color.Transparent;
                webView21CreditsWebcamController.KeyDown += WebView21CreditsWebcamController_KeyDown;
                this.Controls.Add(webView21CreditsWebcamController);
                webView21CreditsWebcamController.Focus();
                threadstart = new ThreadStart(ShowStream);
                thread = new Thread(threadstart);
                thread.Start();
            }
            else
            {
                this.pictureBox1.Image.Dispose();
                this.pictureBox1.Image = null;
                this.Controls.Remove(this.pictureBox1);
                this.pictureBox1.Dispose();
            }
        }
        private void WebView21CreditsWebcamController_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void WebView21Chat_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void OnKeyDown(Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                const string message = "• Author: Michaël André Franiatte.\n\r\n\r• Contact: michael.franiatte@gmail.com.\n\r\n\r• Publisher: https://github.com/michaelandrefraniatte.\n\r\n\r• Copyrights: All rights reserved, no permissions granted.\n\r\n\r• License: Not open source, not free of charge to use.";
                const string caption = "About";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void ShowStream()
        {
            System.Threading.Thread.Sleep(20000);
            this.pictureBox1.Image.Dispose();
            this.pictureBox1.Image = null;
            this.Controls.Remove(this.pictureBox1);
            this.pictureBox1.Dispose();
            int px = Screen.PrimaryScreen.Bounds.Width * 28 / 100;
            int py = 1;
            int lx = Screen.PrimaryScreen.Bounds.Width * 70 / 100;
            int ly = Screen.PrimaryScreen.Bounds.Width * 70 / 100 * 9 / 16;
            WINDOW_NAME = windowtitle;
            IntPtr window = FindWindowByCaption(IntPtr.Zero, WINDOW_NAME);
            SetWindowLong(window, GWL_STYLE, WS_SYSMENU);
            SetWindowPos(window, -2, px, py, lx, ly, 0x0040);
            DrawMenuBar(window);
            ShowWindow(window, 9);
            SetForegroundWindow(window);
            Microsoft.VisualBasic.Interaction.AppActivate(WINDOW_NAME);
            SetWindowLong(window, GWL_STYLE, WS_SYSMENU);
            SetWindowPos(window, -2, px, py, lx, ly, 0x0040);
            DrawMenuBar(window);
            ShowWindow(window, 9);
            SetForegroundWindow(window);
            Microsoft.VisualBasic.Interaction.AppActivate(WINDOW_NAME);
        }
        private async void WebView21Chat_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (webView21Chat.Source == new Uri("https://appassets/chat.html"))
            {
                webView21Chat.ExecuteScriptAsync("getLoadPage('apikey', 'channelid');".Replace("apikey", apikey).Replace("channelid", channelid)).ConfigureAwait(false);
            }
            if (File.Exists(Application.StartupPath + @"\WebOverlay.exe.WebView2\EBWebView\Default\IndexedDB\https_www.youtube.com_0.indexeddb.leveldb/LOG.old"))
            {
                try
                {
                    string stringinject = @"
                    var style = `<style>
                        body, html {
                            background-color: transparent !important;
                        }
                        * {
                            background-color: black !important;
                        }
                    </style>`;
                    document.getElementsByTagName('head')[0].innerHTML += style;
                    ";
                    await execScriptHelperChat(stringinject);
                }
                catch { }
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            jpegEncoder = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 255);
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            FinalFrame = new VideoCaptureDevice(CaptureDevice[0].MonikerString);
            videoCapabilities = FinalFrame.VideoCapabilities;
            FinalFrame.VideoResolution = videoCapabilities[videoCapabilities.Length - 1];
            ratio = Convert.ToDouble(FinalFrame.VideoResolution.FrameSize.Width) / Convert.ToDouble(FinalFrame.VideoResolution.FrameSize.Height);
            height = 300;
            width = (int)(height * ratio);
            FinalFrame.NewFrame += FinalFrame_NewFrame;
            FinalFrame.Start();
        }
        private async Task<String> execScriptHelperChat(String script)
        {
            var x = await webView21Chat.ExecuteScriptAsync(script).ConfigureAwait(false);
            return x;
        }
        private async Task<String> execScriptHelperCreditsWebcamController(String script)
        {
            var x = await webView21CreditsWebcamController.ExecuteScriptAsync(script).ConfigureAwait(false);
            return x;
        }
        public async void SetController(bool Controller1ButtonAPressed, bool Controller1ButtonBPressed, bool Controller1ButtonXPressed, bool Controller1ButtonYPressed, bool Controller1ButtonStartPressed, bool Controller1ButtonBackPressed, bool Controller1ButtonDownPressed, bool Controller1ButtonUpPressed, bool Controller1ButtonLeftPressed, bool Controller1ButtonRightPressed, bool Controller1ButtonShoulderLeftPressed, bool Controller1ButtonShoulderRightPressed, bool Controller1ThumbpadLeftPressed, bool Controller1ThumbpadRightPressed, double Controller1TriggerLeftPosition, double Controller1TriggerRightPosition, double Controller1ThumbLeftX, double Controller1ThumbLeftY, double Controller1ThumbRightX, double Controller1ThumbRightY)
        {
            try
            {
                Controller_RX = Controller1ThumbRightX;
                Controller_RY = Controller1ThumbRightY;
                Controller_LX = Controller1ThumbLeftX;
                Controller_LY = Controller1ThumbLeftY;
                Controller_RT = Controller1TriggerRightPosition;
                Controller_LT = Controller1TriggerLeftPosition;
                if (List_A.Count >= 5)
                {
                    List_A.RemoveAt(0);
                    List_A.Add(Controller1ButtonAPressed ? 1 : 0);
                    Controller_A = List_A.Average() > 0 ? true : false;
                }
                else
                {
                    List_A.Add(0);
                }
                if (List_B.Count >= 5)
                {
                    List_B.RemoveAt(0);
                    List_B.Add(Controller1ButtonBPressed ? 1 : 0);
                    Controller_B = List_B.Average() > 0 ? true : false;
                }
                else
                {
                    List_B.Add(0);
                }
                if (List_X.Count >= 5)
                {
                    List_X.RemoveAt(0);
                    List_X.Add(Controller1ButtonXPressed ? 1 : 0);
                    Controller_X = List_X.Average() > 0 ? true : false;
                }
                else
                {
                    List_X.Add(0);
                }
                if (List_Y.Count >= 5)
                {
                    List_Y.RemoveAt(0);
                    List_Y.Add(Controller1ButtonYPressed ? 1 : 0);
                    Controller_Y = List_Y.Average() > 0 ? true : false;
                }
                else
                {
                    List_Y.Add(0);
                }
                if (List_LB.Count >= 5)
                {
                    List_LB.RemoveAt(0);
                    List_LB.Add(Controller1ButtonShoulderLeftPressed ? 1 : 0);
                    Controller_LB = List_LB.Average() > 0 ? true : false;
                }
                else
                {
                    List_LB.Add(0);
                }
                if (List_RB.Count >= 5)
                {
                    List_RB.RemoveAt(0);
                    List_RB.Add(Controller1ButtonShoulderRightPressed ? 1 : 0);
                    Controller_RB = List_RB.Average() > 0 ? true : false;
                }
                else
                {
                    List_RB.Add(0);
                }
                if (List_MAP.Count >= 5)
                {
                    List_MAP.RemoveAt(0);
                    List_MAP.Add(Controller1ButtonBackPressed ? 1 : 0);
                    Controller_MAP = List_MAP.Average() > 0 ? true : false;
                }
                else
                {
                    List_MAP.Add(0);
                }
                if (List_MENU.Count >= 5)
                {
                    List_MENU.RemoveAt(0);
                    List_MENU.Add(Controller1ButtonStartPressed ? 1 : 0);
                    Controller_MENU = List_MENU.Average() > 0 ? true : false;
                }
                else
                {
                    List_MENU.Add(0);
                }
                if (List_LSTICK.Count >= 5)
                {
                    List_LSTICK.RemoveAt(0);
                    List_LSTICK.Add(Controller1ThumbpadLeftPressed ? 1 : 0);
                    Controller_LSTICK = List_LSTICK.Average() > 0 ? true : false;
                }
                else
                {
                    List_LSTICK.Add(0);
                }
                if (List_RSTICK.Count >= 5)
                {
                    List_RSTICK.RemoveAt(0);
                    List_RSTICK.Add(Controller1ThumbpadRightPressed ? 1 : 0);
                    Controller_RSTICK = List_RSTICK.Average() > 0 ? true : false;
                }
                else
                {
                    List_RSTICK.Add(0);
                }
                if (List_DU.Count >= 5)
                {
                    List_DU.RemoveAt(0);
                    List_DU.Add(Controller1ButtonUpPressed ? 1 : 0);
                    Controller_DU = List_DU.Average() > 0 ? true : false;
                }
                else
                {
                    List_DU.Add(0);
                }
                if (List_DD.Count >= 5)
                {
                    List_DD.RemoveAt(0);
                    List_DD.Add(Controller1ButtonDownPressed ? 1 : 0);
                    Controller_DD = List_DD.Average() > 0 ? true : false;
                }
                else
                {
                    List_DD.Add(0);
                }
                if (List_DL.Count >= 5)
                {
                    List_DL.RemoveAt(0);
                    List_DL.Add(Controller1ButtonLeftPressed ? 1 : 0);
                    Controller_DL = List_DL.Average() > 0 ? true : false;
                }
                else
                {
                    List_DL.Add(0);
                }
                if (List_DR.Count >= 5)
                {
                    List_DR.RemoveAt(0);
                    List_DR.Add(Controller1ButtonRightPressed ? 1 : 0);
                    Controller_DR = List_DR.Average() > 0 ? true : false;
                }
                else
                {
                    List_DR.Add(0);
                }
                if (List_XBOX.Count >= 5)
                {
                    List_XBOX.RemoveAt(0);
                    List_XBOX.Add(false ? 1 : 0);
                    Controller_XBOX = List_XBOX.Average() > 0 ? true : false;
                }
                else
                {
                    List_XBOX.Add(0);
                }
                await execScriptHelperCreditsWebcamController($"setController('{Controller_A.ToString()}', '{Controller_B.ToString()}', '{Controller_X.ToString()}', '{Controller_Y.ToString()}', '{Controller_MAP.ToString()}', '{Controller_MENU.ToString()}', '{Controller_DD.ToString()}', '{Controller_DU.ToString()}', '{Controller_DL.ToString()}', '{Controller_DR.ToString()}', '{Controller_LB.ToString()}', '{Controller_RB.ToString()}', '{Controller_LSTICK.ToString()}', '{Controller_RSTICK.ToString()}', '{Controller_LT.ToString()}', '{Controller_RT.ToString()}', '{Controller_XBOX.ToString()}', '{Controller_LX.ToString()}', '{Controller_LY.ToString()}', '{Controller_RX.ToString()}', '{Controller_RY.ToString()}');");
            }
            catch { }
        }
        private async void taskEmulate()
        {
            try
            {
                for (int inc = 0; inc < xnum; inc++)
                {
                    state = controller[inc].GetState();
                    if (inc == 0)
                    {
                        if (state.Gamepad.Buttons.ToString().Contains("A"))
                            Controller1ButtonAPressed = true;
                        else
                            Controller1ButtonAPressed = false;
                        if (state.Gamepad.Buttons.ToString().EndsWith("B") | state.Gamepad.Buttons.ToString().Contains("B, "))
                            Controller1ButtonBPressed = true;
                        else
                            Controller1ButtonBPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("X"))
                            Controller1ButtonXPressed = true;
                        else
                            Controller1ButtonXPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("Y"))
                            Controller1ButtonYPressed = true;
                        else
                            Controller1ButtonYPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("Start"))
                            Controller1ButtonStartPressed = true;
                        else
                            Controller1ButtonStartPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("Back"))
                            Controller1ButtonBackPressed = true;
                        else
                            Controller1ButtonBackPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("DPadDown"))
                            Controller1ButtonDownPressed = true;
                        else
                            Controller1ButtonDownPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("DPadUp"))
                            Controller1ButtonUpPressed = true;
                        else
                            Controller1ButtonUpPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("DPadLeft"))
                            Controller1ButtonLeftPressed = true;
                        else
                            Controller1ButtonLeftPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("DPadRight"))
                            Controller1ButtonRightPressed = true;
                        else
                            Controller1ButtonRightPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("LeftShoulder"))
                            Controller1ButtonShoulderLeftPressed = true;
                        else
                            Controller1ButtonShoulderLeftPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("RightShoulder"))
                            Controller1ButtonShoulderRightPressed = true;
                        else
                            Controller1ButtonShoulderRightPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("LeftThumb"))
                            Controller1ThumbpadLeftPressed = true;
                        else
                            Controller1ThumbpadLeftPressed = false;
                        if (state.Gamepad.Buttons.ToString().Contains("RightThumb"))
                            Controller1ThumbpadRightPressed = true;
                        else
                            Controller1ThumbpadRightPressed = false;
                        Controller1TriggerLeftPosition = state.Gamepad.LeftTrigger;
                        Controller1TriggerRightPosition = state.Gamepad.RightTrigger;
                        Controller1ThumbLeftX = state.Gamepad.LeftThumbX;
                        Controller1ThumbLeftY = state.Gamepad.LeftThumbY;
                        Controller1ThumbRightX = state.Gamepad.RightThumbX;
                        Controller1ThumbRightY = state.Gamepad.RightThumbY;
                        SetController(Controller1ButtonAPressed, Controller1ButtonBPressed, Controller1ButtonXPressed, Controller1ButtonYPressed, Controller1ButtonStartPressed, Controller1ButtonBackPressed, Controller1ButtonDownPressed, Controller1ButtonUpPressed, Controller1ButtonLeftPressed, Controller1ButtonRightPressed, Controller1ButtonShoulderLeftPressed, Controller1ButtonShoulderRightPressed, Controller1ThumbpadLeftPressed, Controller1ThumbpadRightPressed, Controller1TriggerLeftPosition, Controller1TriggerRightPosition, Controller1ThumbLeftX, Controller1ThumbLeftY, Controller1ThumbRightX, Controller1ThumbRightY);
                    }
                }
            }
            catch { }
        }
        public static Bitmap ImageToGrayScale(Bitmap Bmp)
        {
            Bitmap newBitmap = new Bitmap(Bmp.Width, Bmp.Height);
            Graphics g = Graphics.FromImage(newBitmap);
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(Bmp, new Rectangle(0, 0, Bmp.Width, Bmp.Height), 0, 0, Bmp.Width, Bmp.Height, GraphicsUnit.Pixel, attributes);
            g.Dispose();
            return newBitmap;
        }
        public byte[] ImageToByteArray(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, jpegEncoder, encoderParameters);
                return ms.ToArray();
            }
        }
        void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            img = (Bitmap)eventArgs.Frame.Clone();
        }
        private void timer4_Tick(object sender, EventArgs e)
        {
            taskEmulate();
        }
        private async void timer3_Tick(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(img);
                bmp = new Bitmap(bmp, new Size(bmp.Width / 4, bmp.Height / 4));
                bmp = ImageToGrayScale(bmp);
                byte[] imageArray = ImageToByteArray(bmp);
                base64image = Convert.ToBase64String(imageArray);
                await execScriptHelperCreditsWebcamController($"setBackground('{base64image.ToString()}');");
            }
            catch { }
        }
        private async void timer2_Tick(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + @"\WebOverlay.exe.WebView2\EBWebView\Default\IndexedDB\https_www.youtube.com_0.indexeddb.leveldb/LOG.old"))
            {
                try
                {
                    string stringinject = @"
                    if (window.location.href.indexOf('youtube') > -1 | window.location.href.indexOf('youtu.be') > -1) {
                        try {
                            var playButton = document.querySelector('.ytp-large-play-button:visible');
                            if (playButton) {
                                playButton.click();
                            }
                        }
                        catch { }
                        try {
                            var skipButton = document.querySelector('.ytp-ad-skip-button');
                            if (skipButton) {
                                skipButton.click();
                            }
                        }
                        catch { }
                        try {
                            var skipButton = document.querySelector('.ytp-ad-skip-button-modern');
                            if (skipButton) {
                                skipButton.click();
                            }
                        }
                        catch { }
                        try {
                            var closeButton = document.querySelector('.ytp-ad-overlay-close-button');
                            if (closeButton) {
                                closeButton.click();
                            }
                        }
                        catch { }
                        try {
                            document.cookie = 'VISITOR_INFO1_LIVE = oKckVSqvaGw; path =/; domain =.youtube.com';
                            var cookies = document.cookie.split('; ');
                            for (var i = 0; i < cookies.length; i++)
                            {
                                var cookie = cookies[i];
                                var eqPos = cookie.indexOf('=');
                                var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
                                document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT';
                            }
                        }
                        catch { }
                        try {
                            var els = document.getElementsByClassName('video-ads ytp-ad-module');
                            for (var i=0;i<els.length; i++) {
                                els[i].click();
                            }
                        }
                        catch { }
                        try {
                            var el = document.getElementsByClassName('ytp-ad-skip-button');
                            for (var i=0;i<el.length; i++) {
                                el[i].click();
                            }
                        }
                        catch { }
                        try {
                            var element = document.getElementsByClassName('ytp-ad-overlay-close-button');
                            for (var i=0;i<element.length; i++) {
                                element[i].click();
                            }
                        }
                        catch { }
                        try {
                            var scripts = document.getElementsByTagName('script');
                            for (let i = 0; i < scripts.length; i++)
                            {
                                var content = scripts[i].innerHTML;
                                if (content.indexOf('ytp-ad') > -1) {
                                    scripts[i].innerHTML = '';
                                }
                                var src = scripts[i].getAttribute('src');
                                if (src.indexOf('ytp-ad') > -1) {
                                    scripts[i].setAttribute('src', '');
                                }
                            }
                        }
                        catch { }
                        try {
                            var iframes = document.getElementsByTagName('iframe');
                            for (let i = 0; i < iframes.length; i++)
                            {
                                var content = iframes[i].innerHTML;
                                if (content.indexOf('ytp-ad') > -1) {
                                    iframes[i].innerHTML = '';
                                }
                                var src = iframes[i].getAttribute('src');
                                if (src.indexOf('ytp-ad') > -1) {
                                    iframes[i].setAttribute('src', '');
                                }
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('*');
                            for (var i = 0; i < allelements.length; i++) {
                                var classname = allelements[i].className;
                                if (classname.indexOf('ytp-ad') > -1 | classname.indexOf('-ad-') > -1 | classname.indexOf('ad-') > -1 | classname.indexOf('ads-') > -1 | classname.indexOf('ad-showing') > -1 | classname.indexOf('ad-container') > -1 | classname.indexOf('ytp-ad-overlay-open') > -1 | classname.indexOf('video-ads') > -1)  {
                                    allelements[i].innerHTML = '';
                                }
                            }
                        }
                        catch { }
                        try {
                            var players = document.getElementById('movie_player');
                            for (let i = 0; i < players.length; i++) {
                                players.classList.remove('ad-interrupting');
                                players.classList.remove('playing-mode');
                                players.classList.remove('ytp-autohide');
                                players.classList.add('ytp-hide-info-bar');
                                players.classList.add('playing-mode');
                                players.classList.add('ytp-autohide');
                            }
                        }
                        catch { }
                        try {
                            var fabelements = document.querySelectorAll('yt-reaction-control-panel-button-view-model');
                            for (var i = 0; i < fabelements.length; i++) {
                                    fabelements[i].innerHTML = '';
                            }
                        }
                        catch { }
                        try {
                            var fabelement = document.querySelector('#fab-container');
                            fabelement.innerHTML = '';
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.ad-showing');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.ad-container');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.ytp-ad-overlay-open');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.video-ads');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.ytp-ad-overlay-image');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.ytp-ad-overlay-container');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('.ytd-carousel-ad-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('ytd-ad-slot-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('ytd-action-companion-ad-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('ytd-engagement-panel-section-list-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('ytd-player-legacy-desktop-watch-ads-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('yt-reaction-control-panel-button-view-model');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('tp-yt-paper-dialog');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('ytd-statement-banner-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelectorAll('ytd-brand-video-singleton-renderer');
                            contents.forEach(elem => elem.style.display = 'none');
                        }
                        catch { }
                        try {
                            var contents = document.querySelector('#reaction-control-panel').style.display = 'none';
                        }
                        catch { }
                        try {
                            var contents = document.querySelector('#emoji-fountain').style.display = 'none';
                        }
                        catch { }
                        try {
                            var contents = document.querySelector('#fab-container').style.display = 'none';
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#primary');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#container');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#related');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#panels');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('ytd-playlist-panel-renderer');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#donation-shelf');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#background');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#show-hide-button');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = 'none';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#columns');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = '';
                                allelements[i].classList.remove('ytd-watch-flexy');
                                allelements[i].style.width = '26vw';
                                allelements[i].style.height = '64vh';
                                allelements[i].style.minWidth = '26vw';
                                allelements[i].style.minHeight = '64vh';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#content');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = '';
                                allelements[i].classList.remove('ytd-watch-flexy');
                                allelements[i].style.width = '26vw';
                                allelements[i].style.height = '64vh';
                                allelements[i].style.minWidth = '26vw';
                                allelements[i].style.minHeight = '64vh';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('ytd-app');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.display = '';
                                allelements[i].classList.remove('ytd-watch-flexy');
                                allelements[i].style.width = '26vw';
                                allelements[i].style.height = '64vh';
                                allelements[i].style.minWidth = '26vw';
                                allelements[i].style.minHeight = '64vh';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#secondary');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.width = '26vw';
                                allelements[i].style.height = '64vh';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#chat-container');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.width = '26vw';
                                allelements[i].style.height = '64vh';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            document.documentElement.style.backgroundColor = 'transparent !important';
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('*');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.background = 'transparent';
                                allelements[i].style.backgroundColor = 'transparent !important';
                                allelements[i].style.margin = '0px';
                                allelements[i].style.overflowY = 'hidden';
                                allelements[i].removeAttribute('darker-dark-theme');
                                allelements[i].removeAttribute('darker-dark-theme-deprecate');
                                allelements[i].removeAttribute('dark');
                                allelements[i].style.backgroundColor = '';
                            }
                        }
                        catch { }
                    }
                    ";
                    await execScriptHelperChat(stringinject);
                }
                catch { }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            valchanged(0, GetAsyncKeyState(Keys.Add));
            if (wd[0] == 1)
            {
                if (!getstate)
                {
                    Start();
                    getstate = true;
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            webView21CreditsWebcamController.Dispose();
            webView21Chat.Dispose();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            try
            {
                FinalFrame.NewFrame -= FinalFrame_NewFrame;
                System.Threading.Thread.Sleep(1000);
                if (FinalFrame.IsRunning)
                    FinalFrame.Stop();
            }
            catch { }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("params.txt"))
            {
                file.WriteLine("// Page for credits and webcam and controller");
                file.WriteLine(page);
                file.WriteLine("// Youtube data API key");
                file.WriteLine(apikey);
                file.WriteLine("// Channel ID");
                file.WriteLine(channelid);
                file.WriteLine("// Game window title");
                file.WriteLine(windowtitle);
            }
        }
    }
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {
        public static Form1 form1 = new Form1();
        public static string txt;
        public string LoadPage(string param)
        {
            Form1.webView21Chat.ExecuteScriptAsync("reLoadPlayer();").ConfigureAwait(false);
            return param;
        }
    }
}
