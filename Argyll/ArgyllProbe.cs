﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using NotifyChange;

namespace Argyll
{
    public class ArgyllProbe : Notifier
    {
        public ArgyllProbe(bool autoconfig = true)
        {
            if(autoconfig) ConfigFromDipcalGUI();
        }


        private static IniFile _dispcalIni;
        private static IniFile DispcalIni
        {
            get
            {
                if (_dispcalIni == null)
                    _dispcalIni = new IniFile(
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            @"dispcalGui\dispcalGUI.ini"
                            ));

                return _dispcalIni;
            }

        }


        private readonly double[] _xyz = { 0, 0, 0 };
        //            private readonly double[] _lab = { 0, 0, 0 };

        private static void ArgyllSendKey(Process p, String key)
        {
            //System.Threading.Thread.Sleep(300);
            p.StandardInput.Flush();
            p.StandardInput.Write(key);
            p.StandardInput.Flush();
        }

        private string _name = "new";
        private bool _calibrating = false;
        private bool _spectrum = false;
        private double _spectrumFrom = 0;
        private double _spectrumTo = 0;
        private int _spectrumSteps = 0;
        private double _cct = 0;
        private double _cri = 0;
        private double _tlci = 0;
        private double _lux = 0;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public double SpectrumFrom
        {
            get { return _spectrumFrom; }
            set { SetProperty(ref _spectrumFrom, value); }
        }

        public double SpectrumTo
        {
            get { return _spectrumTo; }
            set { SetProperty(ref _spectrumTo, value); }
        }

        public int SpectrumSteps
        {
            get { return _spectrumSteps; }
            set { SetProperty(ref _spectrumSteps, value); }
        }

        public double Cct
        {
            get { return _cct; }
            set { SetProperty(ref _cct, value); }
        }

        public double Cri
        {
            get { return _cri; }
            set { SetProperty(ref _cri, value); }
        }

        public double Tlci
        {
            get { return _tlci; }
            set { SetProperty(ref _tlci, value); }
        }

        public double Lux
        {
            get { return _lux; }
            set { SetProperty(ref _lux, value); }
        }

        public ObservableCollection<double> Spectrum { get; set; } = new ObservableCollection<double> {0};

        public ObservableCollection<double> WaveLength { get; set; } = new ObservableCollection<double> {0};
        private void ArgyllOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            string line = outLine.Data;

            Console.WriteLine(line);

            if (line == null) return;

            Process p = sendingProcess as Process;

            if (p == null) return;

            if (_spectrum)
            {
                 string[] s = line.Split(',');

                Spectrum.Clear();
                WaveLength.Clear();

                double nm = SpectrumFrom;
                double step = (SpectrumTo - SpectrumFrom)/(SpectrumSteps - 1);

                foreach (string t in s)
                {
                    Spectrum.Add( double.Parse(t));
                    WaveLength.Add(nm);
                    nm += step;
                }
                _spectrum = false;
            }


            if (line.Contains("Spectrum from"))
            {
                int pos = line.IndexOf("Spectrum from", StringComparison.Ordinal);
                string sub = line.Substring(pos + 14);
                string[] s = sub.Split(' ');
                SpectrumFrom = double.Parse(s[0]);
                SpectrumTo = double.Parse(s[2]);
                SpectrumSteps = int.Parse(s[5]);
                _spectrum = true;
            }

            if (line.Contains("Ambient"))
            {
                string[] s = line.Split(' ');
                Lux = double.Parse(s[3]);
                Cct = double.Parse(s[7].Replace("K",""));
            }

            if (line.Contains("(Ra)"))
            {
                string[] s = line.Split(' ');
                Cri = double.Parse(s[6]);
            }

            if (line.Contains("(Qa)"))
            {
                string[] s = line.Split(' ');
                Tlci = double.Parse(s[8]);
            }

            if (line.Contains("Error - Opening USB port"))
                ArgyllSendKey(p, "q");

            if (line.Contains("calibration position"))
            {
                if (!_calibrating)
                {
                    var result = MessageBox.Show("Place instrument in calibration position", "Instrument",
                        MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    ArgyllSendKey(p, result == MessageBoxResult.OK ? "k" : "q");

                    _calibrating = true;
                }
                else ArgyllSendKey(p, "k");
            }

            if (line.Contains("Place instrument"))
            {
                System.Threading.Thread.Sleep(300);
                p.StandardInput.Flush();
                //var result = MessageBox.Show("Place instrument in measure position", "Instrument",
                //    MessageBoxButton.OKCancel, MessageBoxImage.Information);
                //ArgyllSendKey(p, result == MessageBoxResult.OK ? "0" : "q");
                ArgyllSendKey(p, "0");
            }

            if (line.Contains("Result is XYZ:"))
            {
                int pos = line.IndexOf("XYZ: ", StringComparison.Ordinal);
                string sub = line.Substring(pos + 5);
                sub = sub.Remove(sub.IndexOf(','));
                string[] s = sub.Split(' ');
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        _xyz[i] = Double.Parse(s[i]);
                    }
                    catch { _xyz[i] = 0; }
                }

