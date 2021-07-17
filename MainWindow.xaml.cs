using System;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using Mimir._UserControls;
using Mimir._window;


namespace Mimir
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        #region ==============================================MAINWINDOW==============================================
        private void Button_T1(object sender, RoutedEventArgs e)
        {
            MAIN1.Content = new UC1();
        }
        private void Button_T2(object sender, RoutedEventArgs e)
        {
            MAIN1.Content = new UC2();
        }
        private void Btn_Info_Click(object sender, RoutedEventArgs e)
        {
            
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Properties.Settings.Default.Version = ("Version: " + version);

            var window = new Info
            {
                Owner = this                         // Macht das Fenster Hauptfenster zum Besitzer -- wird benoetigt um das Fenster mittig des Hauptfensters angezeigt -- in WPF Center Owner einstellen
            };
            _ = window.ShowDialog();                 // Methode wo das Haupfenster gesperrt wird
        }
        private void Btn_Exit_Click(object sender, RoutedEventArgs u)
        {
            Properties.Settings.Default.Save();                              // Einstellungen sichern
            Environment.Exit(0);                                                   // Fenster schließen über MEnue
        }




        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();                              // Einstellungen sichern
            base.OnClosing(e);                                               // Fenster schließen
        }
        #endregion


    }
}