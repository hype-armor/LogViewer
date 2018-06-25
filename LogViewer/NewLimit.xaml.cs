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

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for NewLimit.xaml
    /// </summary>
    public partial class NewLimit : Window
    {
        public string name;
        public string value;

        public NewLimit()
        {
            InitializeComponent();
            Update();
        }

        public void Update()
        {
            LimitName.Content = name;
            LimitValue.Text = value;
        }

        public string newValue;
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (value.Trim() != LimitValue.Text.Trim())
            {
                newValue = LimitValue.Text.Trim();
            }
            else
            {
                newValue = value;
            }

            this.Close();
        }
    }
}