                _calibrating = false;
                //if (line.Contains("D50 Lab:"))
                //{
                //    pos = line.IndexOf("D50 Lab:", StringComparison.Ordinal);
                //    sub = line.Substring(pos + 9);
                //    //sub.Remove(sub.IndexOf(','));
                //    s = sub.Split(' ');
                //    for (int i = 0; i < 3; i++)
                //    {
                //        try
                //        {
                //            _lab[i] = Double.Parse(s[i]);
                //        }
                //        catch { _lab[i] = 0; }
                //    }

                //}

                //((Process)sendingProcess).Kill();
            }
        }

        public enum MeasurementMode
        {
            Emissive,
            Projector,
            Ambiant,
            Flash
        }

        public enum ObserverEnum
        {
            CIE_1931_2,
            CIE_1964_10,
            SB_1955_2,
            JV_1978_2,
            Shaw,
        }

        public static string ArgyllPath { get; set; }

        public int ColorTemp { get; set; } = 6500;
        public MeasurementMode Mode { get; set; } = MeasurementMode.Emissive;
        public bool HighResolution { get; set; } = true;
        public bool Adaptive { get; set; } = true;
        public bool ReadSpectrum { get; set; } = false;
        public bool ReadCri { get; set; } = false;

        private ObserverEnum Observer { get; set; } = ObserverEnum.CIE_1931_2;

        public static void PathFromDispcalGUI()
        {
            ArgyllPath = DispcalIni.ReadValue("Default", "argyll.dir", "");
        }

        public void ConfigFromDipcalGUI()
        {
            PathFromDispcalGUI();

            ColorTemp = 
            int.Parse(DispcalIni.ReadValue("Default", "whitepoint.colortemp", "5000"));

            switch (DispcalIni.ReadValue("Default", "measurement_mode", "1"))
            {
                case "c": // CRT ???
                    break;
                case "p": // CRT ???
                    Mode = MeasurementMode.Projector;
                    break;
                case "1":
                    Mode = MeasurementMode.Emissive;
                    break;
            }

            HighResolution = (DispcalIni.ReadValue("Default", "measurement_mode.highres", "0") == "1");

            Adaptive = (DispcalIni.ReadValue("Default", "measurement_mode.adaptive", "1") == "1");

            string obs = DispcalIni.ReadValue("Default", "observer", "1931_2");
            switch (obs)
            {
                case "1931_2":
                    Observer = ObserverEnum.CIE_1931_2;
                    break;
                case "1964_10":
                    Observer = ObserverEnum.CIE_1964_10;
                    break;
                case "1955_2":
                    Observer = ObserverEnum.SB_1955_2;
                    break;
                case "shaw":
                    Observer = ObserverEnum.Shaw;
                    break;
                case "1978_2":
                    Observer = ObserverEnum.JV_1978_2;
                    break;
            }
        }

        public string SpotReadArgs
        {
            get
            {
                string s = " -N";
                switch (Mode)
                {
                    case MeasurementMode.Projector:
                        s += " -pb";
                        break;
                    case MeasurementMode.Emissive:
                        s += " -e";
                        break;
                    case MeasurementMode.Ambiant:
                        s += " -a";
                        break;
                    case MeasurementMode.Flash:
                        s += " -f";
                        break;
                }



                if (HighResolution) s += " -H";

                if (!Adaptive) s += " -Y A";

                s += " -O";

                s += " -Q";

                switch (Observer)
                {
                    case ObserverEnum.CIE_1931_2:
                        s += " 1931_2";
                        break;
                    case ObserverEnum.CIE_1964_10:
                        s += " 1964_10";
                        break;
                    case ObserverEnum.SB_1955_2:
                        s += " 1955_2";
                        break;
                    case ObserverEnum.Shaw:
                        s += " shaw";
                        break;
                    case ObserverEnum.JV_1978_2:
                        s += " 1978_2";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (ReadSpectrum) s += " -s";
                if (ReadCri) s += " -T";
                return s;
            }
        }


        public bool Installed => ArgyllPath != "";

        public bool SpotRead()
        {
            if (!Installed) return false;

            do
            {
                ExecSpotRead();
            } while (_calibrating);

            return true;
        }

        public ProbedColor ProbedColor => new ProbedColorXYZ
            {
                Illuminant = ProbedColor.DIlluminant(ColorTemp),
                X = _xyz[0],
                Y = _xyz[1],
                Z = _xyz[2]
            };

        public void ExecSpotRead()
        {
            Process[] aProc = Process.GetProcessesByName("Spotread");
            for (int i = 0; i < aProc.Length; i++)
            {
                aProc[i].Kill();
                if (!aProc[i].HasExited)
                    aProc[i].WaitForExit();
            }

            Process p = new Process();

            p.StartInfo.FileName = Path.Combine(ArgyllPath, @"Spotread.exe");
            //                p.StartInfo.Arguments = "-N -O -Y A";
            p.StartInfo.Arguments = SpotReadArgs;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = true;

            try
            {
                p.StartInfo.EnvironmentVariables.Add("ARGYLL_NOT_INTERACTIVE", "yes");
            }
            catch
            {
            }

            p.ErrorDataReceived += new DataReceivedEventHandler(ArgyllOutputHandler);
            p.OutputDataReceived += new DataReceivedEventHandler(ArgyllOutputHandler);

            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

            if (!p.HasExited) p.WaitForExit();
        }
        public void Save()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".probe";
            dlg.Filter = "Probe documents (.probe)|*.probe";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                Save(filename);
            }
        }

        public void Save(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ArgyllProbe));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }


        public static ArgyllProbe Load()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".probe";
            dlg.Filter = "Probe documents (.probe)|*.probe";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                return Load(filename);
            }
            return null;
        }


        public static ArgyllProbe Load(string path)
        {
            ArgyllProbe probe = null;

            XmlSerializer deserializer = new XmlSerializer(typeof(ArgyllProbe));

            try
            {
                TextReader reader = new StreamReader(path);
                probe = (ArgyllProbe)deserializer.Deserialize(reader);
                reader.Close();
            }
            catch (FileNotFoundException)
            {

            }

            return probe;
        }


    }
}

