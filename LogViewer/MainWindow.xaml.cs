using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
//using OxyPlot;
using OxyPlot.Series;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
        }

        private void LoadLog_Click(object sender, RoutedEventArgs e)
        {
            // @"C:\Users\Greg\Downloads\datalog19.csv"

            LogFile lf = new LogFile();
            lf.plot = plot;
            lf.listBox = listOfChkboxes;
            lf.LoadFromCSV(@"C:\Users\Greg\Downloads\datalog19.csv");
            
            plot.InvalidatePlot();
        }

    }

    public class LogFile
    {
        public List<string> Columns { get { return columns.ToList(); } }
        public OxyPlot.Wpf.Plot plot;
        public ListBox listBox;
        private string[] columns;
        private Random r = new Random();
        private List<Limit> limits = new List<Limit>();

        public void LoadFromCSV(string file)
        {

            // Read lines from source file
            string[] arr = System.IO.File.ReadAllLines(file);

            columns = arr[0].Split(',');
            int columntCount = columns.Count();

            plot.Title = file;
            plot.TitleFontSize = 12;
            plot.LegendTitle = "Legend";
            plot.LegendTitleFontSize = 9;

            for (int i = 0; i < columntCount; i++)
            {
                LineSeries lineSeries = new LineSeries();
                for (int ii = 1; ii < arr.Length; ii++)
                {
                    lineSeries.Points.Add(
                        new OxyPlot.DataPoint(
                            Double.Parse(arr[ii].Split(',')[0]),
                            Double.Parse(arr[ii].Split(',')[i]
                            )));
                }

                OxyPlot.Wpf.TwoColorLineSeries twoColorSeries = new OxyPlot.Wpf.TwoColorLineSeries
                {
                    ItemsSource = lineSeries.Points,
                    Title = columns[i],
                    Limit = r.Next(100),
                    Color = Color.FromRgb((byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString())),
                    Color2 = Color.FromRgb((byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString())),
                    Visibility = Visibility.Hidden
                };

                Limit l = new Limit
                {
                    name = columns[i],
                    value = r.Next(100)
                };

                limits.Add(l);

                plot.Series.Add(twoColorSeries);

                if (columns[i] != "Time (sec)")
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Content = columns[i]
                    };
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += CheckBox_Unchecked;
                    listBox.Items.Add(checkBox);
                }
            }

            //SaveLimits();
            using (XmlReader reader = XmlReader.Create("limits.xml"))
            {
                Limit limit = new Limit();
                while (reader.Read())
                {
                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "limit":
                                // Detect this element.
                                Console.WriteLine("limit");
                                reader.Read();
                                if (reader.Read())
                                {
                                    Console.WriteLine("  Name node: " + reader.Value.Trim());
                                }
                                reader.Read(); reader.Read();
                                if (reader.Read())
                                {
                                    Console.WriteLine("  Value node: " + reader.Value.Trim());
                                }
                                break;
                        }
                    }
                }
            }

            //limits.Serialize();
        }

        private void SaveLimits()
        {
            using (XmlWriter writer = XmlWriter.Create("limits.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("limits");

                foreach (Limit limit in limits)
                {
                    writer.WriteStartElement("limit");

                    writer.WriteElementString("Name", limit.name);
                    writer.WriteElementString("Value", limit.value.ToString());


                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        #region Events
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in plot.Series)
            {
                if (item.Title == (sender as CheckBox).Content.ToString())
                {
                    item.Visibility = Visibility.Hidden;
                    plot.InvalidatePlot();
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (OxyPlot.Wpf.TwoColorLineSeries item in plot.Series)
            {
                if (item.Title == (sender as CheckBox).Content.ToString())
                {
                    item.Visibility = Visibility.Visible;
                    item.Color = Color.FromRgb((byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString()));
                    item.Color2 = Color.FromRgb((byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString()), (byte)Int32.Parse((r.Next(0, 255)).ToString()));
                    plot.InvalidatePlot();
                }
            }
        } 
        #endregion
    }

    public class Limit
    {
        public string name;
        public double value;

    }

    public static class XmlExtension
    {
        public static string Serialize<T>(this T value)
        {
            if (value == null) return string.Empty;

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                {
                    xmlSerializer.Serialize(xmlWriter, value);
                    return stringWriter.ToString();
                }
            }
        }
    }
}
