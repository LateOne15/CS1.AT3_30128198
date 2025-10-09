using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicencePlateManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            tbtnBinary.IsChecked = true;
        }

        #region Messages
        private void DisplayMessage(string msg, string caption) // display non-error message
        {
            MessageBox.Show(msg, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisplayError(string msg, string caption) // display error message
        {
            MessageBox.Show(msg, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        #endregion

        private bool CheckInput(string input)
        {
            return true;
        }

        // these two methods swap the buttons depending on which one is pressed to ensure only one is enabled at a time
        private void tbtn_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == tbtnBinary)
            {
                tbtnSequential.IsChecked = false;
            }
            else if (sender == tbtnSequential)
            {
                tbtnBinary.IsChecked = false;
            }
        }

        private void tbtn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == tbtnBinary)
            {
                tbtnSequential.IsChecked = true;
            }
            else if (sender == tbtnSequential)
            {
                tbtnBinary.IsChecked = true;
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            string InPlate = tbEnter.Text;
            bool success = CheckInput(InPlate);
            if (success)
            {
                lbxAllPlates.Items.Add(InPlate);
            }
            else
            {
                DisplayError("Error: No Plate Provided\nPlease provide a valid licence plate.", "Input Error");
            }
            tbEnter.Text = string.Empty;
            tbEnter.Focus();
        }

        private void btnTag_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAllPlates.SelectedItem != null)
            {
                lbxTagged.Items.Add(lbxAllPlates.SelectedItem);
                lbxAllPlates.Items.Remove(lbxAllPlates.SelectedItem);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAllPlates.SelectedItem != null)
            {

            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}