/*
    private int state = 0;
    private double[] xyz = { 0, 0, 0 };
    private double[] rvb = { 0, 0, 0 };
    private double[] d50 = { 0, 0, 0 };
    private void ArgyllOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
       System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
       string line = outLine.Data;
       Console.WriteLine(line);
       if(line == null) return;
       if(line.Contains("Error - Opening USB port")) state = 255;
       if(line.Contains("needs a calibration")) state = -1;
       if(line.Contains("Place instrument")) state = 1;
       if(line.Contains("Result is XYZ:"))
       {
          int pos = line.IndexOf("XYZ: ");
          string sub = line.Substring(pos + 5);
          sub = sub.Remove(sub.IndexOf(','));
          string[] s = sub.Split(' ');
          for(int i = 0; i < 3; i++)
          {
             try
             {
                xyz[i] = Double.Parse(s[i]);
             }
             catch { xyz[i] = 0; }
          }
          if(line.Contains("D50 Lab:"))
          {
             pos = line.IndexOf("D50 Lab:");
             sub = line.Substring(pos + 9);
             //sub.Remove(sub.IndexOf(','));
             s = sub.Split(' ');
             for(int i = 0; i < 3; i++)
             {
                try
                {
                   d50[i] = Double.Parse(s[i]);
                }
                catch { d50[i] = 0; }
             }
          }
          state = 2;
       }
    }
    public void Lance()
    {
       Process p = new Process();
       p.StartInfo.FileName = "C:\\Fabien\\Argyll_V1.4.0\\bin\\Spotread.exe";
       p.StartInfo.Arguments = "-N -d";
       //p.StartInfo.Arguments = "-N -H -d";
       //p.StartInfo.Arguments = "--help";
       p.StartInfo.UseShellExecute = false;
       p.StartInfo.RedirectStandardOutput = true;
       p.StartInfo.RedirectStandardError = true;
       p.StartInfo.RedirectStandardInput = true;
       p.StartInfo.CreateNoWindow = true;
       p.StartInfo.EnvironmentVariables.Add("ARGYLL_NOT_INTERACTIVE", "yes");
       p.ErrorDataReceived += new DataReceivedEventHandler(ArgyllOutputHandler);
       p.OutputDataReceived += new DataReceivedEventHandler(ArgyllOutputHandler);
       state = 0;
       p.Start();
       p.BeginErrorReadLine();
       p.BeginOutputReadLine();
       while(state == 0) { }
       if(state == -1)
       {
          p.StandardInput.Write("k");
          p.StandardInput.Flush();
          Console.Write(state);
       }
       while(state == -1) { }
       if(state > 100) return;
       if(state == 1)
       {
          p.StandardInput.Write("0");
          p.StandardInput.Flush();
       }
       while(state == 1) { }
       try
       {
          Console.WriteLine(xyz[0].ToString() + ' ' + xyz[1].ToString() + ' ' + xyz[2].ToString());
          //rvb[0] = 
          Console.WriteLine(rvb[0].ToString() + ' ' + rvb[1].ToString() + ' ' + rvb[2].ToString());
          Console.WriteLine(d50[0].ToString() + ' ' + d50[1].ToString() + ' ' + d50[2].ToString());
       }
       catch
       {
       }
       p.StandardInput.Write("q");
       p.StandardInput.Flush();
       if(!p.HasExited) p.Kill();
       if(!p.HasExited) p.WaitForExit();
    }
 }
}
*/

