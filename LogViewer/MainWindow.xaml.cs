using System;
using System.Collections.Generic;
using System.Data;
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
        private DataSet dataSet = new DataSet();

        public void LoadFromCSV(string file)
        {
            listBox.Items.Clear();
            plot.Series.Clear();

            // Read lines from source file
            string[] arr = System.IO.File.ReadAllLines(file);
            LoadLimits();

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
                    Limit = GetLimitValue(columns[i]),
                    Color = ColorMe(),
                    Color2 = ColorMe(),
                    Visibility = Visibility.Hidden
                };

                plot.Series.Add(twoColorSeries);

                if (columns[i] != "Time (sec)")
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Content = columns[i]
                    };
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += CheckBox_Unchecked;
                    checkBox.MouseRightButtonUp += CheckBox_MouseRightButtonUp;

                    if (GetLimitValue(columns[i]) == -1)
                    {
                        checkBox.Background = Brushes.Red;
                    }
                    

                    listBox.Items.Add(checkBox);
                }
            }

            

            //limits.Serialize();
        }

        private void CheckBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            NewLimit newLimit = new NewLimit();
            newLimit.Title += (sender as CheckBox).Content.ToString();
            newLimit.name = (sender as CheckBox).Content.ToString();
            newLimit.value = GetLimitValue((sender as CheckBox).Content.ToString()).ToString();
            newLimit.Update();
            newLimit.ShowDialog();

            if (newLimit.newValue.Trim().Length > 0)
            {
                SetLimitValue((sender as CheckBox).Content.ToString(), double.Parse(newLimit.newValue));
            }
        }

        private void LoadLimits()
        {
            
            dataSet.ReadXml("limits.xml", XmlReadMode.InferSchema);

            foreach (DataTable table in dataSet.Tables) // not really needed.
            {
                foreach (DataRow row in table.AsEnumerable())
                {
                    Limit l = new Limit
                    {
                        name = row[0].ToString(),
                        value = double.Parse(row[1].ToString())
                    };
                    limits.Add(l);
                }
            }
        }

        private double GetLimitValue(string name)
        {
            var o = from j in limits
                    where j.name == name
                    select j.value;
            return o.Count() == 1 ? o.First() : -1;
        }

        private void SetLimitValue(string name, double value)
        {
            Limit o = (from j in limits
                    where j.name == name
                    select j).SingleOrDefault();

            if (o != null)
            {
                o.value = value;
            }
            else
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    DataRow dr = table.NewRow();
                    dr[0] = name;
                    dr[1] = value;
                    table.Rows.Add(dr);
                }
                dataSet.WriteXml("limits.xml");
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
                    item.Color = ColorMe();
                    item.Color2 = ColorMe();
                    plot.InvalidatePlot();
                }
            }
        }
        #endregion

        public Color ColorMe()
        {
            return Color.FromRgb(
                (byte)Int32.Parse((r.Next(0, 255)).ToString()), 
                (byte)Int32.Parse((r.Next(0, 255)).ToString()), 
                (byte)Int32.Parse((r.Next(0, 255)).ToString()));
        }
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
