using System.Windows;

namespace Mimir
{
    /// <summary>
    /// Interaktionslogik für Options_T1.xaml
    /// </summary>
    public partial class Options_T1 : Window
    {
        public Options_T1()
        {
            InitializeComponent();
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();                              // Einstellungen sichern
            base.OnClosing(e);                                               // Fenster schließen
        }
    }
}
