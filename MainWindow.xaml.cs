
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RagemakerToPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            status.Text = "Status:";
        }

        private async void LoadRageXML_Click(object sender, RoutedEventArgs e)
        {

            Ragecomic comic = new Ragecomic();

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            //ofd.InitialDirectory = ".";
            ofd.RestoreDirectory = true;
            ofd.Filter = "Ragemaker .xml file (*.xml)|*.xml";
            ofd.Title = "Select ragemaker .xml file to convert to PDF";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = ofd.FileNames[0];

                // Open File
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.Async = true;

                    // Read XML
                    using (XmlReader reader = XmlReader.Create(fs, settings))
                    {

                        while (await reader.ReadAsync())
                        {
                            string elementName = reader.Name;
                            switch (elementName)
                            {   
                                /*
                                 * <panels>8</panels>
                                 * <gridLinesArray>111001110011100</gridLinesArray>
                                 * <gridAboveAll>true</gridAboveAll>
                                 * <showGrid>true</showGrid>
                                 * <redditWatermark>false</redditWatermark>
                                 */
                                case "panels":
                                    int panels = int.Parse(reader.ReadInnerXml());
                                    status.Text += "panels: " + panels.ToString() + "\n";
                                    comic.panels = panels;
                                    break;
                                case "gridLinesArray":
                                    string gridLinesArray = reader.ReadInnerXml();
                                    status.Text += "gridLinesArray: " + gridLinesArray+ "\n";
                                    comic.gridLines = new bool[gridLinesArray.Length];
                                    for(var i = 0; i < gridLinesArray.Length; i++)
                                    {
                                        comic.gridLines[i] = gridLinesArray[i].ToString() == "1" ? true: false;
                                        //status.Text += comic.gridLines[i] + "\n";
                                    }
                                    //comic.panels = panels;
                                    break;
                                case "gridAboveAll":
                                    comic.gridAboveAll = bool.Parse(reader.ReadInnerXml());
                                    status.Text += "gridAboveAll: " + comic.gridAboveAll + "\n";
                                    break;
                                case "showGrid":
                                    comic.showGrid = bool.Parse(reader.ReadInnerXml());
                                    status.Text += "showGrid: " + comic.showGrid + "\n";
                                    break;
                                case "redditWatermark":
                                    comic.redditWatermark = bool.Parse(reader.ReadInnerXml());
                                    status.Text += "redditWatermark: " + comic.redditWatermark + "\n";
                                    break;
                                case "Face":
                                case "Text":
                                case "Draw":
                                case "Image":
                                    if (reader.NodeType == XmlNodeType.EndElement) break;
                                    using(XmlReader subtreeReader = reader.ReadSubtree())
                                    {
                                        RageItem newItem = await RageItem.createRageItem(elementName, subtreeReader);
                                        comic.items.Add(newItem);
                                        status.Text += "new item: " + newItem.ToString() + "\n";
                                    }                                    
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                status.Text += filename + " was parsed successfully.\n\n\nProceeding to PDF generation\n\n\n";

                RagecomicToPDF.MakePDF(comic, filename + ".pdf");

            }
            else
            {
                MessageBox.Show("error");
            }

            Thread thread = new Thread(LoadAndProcessRageXML);
            thread.Start();

            
        }

        private void LoadAndProcessRageXML()
        {
            
        }
    }
}
