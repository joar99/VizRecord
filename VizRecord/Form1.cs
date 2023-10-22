using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using RtmpSharp;
using RtmpSharp.Net;

namespace VizRecord
{
    public partial class Form1 : Form
    {
        private static ScreenCapture screenCapture = new ScreenCapture();
        public static bool isRecording = false;
        private static Encoder encoder = new Encoder();

        public Form1()
        {
            InitializeComponent();
            PopulateComboBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckSelection())
            {
                string selectedMonitor = comboBox1.SelectedItem as string;
                isRecording = true;
                Thread captureThread = new Thread(RecordScreen);
                captureThread.Start(selectedMonitor);
            }
            else
            {
                MessageBox.Show("Please Select Input Source");
            }
        }
        

        public void POSTRecord(string monitor)
        {
          
            string selectedMonitor = monitor as string;
            isRecording = true;
            Thread captureThread = new Thread(RecordScreen);
            captureThread.Start(selectedMonitor);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isRecording = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static async void RecordScreen(object monitorName)
        {

            while (isRecording)
            {
                string selectedMonitor = monitorName as string;
                Bitmap frame = screenCapture.CaptureScreen(selectedMonitor);
                Bitmap frameCopy = (Bitmap)frame.Clone();
                if (pictureBox1.IsHandleCreated)
                {
                    pictureBox1.BeginInvoke(new Action(() => pictureBox1.Image = frame));
                }

                if (frameCopy != null)
                {
                    await encoder.SendToRTMP(frameCopy);

                }

            }
            if (pictureBox1.IsHandleCreated)
            {
                pictureBox1.BeginInvoke(new Action(() => pictureBox1.Image = null));
            }

        }

        public void PopulateComboBox()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                comboBox1.Items.Add(screen.DeviceName);
            }
        }

        public bool CheckSelection()
        {
            string selectedMonitor = comboBox1.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedMonitor))
            {
                return false;
            }

            return true;
        }

    }

    public class ScreenCapture
    {
        private Rectangle screenBounds;
        public Rectangle ScreenBounds => screenBounds;

        private Rectangle GetScreenBounds(string monitor)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.DeviceName == monitor)
                {
                    return screen.Bounds;
                }
            }
            throw new ArgumentException("Invalid Monitor Specified");
        }

        public Bitmap CaptureScreen(string monitor)
        {
            screenBounds = GetScreenBounds(monitor);

            try
            {
                using (Bitmap frame = new Bitmap(screenBounds.Width, screenBounds.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(frame))
                    {
                        graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size);

                        return frame.Clone() as Bitmap;
                    }
                }
            }
            catch (ArgumentException ex)
            {

                Bitmap emptyBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
                using (Graphics graphics = Graphics.FromImage(emptyBitmap))
                {
                    graphics.Clear(Color.White);
                }

                return emptyBitmap;
            }
        }

    }

    public class Encoder
    {

        private Process ffmpeg;

        private StreamWriter sw;

        public Encoder()
        {
            string ffmpegPath = @"C:\Users\jonas\ffmpeg-6.0-full_build\bin\ffmpeg.exe";
            string ffmpegArgs = "-re -f image2pipe -i - -c:v libx264 -f flv rtmp://192.168.8.140:1935/VizTest/VizStream";

            ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegPath;
            ffmpeg.StartInfo.Arguments = ffmpegArgs;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.StartInfo.RedirectStandardInput = true;

            ffmpeg.Start();

            sw = new StreamWriter(ffmpeg.StandardInput.BaseStream);
        }

        public async Task SendToRTMP(Bitmap bitmap)
        {
            bitmap.Save(sw.BaseStream, ImageFormat.Png);

            await sw.FlushAsync();

            bitmap.Dispose();
        }
    }

}







