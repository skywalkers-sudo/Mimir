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

namespace Mimir._window
{
    /// <summary>
    /// Interaktionslogik für Options_SQL.xaml
    /// </summary>
    public partial class Options_SQL : Window
    {
        public Options_SQL()
        {
            InitializeComponent();
            SetProperties();
        }


        public void SetProperties()
        {
            Uri iconUri = new Uri("pack://application:,,,/_Images/ico/faviconsettings.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();                              // Einstellungen sichern
            base.OnClosing(e);                                               // Fenster schließen
        }



    }
}
