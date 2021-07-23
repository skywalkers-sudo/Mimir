using System;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Windows.Navigation;
using System.Threading;

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
                    _ = sb.Append("==========  neues Wkz gefunden " + filename + "  ==========");




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

                    }

                    // ================================================================================verschiebe Datei=======================================================================================
                    if (System.IO.Directory.Exists(TARGETXML))
                    {
                        System.IO.File.Move(xmlList[0], @TARGETXML + filename);
                        _ = sb.Append("\n" + " --> File moved to: " + @TARGETXML);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(@TARGETXML);
                        System.IO.File.Move(xmlList[0], @TARGETXML + filename);
                        _ = sb.Append("\n" + " --> Created Directory " + @TARGETXML + " and moved File");
                    }

                    // ==================================================================================== FINI =============================================================================================
                    _ = sb.Append("\n" + "=================  Fini " + filename + "  ================");
                    // schreibe Infos in settingsdatei (für Ausgabefenster)
                    Properties.Settings.Default.syncxml_Info = sb.ToString();
                    // infos in log schreiben
                    if (Directory.Exists(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/"))
                    {
                        StreamWriter myWriter = File.CreateText(@Properties.Settings.Default.optionsT1_destination + "log_sync_xml/" + filnamewithoutExtension + ".log");
                        myWriter.WriteLine(sb.ToString());
                        myWriter.Close(); // öffne die zu schreibende Datei
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
            // ---------------------------------------------------------------------------------
            // Variablen


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

            // ---------------------------------------------------------------------------------
            // work
            File.WriteAllText(filereadVCClean, string.Empty);                                //  tmp leeren

            try                                                                              // wenn gefunden
            {




                // START ZeilenNr aus Ursprungsdatei löschen       -----------------------
                string line;
                int counter = 0;
                using (var sr = new StreamReader(filereadVCOrginal))                     // öffne eine zu lesende Datei
                using (var sw = new StreamWriter(filereadVCClean))                       // öffne die zu schreibende Datei
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        counter++;

                        string result = line.TrimStart(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ' });  // alle Zeichen enhaltenen Zeichen im Array werden am Anfang gelöscht
                                                                                                                               // merke die Methode ändert den string nicht - daher in neuen string result geschrieben
                        sw.WriteLine(result);

                    }
                }
                tb_INFO.Text = "There were " + counter + " lines";
                tb_INFO.Text = "Zeilennummerierung geloescht";
                // END ZeilenNr aus Ursprungsdatei löschen       -------------------------





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
                            }
                            else { }

                        }   // WerkzeugNr finden und schreiben

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
                        }

                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[1], System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            if (line.StartsWith(";"))
                            {
                                sw.WriteLine(line);
                            }
                            else { }
                        }

                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern[2], System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            sw.WriteLine(line);
                        }

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










            }

            // ---------------------------------------------------------------------------------
            // ausfuehren falls Fehler
            catch (Exception u)
            {
                tb_INFO.Text = "" + u;
                tb_AusgabeVCINFO.Text = " ";
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


        #region ==============================================MAINWINDOW==============================================
        private void Btn_Info_Click(object sender, RoutedEventArgs e)
        {
            
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Properties.Settings.Default.Version = ("Version: " + version);

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