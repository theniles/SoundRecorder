using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.IO;
using NAudio;
using NAudio.Codecs;
using NAudio.Wave;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;
using System.Diagnostics;
using NAudio.Wave.SampleProviders;

namespace SoundRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private WaveFileWriter fileWriter;

        private WaveInEvent waveIn;

        private bool recording;

        private string saveLocation;

        public WaveInCapabilities[] InputAudios { get; set; }

        public string SaveLocation { get { return saveLocation; } set { saveLocation = value; OnPropertyChanged(); } }

        public MainWindow()
        {
            InitializeComponent();
            refreshInputDevices();

            SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + "NileSounds";

            try
            {
                Directory.CreateDirectory(SaveLocation);
            }
            catch (Exception ex)
            {
                Info("Could not create the default output folder. Reason: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Info(object o)
        {
            MessageBox.Show(o.ToString(), "NileSound", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //toggle record
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            recording = !recording;
            btn.Content = recording ? "Stop recording" : "Start recording";

            if (recording)
            {
                try
                {
                    if (audioCombo.SelectedIndex == -1)
                    {
                        throw new Exception("No audio device is selected.");
                    }

                    string saveFile = "NileSound_" +
                                       Directory.EnumerateFiles(SaveLocation, "NileSound_*.wav").Count() +
                                       ".wav";

                    waveIn = new WaveInEvent();
                    waveIn.DeviceNumber = audioCombo.SelectedIndex;
                    waveIn.DataAvailable += WaveIn_DataAvailable;
                    waveIn.RecordingStopped += WaveIn_RecordingStopped;

                    fileWriter = new WaveFileWriter(Path.Combine(SaveLocation, saveFile), waveIn.WaveFormat);
                    waveIn.StartRecording();

                    audioCombo.IsEnabled = false;
                    detectBtn.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    Info("Failiure starting recording. Reason: " + ex.Message);
                }
            }
            else
            {
                audioCombo.IsEnabled = true;
                detectBtn.IsEnabled = true;
                waveIn.StopRecording();
                fileWriter.Dispose();
                waveIn.Dispose();
            }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                audioCombo.IsEnabled = true;
                detectBtn.IsEnabled = true;
                Info("Failiure recording data. Reason: " + e.Exception.Message);
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                fileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                fileWriter.Flush();
            }
            catch (Exception ex)
            {
                Info("Failiure writing to the file. Reason: " + ex.Message);
            }
        }

        //browse save loc
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "NileSounds save location selector";
            var res = dlg.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                SaveLocation = dlg.SelectedPath;
            }
        }

        private void refreshInputDevices()
        {
            int l = WaveIn.DeviceCount;
            InputAudios = new WaveInCapabilities[WaveIn.DeviceCount];

            for (int i = 0; i < l; i++)
            {
                InputAudios[i] = WaveIn.GetCapabilities(i);
            }

            OnPropertyChanged(nameof(InputAudios));

            audioCombo.SelectedIndex = 0;
        }

        //detect aduio devices click
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            refreshInputDevices();
        }

        //open save location folder
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", SaveLocation);
            }
            catch (Exception ex)
            {
                Info("Failiure in opening the folder. Reason: " + ex.Message);
            }
        }
    }
}
