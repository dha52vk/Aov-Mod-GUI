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
using System.Windows.Shapes;

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public bool IsIndeterminate { get => IsIndeterminate; set => MainProgressBar.IsIndeterminate = value; }

        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void SetProgressMaxium(double maxium)
        {
            MainProgressBar.Maximum = maxium;
        }

        public async void UpdateProgress(double progressPercent, string progressStatus)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (progressPercent >= MainProgressBar.Maximum)
                {
                    Close();
                }
                MainProgressBar.Value = progressPercent;
                ProgressLabel.Content = progressStatus;
            });
        }

        public async void Execute(Action action)
        {
            await Task.Run(action).ConfigureAwait(true);
            UpdateProgress(MainProgressBar.Maximum, "Completed");
        }

        public void SetCancelable(bool cancelable)
        {
            CancelBtn.IsEnabled = cancelable;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
