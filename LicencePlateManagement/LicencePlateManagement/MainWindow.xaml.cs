using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Lleyton Eggins, AT3
// Date: 16/10/25
// Version: 1.00
// Licence Plate Management
// Creates and/or saves a list of Licence Plates in either
// a tagged and untagged list, and displays them in the program.

namespace LicencePlateManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Regex _rxPlate = new(@"^1[A-Z]{3}-\d{3}$"); // 1, three capital letters, -, three integers
        private string LastSelectedPlate = string.Empty;
        private bool LastSelectedTagged = false; // false for allPlates, true for tagged

        public MainWindow()
        {
            InitializeComponent();
            tbtnBinary.IsChecked = true; // defaults to binary
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

        private bool DisplayYesNo(string msg, string caption) // display yes/no prompt
        {
            if (MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region InputCheck
        private bool CheckInput(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) // just ensure the input actually exists
            {
                return false;
            }
            return true;
        }

        private bool CheckRegex(string input)
        {
            if (_rxPlate.IsMatch(input)) // checks against the regex for a valid plate
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckDuplicates(string input) // if ever there is a duplicate, returns false
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

        #region SelectionCheck
        private void lbx_SelectionChanged(object sender, SelectionChangedEventArgs e) // two identical methods to catch all potential cases where the selection target changes
        {
            ChangedTarget(sender);
        }

        private void lbxFocus(object sender, RoutedEventArgs e)
        {
            ChangedTarget(sender);
        }

        private void ChangedTarget(object sender)
        {
            if (sender == lbxAllPlates)
            {
                if (lbxAllPlates.SelectedItem != null)
                {
                    LastSelectedPlate = lbxAllPlates.SelectedItem.ToString(); // the item itself
                    LastSelectedTagged = false; // changes which list it knows to target
                }
                else
                {
                    LastSelectedPlate = String.Empty; // in case of null (from deleted item generally)
                }
                tbSelect.Text = LastSelectedPlate; // changes textbox to new item
                tbTag.Text = "Untagged"; // changes text next to textbox
                tbTag.ToolTip = "Plate has not been tagged"; // changes tooltip as well
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
                tbTag.ToolTip = "Plate has been tagged";
            }
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
        #endregion

        #region SortSearch
        private void SortLists()
        {
            List<string> sorted = new List<string>();
            foreach (string s in lbxAllPlates.Items)
            {
                sorted.Add(s); // sets up a list using the untagged listbox items
            }
            sorted.Sort(); // simple sort
            lbxAllPlates.Items.Clear(); // removes the old order to put in the new one
            foreach (string s in sorted)
            {
                lbxAllPlates.Items.Add(s);
            }

            sorted.Clear();
            foreach (string s in lbxTagged.Items)
            {
                sorted.Add(s); // sets up a list using the tagged listbox items
            }
            sorted.Sort();
            lbxTagged.Items.Clear();
            foreach (string s in sorted)
            {
                lbxTagged.Items.Add(s);
            }
        }

        private int BinarySearch(List<string> list, string searchTerm)
        {
            int min = 0, max = list.Count-1, mid = 0;
            if (max < 0)
            {
                return -1;
            }
            while (min <= max) // if this ever crosses, the search has failed
            {
                mid = (min + max) >> 1; // halves the integer using bitshift
                if (String.Compare(list[mid], searchTerm) == 0) // 0 means identical
                {
                    return mid; // once a match is found, returns the index of the match and exits the function
                }

                if (String.Compare(list[mid], searchTerm) < 0) // if this is less than 0, the item is later in the list
                {
                    min = mid + 1; // shouldn't go lower, so sets minimum to one more than itself
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
                    return i; // returns the index if ever the item is found
                }
            }
            return -1;
        }

        #endregion

        #region FileIO
        private void FileSave(string filename) // performs file save operations. can work with empty lists
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("Untagged:");
                foreach (string plate in lbxAllPlates.Items)
                {
                    writer.WriteLine(plate);
                }
                writer.WriteLine("Tagged:");
                foreach (string plate in lbxTagged.Items)
                {
                    writer.WriteLine(plate);
                }
            }
        }

        private string FileIncrement() // increments day file value based on existing files
        {
            int i = 1;
            string filename = $"day_{i.ToString("D2")}.txt";
            while (File.Exists(filename)) // if that file is already created
            {
                i++;
                if (i > 99)
                {
                    filename = $"day_{i.ToString("D")}.txt"; // allows for higher numbers just in case
                }
                else
                {
                    filename = $"day_{i.ToString("D2")}.txt"; // try again one higher
                }
            }
            return filename;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) // on closing, save the file if either list contains a plate
        {
            if (lbxAllPlates.Items.Count > 0 || lbxTagged.Items.Count > 0)
            {
                string filename = FileIncrement(); // checks for existing files
                FileSave(filename); // saves with new filename
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt"; 
            dialog.FileName = "day_01.txt"; // default file name to open

            bool? result = dialog.ShowDialog();

            if (result == true) // if file was selected to open
            {
                string filename = dialog.FileName;
                List<string> contents = new List<string>();
                List<string> contentsUntag = new List<string>();
                List<string> contentsTag = new List<string>();
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null) // puts each file line into a list
                    {
                        contents.Add(line);
                    }
                }
                if (contents.Count > 2) // 3 means that at least one plate exists
                {
                    if (contents[0].Equals("Untagged:")) // makes sure the file is actually formatted correctly
                    {
                        contents.Remove(contents[0]);
                        while (contents[0] != "Tagged:" && contents.Count > 0 && !string.IsNullOrWhiteSpace(contents.First())) // until last item, or tagged list is found
                        {
                            contentsUntag.Add(contents[0]);
                            contents.Remove(contents[0]);
                        }
                        if (contents.Count > 0 && contents.First().Equals("Tagged:")) // if tagged does not exist, still opens the file but warns
                        {
                            contents.Remove(contents[0]);
                            while (contents.Count > 0 && !string.IsNullOrWhiteSpace(contents.First())) // until the end of the file or a gap
                            {
                                contentsTag.Add(contents[0]);
                                contents.Remove(contents[0]);
                            }
                            SortLists();
                        }
                        else
                        {
                            SortLists();
                            DisplayMessage("Tagged list failed to load\nPlease be aware that data may be corrupted", "Warning"); // the warning for no tagged
                        }
                    }
                    else
                    {
                        DisplayError("File is not in the correct\nformat for this program", "File Error");
                    }
                }
                else if (contents.Count == 2) // in case of only having the labels, or just a really small file
                {
                    if (contents.First().Equals("Untagged:") && contents.Last().Equals("Tagged:"))
                    {
                        DisplayMessage("File is in correct format,\nbut no plates are provided", "Warning");
                    }
                    else if (contents.First().Equals("Untagged:") && !string.IsNullOrWhiteSpace(contents.Last())) // file only has one item
                    {
                        contents.Remove(contents[0]);
                        contentsUntag.Add(contents[0]);
                        contents.Remove(contents[0]);
                        SortLists();
                        DisplayMessage("Tagged list failed to load\nPlease be aware that data may be corrupted", "Warning"); // the warning for no tagged
                    }
                    else
                    {
                        DisplayError("File is not in the correct\nformat for this program", "File Error");
                    }
                }
                else
                {
                    DisplayError("File is not in the correct\nformat for this program", "File Error");
                }
                if (contentsUntag.Count > 0 || contentsTag.Count > 0)
                {
                    bool goodData = true;
                    foreach (string plate in contentsUntag)
                    {
                        if (CheckInput(plate) && CheckDuplicates(plate) && CheckRegex(plate))
                        {
                            lbxAllPlates.Items.Add(plate);
                        }
                        else
                        {
                            goodData = false;
                        }

                    }
                    foreach (string plate in contentsTag)
                    {
                        if (CheckInput(plate) && CheckDuplicates(plate) && CheckRegex(plate))
                        {
                            lbxTagged.Items.Add(plate);
                        }
                        else
                        {
                            goodData = false;
                        }
                    }
                    if (goodData)
                    {
                        DisplayMessage("File opened successfully", "Success!");
                    }
                    else
                    {
                        DisplayMessage("Some of the provided plates were invalid for this program\nPlease ensure inputs match appropriate plate formatting", "Warning");
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAllPlates.Items.Count > 0 || lbxTagged.Items.Count > 0)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Text documents (.txt)|*.txt";
                dialog.FileName = FileIncrement(); // checks for existing filenames for the default

                bool? result = dialog.ShowDialog();

                if (result == true) // if saving was chosen
                {
                    string fileName = dialog.FileName;
                    FileSave(fileName);
                }
            }
            else
            {
                DisplayError("Error: No plates to save", "Saving error");
            }
        }
        #endregion
        
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            string InPlate = tbEnter.Text; // checks 3 conditions, with an error for each
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
                        SortLists();
                    }
                    else
                    {
                        DisplayError("Error: Duplicate Plate Found\nPlease ensure all plates are unique", "Input Error");
                        tbEnter.Focus();
                    }
                }
                else
                {
                    DisplayError("Error: Invalid Plate Format\nPlease provide a valid licence plate.", "Input Error");
                    tbEnter.Focus();
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
                if (LastSelectedTagged) // determines which list actually needs to be tagged or untagged
                {
                    lbxAllPlates.Items.Add(lbxTagged.SelectedItem); // has to add the item to one before removing it from the other
                    lbxAllPlates.SelectedItem = lbxTagged.SelectedItem; // sets the selection to the same item in the new list
                    lbxTagged.Items.Remove(lbxTagged.SelectedItem);
                }
                else
                {
                    lbxTagged.Items.Add(lbxAllPlates.SelectedItem);
                    lbxTagged.SelectedItem = lbxAllPlates.SelectedItem;
                    lbxAllPlates.Items.Remove(lbxAllPlates.SelectedItem);
                }
                SortLists(); // sorts after the fact
            }
            else
            {
                DisplayError("Error: Selection is Empty\nPlease select a plate to tag/untag", "Selection Error");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(LastSelectedPlate))
            {
                if (LastSelectedTagged) // determines which list to delete from
                {
                    lbxTagged.Items.Remove(lbxTagged.SelectedItem);
                }
                else
                {
                    lbxAllPlates.Items.Remove(lbxAllPlates.SelectedItem);
                }
                tbSelect.Focus();
                SortLists();
            }
            else
            {
                DisplayError("Error: No Plate Selected", "Selection Error");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(LastSelectedPlate)) // makes sure there something ther first
            {
                string EditPlate = tbEdit.Text;
                ListView currentList = lbxAllPlates;
                if (LastSelectedTagged)
                {
                    currentList = lbxTagged; // determines which list you're editing
                }
                bool success = CheckInput(EditPlate); // checks the three conditions, with their own error messages
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
                            SortLists();
                        }
                        else
                        {
                            DisplayError("Error: Duplicate Plate Found\nPlease ensure all plates are unique", "Input Error");
                            tbEdit.Focus();
                        }
                    }
                    else
                    {
                        DisplayError("Error: Invalid Plate Format\nPlease provide a valid licence plate.", "Input Error");
                        tbEdit.Focus();
                    }
                }
                else
                {
                    DisplayError("Error: No Plate Provided\nPlease provide a valid licence plate.", "Input Error");
                }
                tbEdit.Text = string.Empty;
                tbEdit.Focus(); // focuses back to the same box
            }
            else
            {
                DisplayError("Error: No Plate Selected", "Selection Error");
                tbEdit.Focus();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbSearch.Text)) // two error conditions, then checks which search method
            {
                if (lbxAllPlates.Items.Count > 0 || lbxTagged.Items.Count > 0)
                {
                    if (tbtnBinary.IsChecked == true) // binary search
                    {
                        // sorts each list first
                        SortLists();
                        // then searches
                        List<string> searching = new List<string>();
                        foreach (string s in lbxAllPlates.Items) // attempts with untagged first
                        {
                            searching.Add(s);
                        }
                        int index = BinarySearch(searching, tbSearch.Text); // the actual search
                        if (index == -1) // swaps to tagged list if none found
                        {
                            searching.Clear();
                            foreach (string s in lbxTagged.Items)
                            {
                                searching.Add(s);
                            }
                            index = BinarySearch(searching, tbSearch.Text);
                            if (index == -1) // if both searches fail
                            {
                                DisplayError("Error: No Match Found\nPlate does not exist in database", "Match Not Found");
                                tbSearch.Focus();
                            }
                            else
                            {
                                lbxTagged.SelectedIndex = index;
                                DisplayMessage($"Plate found in database!\nPlate is tagged at index {index}", "Match Found"); // match found!
                            }
                        }
                        else
                        {
                            lbxAllPlates.SelectedIndex = index;
                            DisplayMessage($"Plate found in database!\nPlate is untagged at index {index}", "Match Found"); // match found!
                        }
                    }
                    else if (tbtnSequential.IsChecked == true) // sequential search
                    {
                        List<string> searching = new List<string>();
                        foreach (string s in lbxAllPlates.Items) // start with untagged
                        {
                            searching.Add(s);
                        }
                        int index = SequentialSearch(searching, tbSearch.Text); // the actual search
                        if (index == -1) // swaps to tagged list if not found in untagged. no duplicates should exist in system
                        {
                            searching.Clear();
                            foreach (string s in lbxTagged.Items)
                            {
                                searching.Add(s);
                            }
                            index = SequentialSearch(searching, tbSearch.Text);
                            if (index == -1) // in case of neither
                            {
                                DisplayError("Error: No Match Found\nPlate does not exist in database", "Match Not Found");
                                tbSearch.Focus();
                            }
                            else
                            {
                                lbxTagged.SelectedIndex = index;
                                DisplayMessage($"Plate found in database!\nPlate is tagged at index {index}", "Match Found"); // match found!
                            }
                        }
                        else
                        {
                            lbxAllPlates.SelectedIndex = index;
                            DisplayMessage($"Plate found in database!\nPlate is untagged at index {index}","Match Found"); // match found!
                        }
                    }
                }
                else
                {
                    DisplayError("Error: Lists are Empty!\nPlease ensure searching occurs\nwith at least one list populated.", "Search Error");
                    tbSearch.Focus();
                }
            }
            else
            {
                DisplayError("Error: Input is Empty\nPlease enter a valid licence plate to search", "Input Error");
                tbSearch.Focus();
            }
            tbSearch.Text = string.Empty;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnAllReset) // removes all instead of just untagged
            {
                if (lbxAllPlates.Items.Count == 0 && lbxTagged.Items.Count == 0)
                {
                    DisplayError("Error: Plate Lists are Empty\nNo plates to reset","Reset Error");
                }
                else if (DisplayYesNo("Are you sure you want to remove\nall plates from both lists?","Confirm reset"))
                {
                    lbxAllPlates.Items.Clear(); // upon confirmation
                    lbxTagged.Items.Clear();
                }
            }
                
            else if (sender == btnTaggedReset) // only removes tagged, puts them back onto untagged
            {
                if (lbxTagged.Items.Count == 0)
                {
                    DisplayError("Error: Tagged Plate List is Empty\nNo plates to reset", "Reset Error");
                }
                else if (DisplayYesNo("Are you sure you want reset the tagged\nlist by moving its contents back to the\nuntagged list?","Confirm reset"))
                {
                    foreach (var item in lbxTagged.Items) // if it gets confirmation
                    {
                        lbxAllPlates.Items.Add(item);
                    }
                    lbxTagged.Items.Clear();
                }
            }
        }

        private void lbx_MouseDoubleClick(object sender, MouseButtonEventArgs e) // double click delete functionality
        {
            if (sender == lbxAllPlates && !string.IsNullOrWhiteSpace(LastSelectedPlate) && !LastSelectedTagged)
            {
                if (DisplayYesNo("Do you want to delete this plate?", "Delete plate"))
                {
                    lbxAllPlates.Items.Remove(lbxAllPlates.SelectedItem);
                    SortLists();
                }
            }
            else if (sender == lbxTagged && !string.IsNullOrWhiteSpace(LastSelectedPlate) && LastSelectedTagged)
            {
                if (DisplayYesNo("Do you want to untag this plate?", "Untag plate"))
                {
                    lbxAllPlates.Items.Add(lbxTagged.SelectedItem);
                    lbxTagged.Items.Remove(lbxTagged.SelectedItem);
                    SortLists();
                }
            }
        }
    }
}