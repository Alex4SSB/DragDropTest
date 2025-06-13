using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Vanara.Windows.Shell;

namespace DragDropTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, object> data = new();

        private IDataObject dataObject;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            var selected = ListBox1.Text;

            data.Clear();

            dataObject = e.Data;
            var formats = e.Data.GetFormats();

            ListBox1.ItemsSource = formats;
            
            data = formats.ToDictionary(f => f, f => f == "FileContents" ? new[] { f } : e.Data.GetData(f));

            if (selected is not null && formats.Any(f => f == selected))
                ListBox1.SelectedItem = selected;
            else
                ListBox1.SelectedIndex = 0;

            ShowData();
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowData();
        }

        private void ShowData()
        {
            try
            {
                switch (data[(string)ListBox1.SelectedItem])
                {
                    case string str:
                        TextBox1.Text = str;
                        break;
                    case string[] strArr:
                        if ((string)ListBox1.SelectedItem == "FileContents")
                        {
                            //var shdo = new ShellDataObject(dataObject);

                        }
                        else
                        {
                            LengthBlock.Text = "Array length: " + strArr.Length;
                            TextBox1.Text = string.Join("\n", strArr);
                        }
                        break;
                    case MemoryStream stream:
                        byte[] bytes = stream.ToArray();
                        LengthBlock.Text = "Stream length: " + bytes.Length;
                        if (bytes.Length < 100000)
                        {
                            if (CheckBox1.IsChecked is true)
                            {
                                if ((string)ListBox1.SelectedItem == "Shell IDList Array")
                                {
                                    var shia = ShellItemArray.FromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject);
                                    if (shia is not null)
                                    {
                                        var sho = ShellItem.Open(shia[0].ParsingName);

                                        ShellFileOperations shFileOp = new(IntPtr.Zero);

                                        if (sho.IsFileSystem)
                                            TextBox1.Text = string.Join('\n', shia.Select(s => s.ParsingName));
                                        else
                                            TextBox1.Text = string.Join('\n', shia.Select(s => s.GetDisplayName(ShellItemDisplayString.DesktopAbsoluteEditing)));
                                    }
                                }
                                else
                                    TextBox1.Text = Encoding.Unicode.GetString(bytes);
                            }
                            else
                            {
                                TextBox1.Text = BitConverter.ToString(bytes);
                            }
                        }
                        else
                            TextBox1.Text = "-- STREAM TOO LONG --";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            ShowData();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = ListBox1.Text;

            data.Clear();
            
            dataObject = Clipboard.GetDataObject();
            if (dataObject is null)
                return;

            var formats = dataObject.GetFormats();

            ListBox1.ItemsSource = formats;

            data = formats.ToDictionary(f => f, f => f == "FileContents" ? new[] { f } : dataObject.GetData(f));

            if (selected is not null && formats.Any(f => f == selected))
                ListBox1.SelectedItem = selected;
            else
                ListBox1.SelectedIndex = 0;

            ShowData();
        }
    }
}