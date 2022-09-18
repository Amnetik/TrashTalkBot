using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Interop;
using static TrashTalkBot.Messaging;
using static TrashTalkBot.KeyMessage;
using System.Threading;
using System.ComponentModel;

namespace TrashTalkBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Initialization
        public static IntPtr Handler;
        public MainWindow()
        {
            InitializeComponent();

        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            InitializeHook();
            Handler = new WindowInteropHelper(this).Handle;
            InitDataset();
            InitKeyConfiguration();
            InitializeKeyOnScreen();
        }
        void InitializeHook()
        {
            var windowHelper = new WindowInteropHelper(this);
            var windowSource = HwndSource.FromHwnd(windowHelper.Handle);
            windowSource.AddHook(MessagePumpHook);
        }

        /// <summary>
        /// Writes the saved keys on screen
        /// </summary>
        void InitializeKeyOnScreen()
        {
            if (TryGetKeyMessage(TypeMessage.Hello, out var key1))
                this.KeyHello.Text = key1.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.Hf, out var key2))
                this.KeyHf.Text = key2.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.ProTip, out var key3))
                this.KeyProTip.Text = key3.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.NiceShot, out var key4))
                this.KeyNiceShot.Text = key4.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.WhatASave, out var key5))
                this.KeyWhatASave.Text = key5.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.Miss, out var key6))
                this.KeyMiss.Text = key6.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.TrySmth, out var key7))
                this.KeyTrySmth.Text = key7.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.Offense, out var key8))
                this.KeyOffense.Text = key8.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.FF, out var key9))
                this.KeyFF.Text = key9.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.Gg, out var key10))
                this.KeyGG.Text = key10.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.Ez, out var key11))
                this.KeyEZ.Text = key11.Key.ToString();
            if (TryGetKeyMessage(TypeMessage.Other, out var key12))
                this.KeyOther.Text = key12.Key.ToString();
            this.KeyOpenChat.Text = OpenChatKey.ToString();
        }
        #endregion

        #region Handler KeyPressed
        IntPtr MessagePumpHook(IntPtr handle, int msg, IntPtr idKey, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                SendMessage((int)idKey);
                handled = true;
            }

            return IntPtr.Zero;
        }
        #endregion

        #region Event Handlers
        private void SelectRandom_Checked(object sender, RoutedEventArgs e)
        {
            IsLevelRandom = CheckRandom.IsChecked ?? true;
            refreshTbTrashLevel();
        }

        private void SelectRandom_Unchecked(object sender, RoutedEventArgs e)
        {
            IsLevelRandom = CheckRandom.IsChecked ?? true;
            refreshTbTrashLevel();
        }

        private void ButtonMinus_Click(object sender, RoutedEventArgs e)
        {
            if ((int)CurrentLevelMessage > 1)
                CurrentLevelMessage--;
            refreshTbTrashLevel();
        }

        private void ButtonPlus_Click(object sender, RoutedEventArgs e)
        {
            if ((int)CurrentLevelMessage < 5)
                CurrentLevelMessage++;
            refreshTbTrashLevel();
        }

        private void OnOff_Checked(object sender, RoutedEventArgs e)
        {
            isOffMessaging = false;
        }

        private void OnOff_Unchecked(object sender, RoutedEventArgs e)
        {
            isOffMessaging = true;
        }

        private void Key_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                return;

            isOffMessaging = true;

            var tb = (TextBox)sender;
            tb.Text = "";
            tb.Text = e.Key.ToString();
            TypeMessage type = TypeMessage.Hello;
            switch (tb.Name)
            {
                case "KeyHello":
                    type = TypeMessage.Hello;
                    break;
                case "KeyHf":
                    type = TypeMessage.Hf;
                    break;
                case "KeyProTip":
                    type = TypeMessage.ProTip;
                    break;
                case "KeyNiceShot":
                    type = TypeMessage.NiceShot;
                    break;
                case "KeyWhatASave":
                    type = TypeMessage.WhatASave;
                    break;
                case "KeyMiss":
                    type = TypeMessage.Miss;
                    break;
                case "KeyTrySmth":
                    type = TypeMessage.TrySmth;
                    break;
                case "KeyOffense":
                    type = TypeMessage.Offense;
                    break;
                case "KeyFF":
                    type = TypeMessage.FF;
                    break;
                case "KeyGG":
                    type = TypeMessage.Gg;
                    break;
                case "KeyEZ":
                    type = TypeMessage.Ez;
                    break;
                case "KeyOther":
                    type = TypeMessage.Other;
                    break;
                case "KeyOpenChat":
                    OpenChatKey = e.Key;
                    isOffMessaging = false;
                    return;
            }
            ChangeKey(e.Key, type);
            isOffMessaging = false;
        }

        private void refreshTbTrashLevel()
        {
            if (CheckRandom.IsChecked == true)
                LevelIndicator.Text = "Random";
            else
                LevelIndicator.Text = (CurrentLevelMessage).ToString();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveKeyConfiguration())
            {
                WriteInformation("Succesfully\nsaved.");
            }
        }

        /// <summary>
        /// Writes for a brief time an information on screen
        /// </summary>
        /// <param name="msg">String to write</param>
        /// <param name="timeSeconds">Time spent on screen</param>
        private void WriteInformation(string msg, int timeSeconds = 5)
        {
            this.Information.Text = msg;

            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate (object? o, DoWorkEventArgs args)
            {
                BackgroundWorker? b = o as BackgroundWorker;
                Thread.Sleep((timeSeconds - 1) * 1000);
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object? o, RunWorkerCompletedEventArgs args)
            {
                CleanInformation();
            });

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Fades the content wrote on screen
        /// </summary>
        private void CleanInformation()
        {
            SolidColorBrush infoBrush = (SolidColorBrush)this.Information.Foreground;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(
            delegate (object? o, DoWorkEventArgs args)
            {
                BackgroundWorker? b = o as BackgroundWorker;
                for (int i = 5; i <= 100; i += 5)
                {
                    if (b != null)
                        b.ReportProgress(i);
                    Thread.Sleep(50);
                }
            });

            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate (object? o, ProgressChangedEventArgs args)
            {
                float mult = (100 - (float)args.ProgressPercentage) / 100;
                byte alpha = (byte)(mult * infoBrush.Color.A);
                this.Information.Foreground = new SolidColorBrush(Color.FromArgb(alpha, infoBrush.Color.R, infoBrush.Color.G, infoBrush.Color.B));
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object? o, RunWorkerCompletedEventArgs args)
            {
                this.Information.Text = "";
                this.Information.Foreground = infoBrush;
            });
            bw.RunWorkerAsync();

        }
        #endregion
    }
}
