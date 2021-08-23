using System;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Data.SqlClient;
using System.Windows.Navigation;
using System.Threading;
using System.Data;
using Mimir._database;
using System.Windows.Media;
using Mimir._window;
using System.Windows.Media.Imaging;

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
            SQL();
            Version();
            SetProperties();
        }



        #region ========================================MAINWINDOW  START-TAB========================================
        // --------------   close instruction  ----------------------------
        private void Btn_Exit_Click(object sender, RoutedEventArgs u)
        {
            Properties.Settings.Default.scanVC_Info = " ";                   // log GUI leeren Scan VC
            Properties.Settings.Default.Save();                              // Einstellungen sichern
            Environment.Exit(0);                                                   // Fenster schließen über MEnue
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.scanVC_Info = " ";                   // log GUI leeren Scan VC
            Properties.Settings.Default.Save();                              // Einstellungen sichern
            base.OnClosing(e);                                               // Fenster schließen
        }
        // ----------------------get Settings------------------------------
        public void SetProperties()
        {
            // Icon für Window festlegen
            Uri iconUri = new Uri("pack://application:,,,/_Images/ico/favicon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        // -----------------SQL DATABASE CONNECTION------------------------
        public void SQL()
        {
            //MessageBox.Show("Getting Connection ...");
            SqlConnection conn = DBUtils.GetDBConnection();

            try
            {
                //MessageBox.Show("Openning Connection ...");
                conn.Open();        

                // Abfrage ob SQL verbindung erfolgreich war
                if (conn.State == ConnectionState.Open)
                {
                    var stm = "SELECT @@VERSION";
                    using var cmd = new SqlCommand(stm, conn);
                    string version = cmd.ExecuteScalar().ToString();
                    lbl_SQL1.Content = "SQL Database-Connection successful!";
                    lbl_SQL1.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0xF1, 0x1D));   // #FF32F11D grün
                }

                else
                {
                    lbl_SQL1.Content = "SQL-Connection failed!";
                    lbl_SQL1.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x2B, 0x2B));   // #FF FF 2B 2B rot
                }

            }

            catch (Exception)
            {
                // MessageBox.Show("" + e);
                lbl_SQL1.Content = "SQL-Connection failed!";
                lbl_SQL1.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x2B, 0x2B));   // #FF FF 2B 2B rot
            }
            finally
            {
                // die Verbindung schließen.
                conn.Close();
                // das Objekt absagen, die Ressourcen freien.
                conn.Dispose();
                conn = null;
            }
        }
        private void Btn_start_sql_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SQL();
            }
            catch (Exception r)
            {
                MessageBox.Show("" + r);
            }
                
        }
        private void Btn_option_sql_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Options_SQL window = new();


                // Haupfenster als Eigentümer festlegn
                Window mainWindow = Application.Current.MainWindow;
                window.Owner = mainWindow;


                _ = window.ShowDialog();      // Methode wo das Haupfenster gesperrt wird
            }
            catch (Exception u)
            {
                MessageBox.Show("" + u);
            }
        }
        // -------------------------------Version--------------------------
        public void Version()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Properties.Settings.Default.Version = "Version: " + version;
        }
        #endregion


        #region ==============================================XML-sync================================================

        // Filewatcher
        FileSystemWatcher FSW;
        public void Btn_Start_sync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // gui log leeren
                Properties.Settings.Default.syncxml_Info = "";

                // Buttons available
                btn_Start_sync.IsEnabled = false;
                btn_stop_sync.IsEnabled = true;
                btn_option_start_sync.IsEnabled = false;

                // PFad zum überwachen aus Settings lesen
                string path = Mimir.Properties.Settings.Default.optionsT1_source;

                // Create a new FileSystemWatcher and set its properties.
                FSW = new FileSystemWatcher
                {
                    Path = @path,

                    // Only watch text files.      
                    Filter = "*.xml"
                };

                //FSW.Changed += new FileSystemEventHandler(FSW_Changed);
                FSW.Created += new FileSystemEventHandler(FSW_Created);
                //FSW.Deleted += new FileSystemEventHandler(FSW_Deleted);
                //FSW.Renamed += new RenamedEventHandler(FSW_Renamed);

                // Begin watching.      
                FSW.EnableRaisingEvents = true;

                // write Status to xaml
                status.Content = "FILEWATCHER RUN";
                status.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0xF1, 0x1D));   // #FF32F11D grün

            }

            catch (Exception u)
            {
                MessageBox.Show("" + u);
            }
        }

        // Define the event handler.
        private void FSW_Created(object source, FileSystemEventArgs e)
        {
            try
            {
                // =================================================================================HEAD==============================================================================================
                string ROOTXML = Properties.Settings.Default.optionsT1_source;       // Wurzelverzeichis der zu ladenden XML
                string TARGETXML = Properties.Settings.Default.optionsT1_destination;   // Zielverzeichnis der zu schreibenden XML

                string[] xmlListEXIST = Directory.GetFiles(ROOTXML, "*.xml");
                int anzahlxml = xmlListEXIST.GetLength(0);


                while (anzahlxml != 0)

                {

                    // array der ganzen .xml erstellen
                    string[] xmlList = Directory.GetFiles(ROOTXML, "*.xml");

                    // Pfadangabe löschen, es bleibt nur die Datei
                    string filename = null;
                    filename = System.IO.Path.GetFileName(xmlList[0]);

                    // Extension löschen (.xml)
                    char[] MyChar = { 'x', 'm', 'l', '.' };
                    string filnamewithoutExtension = filename.TrimEnd(MyChar);

                    // Erstelle neue NC Nummer für XML eintrag
                    string path1 = @ROOTXML + filename;
                    string newNCNUMBER = filnamewithoutExtension + "000";

                    // stringbuilder für Info
                    StringBuilder sb = new();
                    _ = sb.Append("===================  neues Wkz gefunden " + filename + "  ====================");

                    string datetime = DateTime.Now.ToString();
                    _ = sb.Append("\n --> " + datetime);


                    // ================================================================================FEATURE 1 CHECK========================================================================================
                    if (Properties.Settings.Default.optionsT1_Feature1_check == true)
                    {
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load(path1);


                        // Lesen von Werkzeugstatus (funktiniert nur solange nicht mehr Unterknoten im Knoten param drinnen sind)
                        XmlNode noderead1 = xmlDoc.SelectSingleNode("/omtdx/ncTools/ncTools/ncTools/ncTool/customData/param");

                        if (noderead1 != null)
                        {

                            var Status = noderead1.Attributes["value"].Value;

                            // Lese bestehenden Werkzeugnamen
                            XmlNode noderead2 = xmlDoc.SelectSingleNode("omtdx/ncTools/ncTools/ncTools/ncTool");

                            if (noderead2 != null)
                            {
                                var bNcname = noderead2.Attributes["name"].Value;

                                // Schreibe neuen Werkzeugnamen
                                XmlNode nodewrite = xmlDoc.SelectSingleNode("omtdx/ncTools/ncTools/ncTools/ncTool");
                                nodewrite.Attributes[2].Value = Status + " // " + bNcname;

                                _ = sb.Append("\n" + " --> Werkzeugname geändert auf --> " + Status + " // " + bNcname);

                            }
                        }
                        xmlDoc.Save(path1);

                    }

                    // ================================================================================FEATURE 2 CHECK========================================================================================
                    if (Properties.Settings.Default.optionsT1_Feature2_check == true)
                    {
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load(path1);

                        XmlNode node = xmlDoc.SelectSingleNode("omtdx/ncTools/ncTools/ncTools/ncTool");
                        node.Attributes[1].Value = newNCNUMBER;

                        _ = sb.Append("\n" + " --> ungeprüftes Wkz NC-Nummer auf  --> " + newNCNUMBER + " geändert");

                        xmlDoc.Save(path1);
                    }

                    // ================================================================================FEATURE 3 CHECK========================================================================================
                    if (Properties.Settings.Default.optionsT1_Feature3_check == true)
                    {
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load(path1);


                        // Lesen von Werkzeugreferenzpunkt
                        XmlNode noderead1 = xmlDoc.SelectSingleNode("/omtdx/ncTools/ncTools/ncTools/ncTool/referencePoints/referencePoint");

                        if (noderead1 != null)
                        {
                            var refpoint1 = noderead1.Attributes["name"].Value;

                            // Lesen von Werkzeugklasse
                            XmlNode noderead2 = xmlDoc.SelectSingleNode("/omtdx/tools/tools/tools/tool");
                            if (noderead2 != null)
                            {
                                var toolclass = noderead2.Attributes["type"].Value;

                                if (refpoint1 == "S2" && toolclass == "drilTool")
                                {
                                    // Schreibe neuen Werkzeugfefernznamen
                                    XmlNode nodewrite = xmlDoc.SelectSingleNode("/omtdx/ncTools/ncTools/ncTools/ncTool/referencePoints/referencePoint");
                                    nodewrite.Attributes[1].Value = "1";

                                    _ = sb.Append("\n" + " --> Wkz-Klasse 'toolDrill' und Referenzpunkt '" + refpoint1 + "' erkannt --> Name Referenzpunkt geaendert auf '1' ");

                                }
                                else
                                {
                                    _ = sb.Append("\n" + " --> Werkzeugreferenzname nicht gefunden und/oder Werkzeugklasse ist kein Bohrer  ");

                                }
                            }
                            else
                            {
                                _ = sb.Append("\n" + " --> Referenzpunkte gefunden, Eintrag Werkzeugklasse nicht gefunden");
                            }
                        }
                        else
                        {
                            _ = sb.Append("\n" + " --> Keine Referenzpunktinformationen enthalten ");
                        }

                        xmlDoc.Save(path1);
                    }

                    // ================================================================================FEATURE 4 CHECK========================================================================================
                    if (Properties.Settings.Default.optionsT1_Feature4_check == true)
                    {
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load(path1);


                        // Lesen von Werkzeugreferenzpunkt
                        XmlNode noderead1 = xmlDoc.SelectSingleNode("/omtdx/ncTools/ncTools");

                        if (noderead1 != null)
                        {
                            var folder = noderead1.Attributes["folder"].Value;


                            switch (folder)
                            {
                                // ------------------Fräswerkzeuge
                                case "Planfräser / Messerköpfe":
                                    noderead1.Attributes[0].Value = "7000 - Planfräser / Messerköpfe";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Schaftfräser":
                                    noderead1.Attributes[0].Value = "7003 - Schaftfräser";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Kugelfräser / Ballfräser":
                                    noderead1.Attributes[0].Value = "7004 - Kugelfräser / Ballfräser";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Formfräser / Sonderfräswerkzeuge":
                                    noderead1.Attributes[0].Value = "7006 - Formfräser / Sonderfräswerkzeuge";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Gewindefräser":
                                    noderead1.Attributes[0].Value = "7008 - Gewindefräser";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Scheibenfräser und Sägeblätter":
                                    noderead1.Attributes[0].Value = "7009 - Scheibenfräser und Sägeblätter";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Tonnen- / Linsenfräser":
                                    noderead1.Attributes[0].Value = "7011 - Tonnen- / Linsenfräser";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Radienfräser":
                                    noderead1.Attributes[0].Value = "7012 - Radienfräser";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "T-Nutenfräser":
                                    noderead1.Attributes[0].Value = "7014 - T-Nutenfräser";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                //------------------------------Bohrwerkzeuge--------------------

                                case "NC-Anbohrer":
                                    noderead1.Attributes[0].Value = "7101 - NC-Anbohrer";
                                    break;

                                case "Bohrer":
                                    noderead1.Attributes[0].Value = "7101 - Bohrer";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Reibahlen":
                                    noderead1.Attributes[0].Value = "7102 - Reibahlen";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Spindler / Ausdreher":
                                    noderead1.Attributes[0].Value = "7103 - Spindler / Ausdreher";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Zentrierbohrer":
                                    noderead1.Attributes[0].Value = "7104 - Zentrierbohrer";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Formbohrer & Reibahlen":
                                    noderead1.Attributes[0].Value = "7105 - Formbohrer & Reibahlen";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                //------------------------------Gewindebohrer-/former--------------------

                                case "Metrische-Gewinde":
                                    noderead1.Attributes[0].Value = "7200 - Metrische-Gewinde";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Zoll-Gewinde":
                                    noderead1.Attributes[0].Value = "7201 - Zoll-Gewinde";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Sonder-Gewinde":
                                    noderead1.Attributes[0].Value = "7202 - Sonder-Gewinde";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                //------------------------------Senk und Faswerkzeuge --------------------

                                case "Senk-Werkzeuge":
                                    noderead1.Attributes[0].Value = "7300 - Senk-Werkzeuge";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Entgratfäser (nur Winkel scheidend)":
                                    noderead1.Attributes[0].Value = "7303 - Entgratfräser (nur Winkel scheidend)";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Fasenfräser (mit Umfangsscheide)":
                                    noderead1.Attributes[0].Value = "7304 - Fasenfräser (mit Umfangsschneide)";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                case "Fasen- und Schriftstichel":
                                    noderead1.Attributes[0].Value = "7305 - Fasen- und Schriftstichel";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                //------------------------------Messtaster --------------------

                                case "Messtaster":
                                    noderead1.Attributes[0].Value = "7500 - Messtaster";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                //------------------------------Reinigungswkz Propeller etc --------------------

                                case "Bürsten, Propeller, usw...":
                                    noderead1.Attributes[0].Value = "7600 - Bürsten, Propeller, usw...";
                                    _ = sb.Append("\n" + " --> Klasse " + noderead1.Attributes[0].Value + " gefunden -> Ordner aktualisiert");
                                    break;

                                //------------------------------Wenns mal wieder schiefläuft --------------------
                                default:
                                    _ = sb.Append("\n" + " --> Klasse für Ordner nicht gefunden ");
                                    break;
                            }
                        }
                        xmlDoc.Save(path1);
                    }

                    // ================================================================================FEATURE 5 CHECK========================================================================================
                    if (Properties.Settings.Default.optionsT1_Feature5_check == true)
                    {
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load(path1);             // xml laden

                        // Lesen von Werkzeugstatus (funktiniert nur solange nicht mehr Unterknoten im Knoten param drinnen sind)
                        XmlNode noderead1 = xmlDoc.SelectSingleNode("/omtdx/ncTools/ncTools/ncTools/ncTool/customData/param");

                        string folderspezial = "SONDERWKZS (Bestand prüfen)";

                        if (noderead1 != null)
                        {
                            var Status = noderead1.Attributes["value"].Value;

                            switch (Status)
                            {
                                case "Freigegeben":
                                    _ = sb.Append("\n" + " --> Status Freigegeben gefunden -> wird nicht in Ordner '" + folderspezial + "' hinzugefügt");
                                    break;


                                case "FAVORIT":
                                    _ = sb.Append("\n" + " --> Klasse FAVOURIT gefunden -> wird nicht in Ordner '" + folderspezial + "' hinzugefügt");
                                    break;



                                default:

                                    // nodepath
                                    XmlNode root = xmlDoc.SelectSingleNode("omtdx/ncTools");

                                    //Create a deep clone.  The cloned node
                                    //includes the child nodes.
                                    XmlNode deep = root.CloneNode(true);

                                    //Add the deep clone to the document.
                                    root.InsertBefore(deep, root.FirstChild);

                                    //remove the old node
                                    root.RemoveChild(root.LastChild);

                                    //Create a new attribute.
                                    XmlNode root1 = xmlDoc.SelectSingleNode("omtdx/ncTools/ncTools");
                                    string ns = root1.GetNamespaceOfPrefix("ncTools");
                                    XmlNode attr = xmlDoc.CreateNode(XmlNodeType.Attribute, "folder", ns);
                                    attr.Value = folderspezial;

                                    //Add the attribute to the document.
                                    root1.Attributes.SetNamedItem(attr);


                                    _ = sb.Append("\n" + " --> Sonderstatus gefunden -> in Ordner '" + folderspezial + "' hinzugefügt");

                                    break;

                            }

                            xmlDoc.Save(path1);
                        }



                    }

                    // ================================================================================verschiebe Datei=======================================================================================
                    if (System.IO.Directory.Exists(TARGETXML))
                    {
                        File.Move(xmlList[0], @TARGETXML + filename);
                        _ = sb.Append("\n" + " --> File moved to: " + @TARGETXML);
                    }
                    else
                    {
                        Directory.CreateDirectory(@TARGETXML);
                        File.Move(xmlList[0], @TARGETXML + filename);
                        _ = sb.Append("\n" + " --> Created Directory " + @TARGETXML + " and moved File");
                    }

                    // ==================================================================================== FINI =============================================================================================
                    _ = sb.Append("\n" + "==========================  Fini " + filename + "  ========================== \n");
                    // schreibe Infos in settingsdatei (für Ausgabefenster)
                    Properties.Settings.Default.syncxml_Info = sb.ToString();
                    // infos in log schreiben
                    if (Directory.Exists(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/"))
                    {
                        StreamWriter myWriter = File.CreateText(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/" + filnamewithoutExtension + ".log");
                        myWriter.WriteLine(sb.ToString());
                        myWriter.Close(); // schließe die zu schreibende Datei
                    }
                    else
                    {
                        Directory.CreateDirectory(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/");
                        StreamWriter myWriter = File.CreateText(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/" + filnamewithoutExtension + ".log");
                        myWriter.WriteLine(sb.ToString());
                        myWriter.Close(); // öffne die zu schreibende Datei
                    }

                    anzahlxml--;
                }
            }
            // =======================================================================    Fehler abfangen    =========================================================================================
            catch (Exception u)
            {
                _ = MessageBox.Show("" + u);
            }
        }

        public void Btn_Stop_sync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Begin watching.      
                FSW.EnableRaisingEvents = false;

                // Buttons available
                btn_Start_sync.IsEnabled = true;
                btn_stop_sync.IsEnabled = false;
                btn_option_start_sync.IsEnabled = true;

                // write Status to xaml
                status.Content = "FILEWATCHER STOPPED";
                status.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x2B, 0x2B));   // #FF FF 2B 2B rot
            }
            catch (Exception u)
            {
                MessageBox.Show("" + u);
            }
        }

        private void Btn_option_sync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Options_T1 window = new();


                // Haupfenster als Eigentümer festlegn
                Window mainWindow = Application.Current.MainWindow;
                window.Owner = mainWindow;


                _ = window.ShowDialog();      // Methode wo das Haupfenster gesperrt wird
            }
            catch (Exception u)
            {
                MessageBox.Show("" + u);
            }
        }

        private void Btn_showLog(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/"))
                {
                    Process.Start("explorer.exe", Properties.Settings.Default.optionsT1_destination + "log_sync_xml");
                }
                else
                {
                    Directory.CreateDirectory(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/");
                    Process.Start("explorer.exe", Properties.Settings.Default.optionsT1_destination + "log_sync_xml");
                }
            }
            catch (Exception u)
            {
                _ = MessageBox.Show("" + u);
            }

        }

        #endregion




        #region ==============================================running-VC==============================================
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                FilePath.Text = openFileDlg.FileName;
                //TextBlock1.Text = System.IO.File.ReadAllText(openFileDlg.FileName);
            }

        }
        private void Btn_runningVc(object sender, RoutedEventArgs e)
        {


            // SQL Datenbank öffnen START------------------------------------------
            SqlConnection connection = DBUtils.GetDBConnection();
            connection.Open();
            // SQL Datenbank öffnen ENDE-------------------------------------------



            try                                                                              // fehler abfangen
            {
                // Init ----------------------------------------------------------------------------------------------------------------
                string text = FilePath.Text;
                var filereadVCOrginal = text;                          // oginal Datei
                var filereadVCClean = @"tmp_CLEAN.txt";                // geschriebende Datei ohne Zeilennummerierung
                string filewriteVC = @"tmp_result_Vc1.txt";
                string filewriteVC2 = @"tmp_result_Vc2.txt";

                string[] sPattern = new string[6];                 // zu suchende Schluesselwoerter
                sPattern[0] = "TOOL CALL";
                sPattern[1] = ";";
                sPattern[2] = " *- ";
                sPattern[3] = "FN 0:Q10";
                sPattern[4] = "FN 0:Q11";
                sPattern[5] = "FN 0:Q12";

                string placeholder = " --------------------------------------------------------------------------------------------- ";

                StringBuilder sb1 = new();                                 // stringbuilder für kommentare
                File.WriteAllText(filereadVCClean, string.Empty);          // tmp Textdatei leeren
                Properties.Settings.Default.scanVC_Info = " ";             // Infofenster leeren 
                // -----------------------------------------------------------------------------------------------------------------------



                // START ZeilenNr aus Ursprungsdatei löschen START       -----------------------
                string line;
                int counter = 0;
                using (var sr = new StreamReader(filereadVCOrginal))                     // öffne eine zu lesende Datei
                using (var sw = new StreamWriter(filereadVCClean))                       // öffne die zu schreibende Datei
                
                    while ((line = sr.ReadLine()) != null)
                    {
                        counter++;

                        string result = line.TrimStart(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ' });  // alle Zeichen enhaltenen Zeichen im Array werden am Anfang gelöscht
                                                                                                                               // merke die Methode ändert den string nicht - daher in neuen string result geschrieben
                        sw.WriteLine(result);

                    }
                
                _ = sb1.Append("There were " + counter + " lines");
                _ = sb1.Append("\n" + " --> Zeilennummerierung geloescht");
                // END ZeilenNr aus Ursprungsdatei löschen ENDE      -------------------------



                // create a sql table START ----------------------------------------------------------------------------------------------



                /*
                SqlCommand command;
                SqlDataAdapter adapter = new SqlDataAdapter();
                String sql = "";

                sql = "Insert into WKZ (TUTOID,TUTONAME) values(3,'" + "VBNET" + "')";

                command = new SqlCommand(sql, connection);

                adapter.InsertCommand = new SqlCommand(sql, connection);
                adapter.InsertCommand.ExecuteNonQuery();

                command.Dispose();
                connection.Close();
                */


                /*
                cmd.CommandText = @"CREATE TABLE WKZ(id int identity(1,1) NOT NULL PRIMARY KEY,name INT NOT NULL,SpindleSpeed_S INT,fXY INT,fZ INT, ae FLOAT, ap FLOAT, VC INT, fz FLOAT, z INT)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO WKZ(name, SpindleSpeed_S) VALUES('111111',6000)";
                cmd.ExecuteNonQuery();


                _ = sb1.Append("\n" + "SQL-Table WKZ created");
                */

                // create a sql table END  -----------------------------------------------------------------------------------------------




                // START Zerlegen des PGMs                       -------------------------
                using (var sr = new StreamReader(filereadVCClean))                   // öffne eine zu lesende Datei
                using (var sw = new StreamWriter(filewriteVC))                       // öffne die zu schreibende Datei
                using (var sw1 = new StreamWriter(filewriteVC2))


                    while ((line = sr.ReadLine()) != null)                               // so lange in der lesenden Datei nicht die letzte Zeile erreicht ist
                    {


                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[0], System.Text.RegularExpressions.RegexOptions.IgnoreCase))              // wenn TOOL in Zeile vorhanden ist
                        {
                            sw.WriteLine(placeholder);
                            sw.WriteLine(line);                                                                                                                   // schreibe diesen in den streamwriter

                            if (line.StartsWith("TOOL CALL " + '"'))
                            {
                                string NR = (line[11].ToString() + line[12].ToString() + line[13].ToString() + line[14].ToString() + line[15].ToString() + line[16].ToString());
                                sw1.WriteLine(placeholder);
                                sw1.WriteLine("Wkz-NR:   " + NR);



                                int.TryParse(NR, out int wkzID);   // string in int wandeln


                                /*

                                // SQL Anweisungen

                                // Das Statement Insert.
                                string sql = "Insert into WKZ (ID) "
                                                                 + " values (@ID) ";

                                SqlCommand cmd = connection.CreateCommand();
                                cmd.CommandText = sql;

                                // Die Parameter WKZ einfügen (mehr kürzer schreiben).
                                cmd.Parameters.Add("@WKZ", SqlDbType.Int).Value = 500;

                                // das Command ausführen ( für Delete, insert, update benutzen).
                                int rowCount = cmd.ExecuteNonQuery();

                                MessageBox.Show("Row Count affected = " + rowCount);
                                */


                            }
                            else { }

                        }   // Werkzeug ID











                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[0], System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            if (line.StartsWith("TOOL CALL " + '"'))
                            {

                                string S = line.Substring(22);
                                sw1.WriteLine("S:        " + S);
                            }
                            if (line.StartsWith("TOOL CALL S"))
                            {
                                string S = line.Substring(11);
                                sw1.WriteLine("S change:   " + S);
                            }
                            else { }
                        }   // Drehzahl

                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[3], System.Text.RegularExpressions.RegexOptions.IgnoreCase))         // wenn dann
                        {
                            sw.WriteLine(line);

                            string XY = (line[9].ToString() + line[10].ToString() + line[11].ToString() + line[12].ToString() + line[13].ToString());
                            sw1.WriteLine("XY:       " + XY);

                        }   // XY Vorschub finden und schreiben

                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[4], System.Text.RegularExpressions.RegexOptions.IgnoreCase))         // wenn dann
                        {
                            sw.WriteLine(line);

                            string XY = (line[9].ToString() + line[10].ToString() + line[11].ToString() + line[12].ToString() + line[13].ToString());
                            sw1.WriteLine("Z:        " + XY);

                        }   // Z Vorschub finden und schreiben 

                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[5], System.Text.RegularExpressions.RegexOptions.IgnoreCase))         // wenn dann
                        {
                            sw.WriteLine(line);

                            string XY = (line[9].ToString() + line[10].ToString() + line[11].ToString() + line[12].ToString() + line[13].ToString());
                            sw1.WriteLine("redu:     " + XY);

                        }   // reduzierten Vorschub finden und schreiben

                        else
                        {

                        }

                    }



                tb_AusgabeVCINFO.Text = System.IO.File.ReadAllText(filewriteVC);
                tb_INFO.Text = "Scan successful";                                    // Kommentarzeile schreiben
                _ = sb1.Append("\n" + "Scan successful");
                tb_INFO.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0xF1, 0x1D));   // #FF32F11D grün
                Properties.Settings.Default.scanVC_Info = sb1.ToString();    // schreibe Infos in settingsdatei (für Ausgabefenster)







            }

            // ---------------------------------------------------------------------------------
            
            catch (Exception u)     // ausfuehren falls Fehler
            {
                MessageBox.Show("" + u);
            }
            finally
            {
                // die Verbindung schließen.
                connection.Close();
                // das Objekt absagen, die Ressourcen freien.
                connection.Dispose();
                connection = null;
            }

        }
        private void Btn_openresults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileToOpen = "tmp_result_Vc1.txt";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = fileToOpen
                    }
                };
                process.Start();
                process.WaitForExit();
            }

            // ausfuehren falls Fehler
            catch (Exception u)
            {
                tb_INFO.Text = "" + u;
                tb_AusgabeVCINFO.Text = " ";
            }
        }
        private void Btn_openresults2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileToOpen = "tmp_result_Vc2.txt";
                //var fileToOpen = "tmp_CLEAN.txt";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = fileToOpen
                    }
                };
                process.Start();
                process.WaitForExit();
            }

            // ausfuehren falls Fehler
            catch (Exception u)
            {
                tb_INFO.Text = "" + u;
                tb_AusgabeVCINFO.Text = " ";
            }
        }





        #endregion




        private void Test_Click(object sender, RoutedEventArgs e)
        {

            try
            { 





            }



            catch (Exception u)
            {
                MessageBox.Show("" + u);
            }




        }


    }
}