using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly Regex _rxPlate = new(@"^1[A-Z]{3}-\d{3}$");
        private string LastSelectedPlate = string.Empty;
        private bool LastSelectedTagged = false; // false for allPlates, true for tagged

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

        #region InputCheck
        private bool CheckInput(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            return true;
        }

        private bool CheckRegex(string input)
        {
            if (_rxPlate.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckDuplicates(string input)
        {
            foreach (string plate in lbxAllPlates.Items)
            {
                if (input.Equals(plate))
                {
                    return false;
                }
            }
            foreach (string plate in lbxTagged.Items)
            {
                if (input.Equals(plate))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region SortSearch
        private List<string> SortList(List<string> list)
        {
            int iterations = list.Count;
            for (int i = 0; i < iterations - 1; i++)
            {
                for (int j = 0; j < iterations - i - 1; j++)
                {
                    if(String.Compare(list[j], list[j+1]) > 0) {
                        string temp = list[j + 1];
                        list[j+1] = list[j];
                        list[j] = temp;
                    }
                }
            }
            return list;
        }

        private int BinarySearch(List<string> list, string searchTerm)
        {
            int min = 0, max = list.Count-1, mid = 0;
            if (max < 0)
            {
                return -1;
            }
            while (min <= max) // the binary search itself
            {
                mid = (min + max) >> 1; // halves the integer using bitshift
                if (String.Compare(list[mid], searchTerm) == 0)
                {
                    return mid; // once a match is found, returns the index of the match and exits the function
                }

                if (String.Compare(list[mid], searchTerm) < 0) // shouldn't go lower, so sets minimum to one more than itself
                {
                    min = mid + 1;
                }
                else // shouldn't go higher, so sets max to one less than itself
                {
                    max = mid - 1;
                }
            }

            return -1;
        }

        private int SequentialSearch(List<string> list, string searchTerm)
        {
            int iterations = list.Count;
            if (iterations == 0)
            {
                return -1;
            }
            for (int i = 0; i < iterations; i++)
            {
                if(searchTerm.Equals(list[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

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
                success = CheckRegex(InPlate);
                if (success)
                {
                    success = CheckDuplicates(InPlate);
                    if (success)
                    {
                        lbxAllPlates.Items.Add(InPlate);
                    }
                    else
                    {
                        DisplayError("Error: Duplicate Plate Found\nPlease ensure all plates are unique", "Input Error");
                    }
                }
                else
                {
                    DisplayError("Error: Invalid Plate Format\nPlease provide a valid licence plate.", "Input Error");
                }
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
            if (!String.IsNullOrWhiteSpace(LastSelectedPlate))
            {
                if (LastSelectedTagged)
                {
                    lbxAllPlates.Items.Add(lbxTagged.SelectedItem);
                    lbxAllPlates.SelectedItem = lbxTagged.SelectedItem;
                    lbxTagged.Items.Remove(lbxTagged.SelectedItem);
                }
                else
                {
                    lbxTagged.Items.Add(lbxAllPlates.SelectedItem);
                    lbxTagged.SelectedItem = lbxAllPlates.SelectedItem;
                    lbxAllPlates.Items.Remove(lbxAllPlates.SelectedItem);
                }
            }
            else
            {
                DisplayError("Error: Selection is Empty\nPlease select a plate to tag/untag", "Selection Error");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAllPlates.SelectedItem != null)
            {
                lbxAllPlates.Items.Remove(lbxAllPlates.SelectedItem);
            }
            else
            {
                DisplayError("Error: No Plate Selected","Selection Error");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(LastSelectedPlate))
            {
                string EditPlate = tbEdit.Text;
                ListView currentList = lbxAllPlates;
                if (LastSelectedTagged)
                {
                    currentList = lbxTagged;
                }
                bool success = CheckInput(EditPlate);
                if (success)
                {
                    success = CheckRegex(EditPlate);
                    if (success)
                    {
                        success = CheckDuplicates(EditPlate);
                        if (success)
                        {
                            currentList.Items.Insert(currentList.SelectedIndex, EditPlate);
                            currentList.Items.Remove(currentList.SelectedItem);
                            currentList.SelectedItem = EditPlate;
                        }
                        else
                        {
                            DisplayError("Error: Duplicate Plate Found\nPlease ensure all plates are unique", "Input Error");
                        }
                    }
                    else
                    {
                        DisplayError("Error: Invalid Plate Format\nPlease provide a valid licence plate.", "Input Error");
                    }
                }
                else
                {
                    DisplayError("Error: No Plate Provided\nPlease provide a valid licence plate.", "Input Error");
                }
                tbEdit.Text = string.Empty;
                tbEdit.Focus();
            }
            else
            {
                DisplayError("Error: No Plate Selected", "Selection Error");
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbSearch.Text))
            {
                if (lbxAllPlates.Items.Count > 0 || lbxTagged.Items.Count > 0)
                {
                    if (tbtnBinary.IsChecked == true)
                    {
                        // sorts each list first
                        List<string> sorted = new List<string>();
                        foreach (string s in lbxAllPlates.Items)
                        {
                            sorted.Add(s);
                        }
                        sorted = SortList(sorted);
                        lbxAllPlates.Items.Clear();
                        foreach (string s in sorted)
                        {
                            lbxAllPlates.Items.Add(s);
                        }

                        sorted.Clear();
                        foreach (string s in lbxTagged.Items)
                        {
                            sorted.Add(s);
                        }
                        sorted = SortList(sorted);
                        lbxTagged.Items.Clear();
                        foreach (string s in sorted)
                        {
                            lbxTagged.Items.Add(s);
                        }
                        // then searches
                        List<string> searching = new List<string>();
                        foreach (string s in lbxAllPlates.Items)
                        {
                            searching.Add(s);
                        }
                        int index = BinarySearch(searching, tbSearch.Text);
                        if (index == -1)
                        {
                            searching.Clear();
                            foreach (string s in lbxTagged.Items)
                            {
                                searching.Add(s);
                            }
                            index = BinarySearch(searching, tbSearch.Text);
                            if (index == -1)
                            {
                                DisplayError("Error: No Match Found\nPlate does not exist in database", "Match Not Found");
                            }
                            else
                            {
                                lbxTagged.SelectedIndex = index;
                                DisplayMessage($"Plate found in database!\nPlate is tagged at index {index}", "Match Found");
                            }
                        }
                        else
                        {
                            lbxAllPlates.SelectedIndex = index;
                            DisplayMessage($"Plate found in database!\nPlate is untagged at index {index}", "Match Found");
                        }
                    }
                    else if (tbtnSequential.IsChecked == true)
                    {
                        List<string> searching = new List<string>();
                        foreach (string s in lbxAllPlates.Items)
                        {
                            searching.Add(s);
                        }
                        int index = SequentialSearch(searching, tbSearch.Text);
                        if (index == -1) // swaps to tagged list if not found in untagged. no duplicates should exist in system
                        {
                            searching.Clear();
                            foreach (string s in lbxTagged.Items)
                            {
                                searching.Add(s);
                            }
                            index = SequentialSearch(searching, tbSearch.Text);
                            if (index == -1)
                            {
                                DisplayError("Error: No Match Found\nPlate does not exist in database", "Match Not Found");
                            }
                            else
                            {
                                lbxTagged.SelectedIndex = index;
                                DisplayMessage($"Plate found in database!\nPlate is tagged at index {index}", "Match Found");
                            }
                        }
                        else
                        {
                            lbxAllPlates.SelectedIndex = index;
                            DisplayMessage($"Plate found in database!\nPlate is untagged at index {index}","Match Found");
                        }
                    }
                }
                else
                {
                    DisplayError("Error: Lists are Empty!\nPlease ensure searching occurs with at least one list populated.", "Search Error");
                }
            }
            else
            {
                DisplayError("Error: Input is Empty\nPlease enter a valid licence plate to search", "Input Error");
            }
            tbSearch.Text = string.Empty;
        }

        private void lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == lbxAllPlates)
            {
                if (lbxAllPlates.SelectedItem != null)
                {
                    LastSelectedPlate = lbxAllPlates.SelectedItem.ToString();
                    LastSelectedTagged = false;
                }
                else
                {
                    LastSelectedPlate = String.Empty;
                }
                tbSelect.Text = LastSelectedPlate;
                tbTag.Text = "Untagged";
            }
            else if (sender == lbxTagged)
            {
                if (lbxTagged.SelectedItem != null)
                {
                    LastSelectedPlate = lbxTagged.SelectedItem.ToString();
                    LastSelectedTagged = true;
                }
                else
                {
                    LastSelectedPlate = String.Empty;
                }
                tbSelect.Text = LastSelectedPlate;
                tbTag.Text = "Tagged";
            }
        }

        private void lbxFocus(object sender, RoutedEventArgs e)
        {
            if (sender == lbxAllPlates)
            {
                if (lbxAllPlates.SelectedItem != null)
                {
                    LastSelectedPlate = lbxAllPlates.SelectedItem.ToString();
                }
                else
                {
                    LastSelectedPlate = String.Empty;
                }
                tbSelect.Text = LastSelectedPlate;
                tbTag.Text = "Untagged";
            }
            else if (sender == lbxTagged)
            {
                if (lbxTagged.SelectedItem != null)
                {
                    LastSelectedPlate = lbxTagged.SelectedItem.ToString();
                }
                else
                {
                    LastSelectedPlate = String.Empty;
                }
                tbSelect.Text = LastSelectedPlate;
                tbTag.Text = "Tagged";
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnAllReset)
            {
                lbxAllPlates.Items.Clear();
                lbxTagged.Items.Clear();
            }
            else if (sender == btnTaggedReset)
            {
                foreach(var item in lbxTagged.Items)
                {
                    lbxAllPlates.Items.Add(item);
                }
                lbxTagged.Items.Clear();
            }
        }
    }
}