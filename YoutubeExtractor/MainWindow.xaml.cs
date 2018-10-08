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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Syroot.Windows.IO;
using VideoLibrary;
using System.Text.RegularExpressions;
using Microsoft.Win32;

/********************************************************************************************************************************************
*                                                       YOUTUBE EXTRACTOR                                                                   *
* Windows Application - Downloads given Youtube url from text file line by line and places it in the Users downlaods folder in a Music      *
* Folder that is created if need be. Keeps the last used text file in memory and checks if it exist and if the url is good.                 *
*                                                                                                                                           *
* CREATED BY: Louis-David Côté                                                                                                              *
* CREATED DATE: 08/10/2018                                                                                                                  *
* LAST MODIFIED: 08/10/2018                                                                                                                 *
********************************************************************************************************************************************/


namespace YoutubeExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static bool FileOrDirectoryExists(string name)
        {
            return (Directory.Exists(name) || File.Exists(name));
        }
        public string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

        public MainWindow()
        {
            InitializeComponent();
            //Check if a file was previously used and reload it
            try {
                lbl_filePath.Content = Properties.Settings.Default.lbl_filePath;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

/********************************************************************************************************************************************
*                                                       CHANGE FILE PATH FUNCTION                                                           *
* Changes the File Path in the label on screen and in the users saved label path.                                                           *                                                                              *
********************************************************************************************************************************************/
        private void changeFilePath(string filePath) {
            lbl_filePath.Content = filePath;
            try {
                Properties.Settings.Default.lbl_filePath = filePath;
                Properties.Settings.Default.Save();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

/********************************************************************************************************************************************
*                                                       GET TEXT FILE FUNCTION                                                              *
* Opens a File Dialog in order to select a text file. Once selected the File Name is placed in the filePath label and saved for the users   *
* next use.                                                                                                                                 *
********************************************************************************************************************************************/
        private void lbl_filePath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = downloadsPath;
            ofd.Filter = "All Text Files (*.txt) | *.txt";
            ofd.Title = "Please Select a Text File";

            ofd.ShowDialog();
            changeFilePath(ofd.FileName);           
        }
/********************************************************************************************************************************************
*                                                       GO FUNCTION - DOWNLOAD                                                              *
* On button click disable button until the function is finished. Then validate if file exist and build a string array for each line in file *
* while adding & at the end of each line and then triming the string to the first & as to keep only www.youtube.com/watch?asdasdc           *
* So the URL can look like: www.youtube.com/watch?asdasdc&lsit=sadsa&a=asd                                                                  *                                                                 *
********************************************************************************************************************************************/
        private void btn_Go_Click(object sender, RoutedEventArgs e)
        {
            btn_Go.IsEnabled = false;
            Regex rgx = new Regex(".*&");
            string filePath = lbl_filePath.Content.ToString();
            if (FileOrDirectoryExists(filePath)) {
                List<string> urlList = new List<string>();
                StreamReader txtFile = new StreamReader(filePath);
                string line;

                while ((line = txtFile.ReadLine()) != null)
                {
                    line = line + "&";
                    line = line.Trim('&');
                    urlList.Add(line);
                }
                txtFile.Close();
                string[] url = urlList.ToArray();
                //ADD FUNCTION CLEAN URL AS TO HAVE AN ARRAY OF ONLY VALID YOUTUBE LINKS LIKE https://www.youtube.com/watch?v=IG8NfUMlt-k
                //ADD FUNCTION THAT TAKES URL ARRAY AND DOWNLOADS ALL THE FILES
                lbl_status.Foreground = Brushes.Green;
                lbl_status.Content = "Sucess!";
            } else {
                lbl_status.Foreground = Brushes.Red;
                lbl_status.Content = "Error: Given File Path Invalid.";
                changeFilePath("");
            }
            btn_Go.IsEnabled = true;
        }
    }
}
