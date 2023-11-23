using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using Newtonsoft.Json;
using WebView2 = Microsoft.Web.WebView2.WinForms.WebView2;
using System.IO;

namespace WebOverlay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int x, y;
        public WebView2 webView21CreditsWebcam = new WebView2();
        private static int width = Screen.PrimaryScreen.Bounds.Width;
        private static int height = Screen.PrimaryScreen.Bounds.Height;
        private static string page;
        public static WebView2 webView21chat = new WebView2();
        private static string apikey, channelid, game;
        public static Image img;
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private VideoCapabilities[] videoCapabilities;
        private static bool getstate = false;
        private static double ratio;
        private static string base64image;
        public WebView2 webView21 = new WebView2();
        private ImageCodecInfo jpegEncoder;
        private EncoderParameters encoderParameters;
        public static int[] wd = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static int[] wu = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static bool[] ws = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
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
                game = file.ReadLine();
            }
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disable-web-security", "--allow-file-access-from-files", "--allow-file-access");
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView21CreditsWebcam.EnsureCoreWebView2Async(environment);
            webView21CreditsWebcam.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets", "assets", CoreWebView2HostResourceAccessKind.DenyCors);
            webView21CreditsWebcam.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView21CreditsWebcam.Source = new Uri("https://appassets/" + page);
            webView21CreditsWebcam.Dock = DockStyle.Fill;
            webView21CreditsWebcam.DefaultBackgroundColor = Color.Transparent;
            this.Controls.Add(webView21CreditsWebcam);
            await webView21chat.EnsureCoreWebView2Async(environment);
            webView21chat.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets", "assets", CoreWebView2HostResourceAccessKind.DenyCors);
            webView21chat.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView21chat.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());
            webView21chat.Source = new Uri("https://appassets/chat.html");
            webView21chat.Dock = DockStyle.Fill;
            webView21chat.DefaultBackgroundColor = Color.Transparent;
            this.Controls.Add(webView21chat);
            webView21chat.NavigationCompleted += WebView21chat_NavigationCompleted;
        }
        private async void WebView21chat_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (webView21chat.Source == new Uri("https://appassets/chat.html"))
            {
                webView21chat.ExecuteScriptAsync("getLoadPage('apikey', 'channelid');".Replace("apikey", apikey).Replace("channelid", channelid)).ConfigureAwait(false);
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
            var x = await webView21chat.ExecuteScriptAsync(script).ConfigureAwait(false);
            return x;
        }
        private async Task<String> execScriptHelperCreditsWebcam(String script)
        {
            var x = await webView21CreditsWebcam.ExecuteScriptAsync(script).ConfigureAwait(false);
            return x;
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
        private async void timer3_Tick(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(img);
                bmp = new Bitmap(bmp, new Size(bmp.Width / 2, bmp.Height / 2));
                bmp = ImageToGrayScale(bmp);
                byte[] imageArray = ImageToByteArray(bmp);
                base64image = Convert.ToBase64String(imageArray);
                await execScriptHelperCreditsWebcam($"setBackground('{base64image.ToString()}');");
            }
            catch { }
        }
        private async void timer2_Tick(object sender, EventArgs e)
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
                                allelements[i].style.width = '400px';
                                allelements[i].style.height = '550px';
                                allelements[i].style.minWidth = '400px';
                                allelements[i].style.minHeight = '550px';
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
                                allelements[i].style.width = '400px';
                                allelements[i].style.height = '550px';
                                allelements[i].style.minWidth = '400px';
                                allelements[i].style.minHeight = '550px';
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
                                allelements[i].style.width = '400px';
                                allelements[i].style.height = '550px';
                                allelements[i].style.minWidth = '400px';
                                allelements[i].style.minHeight = '550px';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#secondary');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.width = '400px';
                                allelements[i].style.height = '550px';
                                allelements[i].style.padding = '0px';
                                allelements[i].style.positionTop = '0px';
                                allelements[i].style.positionLeft = '0px';
                            }
                        }
                        catch { }
                        try {
                            var allelements = document.querySelectorAll('#chat-container');
                            for (var i = 0; i < allelements.length; i++) {
                                allelements[i].style.width = '400px';
                                allelements[i].style.height = '550px';
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
            try
            {
                string stringinject = @"
                    var style = `<style>
                        html {
                            background-color: transparent !important;
                        }
                    </style>`;
                    document.getElementsByTagName('head')[0].innerHTML += style;
                    ";
                await execScriptHelperChat(stringinject);
            }
            catch { }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            valchanged(0, GetAsyncKeyState(Keys.PageUp));
            if (wd[0] == 1)
            {
                this.TopMost = false;
            }
            valchanged(1, GetAsyncKeyState(Keys.PageDown));
            if (wd[1] == 1)
            {
                width = Screen.PrimaryScreen.Bounds.Width;
                height = Screen.PrimaryScreen.Bounds.Height;
                this.Size = new Size(width, height);
                this.Location = new Point(0, 0);
                this.TopMost = true;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            webView21CreditsWebcam.Dispose();
            webView21chat.Dispose();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                FinalFrame.NewFrame -= FinalFrame_NewFrame;
                System.Threading.Thread.Sleep(1000);
                if (FinalFrame.IsRunning)
                    FinalFrame.Stop();
            }
            catch { }
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
            Form1.webView21chat.ExecuteScriptAsync("reLoadPlayer();").ConfigureAwait(false);
            return param;
        }
    }
}
