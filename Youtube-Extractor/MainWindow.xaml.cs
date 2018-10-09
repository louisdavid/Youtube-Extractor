using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

namespace YoutubeExtractor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public string downloadsFolder = new KnownFolder(KnownFolderType.Downloads).Path;
        public string downloadsPath;
        public string logPath;

        public MainWindow() {           
            InitializeComponent();

            downloadsPath = downloadsFolder + @"\Music";
            logPath = downloadsPath + @"\log.txt";
            //Check if a file was previously used and reload it
            try {
                lbl_filePath.Content = Properties.Settings.Default.lbl_filePath;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
/********************************************************************************************************************************************
*                                                      FILE OR FOLDER EXISTS FUNCTION                                                       *
* Returns true or false depening on if the given path Exists. If it's a Folder or File.                                                     *                                                                              *
********************************************************************************************************************************************/
        public bool FileOrDirectoryExists(string name) {
            return (Directory.Exists(name) || File.Exists(name));
        }
/********************************************************************************************************************************************
*                                                      CLEAN URL FUNCTION                                                                   *
* Function that takes in an array of strings containing URL's and proceeds to making sure to keep only the valids one that match a valid    *
* Youtube video link, and returns a string[] of only the valid/cleaned strings                                                              *
********************************************************************************************************************************************/
        private string[] cleanURL(string[] url) {
            Regex rgx = new Regex("^(http(s)??://)?(www.)?((youtube.com/watch?v=)|(youtu.be/))([a-zA-Z0-9-_])+");
            List<string> linkList = new List<string>();

            foreach (string link in url) {
                if (rgx.Match(link).Success) {
                    linkList.Add(link);         
                } else {
                    File.AppendAllText(logPath, DateTime.Now + ": Error: URL doesn't match Expression: " + link + Environment.NewLine);
                }
            }
            return linkList.ToArray();
        }
/********************************************************************************************************************************************
*                                                       DOWNLOAD URL FUNCTION                                                               *
* Function that downloads every url from passed string array and places them in the users download library under Music Folder. Also creates *
* a Music Folder if it doesn't already exist.                                                                                               *
********************************************************************************************************************************************/
        private void downloadURL(string[] url) {
            if (!FileOrDirectoryExists(downloadsPath)) {
                Directory.CreateDirectory(downloadsPath);
                File.AppendAllText(logPath, DateTime.Now + ": Downloads path created: " + downloadsPath + Environment.NewLine);
            }
            int counter = 0;
            var youtube = YouTube.Default;
            lbl_status.Foreground = Brushes.Black;
            File.AppendAllText(logPath, DateTime.Now + ": Downloading: "+ url.Length + " Files: Path: " + downloadsPath + Environment.NewLine);
            do {
                File.AppendAllText(logPath, DateTime.Now + ": " + (counter + 1) + " - " + url[counter] + Environment.NewLine);
                downloadURLAsync(url[counter], youtube, counter);
            } while (counter++ < url.Length - 1);
        }

        private async Task downloadURLAsync(string url, YouTube youtube, int counter) {
            File.AppendAllText(logPath, DateTime.Now + ": Downloading: " + (counter + 1) + " - " + url + Environment.NewLine);
            var video = await youtube.GetVideoAsync(url);
            var bytes = await video.GetBytesAsync();
            File.WriteAllBytes(downloadsPath + @"\" + video.FullName, bytes);
            lbl_status.Content = "Downloaded: " + video.FullName + "\nLocation: " + downloadsPath;
            File.AppendAllText(logPath, DateTime.Now + ": Downloaded: " + (counter + 1) + " - " + video.FullName + Environment.NewLine);
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
            File.AppendAllText(logPath, DateTime.Now + ": File Selected: " + filePath + Environment.NewLine);
        }

/********************************************************************************************************************************************
*                                                       GET TEXT FILE FUNCTION                                                              *
* Opens a File Dialog in order to select a text file. Once selected the File Name is placed in the filePath label and saved for the users   *
* next use.                                                                                                                                 *
********************************************************************************************************************************************/
        private void lbl_filePath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = downloadsFolder;
            ofd.Filter = "All Text Files (*.txt) | *.txt";
            ofd.Title = "Please Select a Text File";

            ofd.ShowDialog();
            if (Path.GetExtension(ofd.FileName) == ".txt") {
                changeFilePath(ofd.FileName);
            }else {
                File.AppendAllText(logPath, DateTime.Now + ": Error: File chosen not a text file (*.txt) - " + ofd.FileName + Environment.NewLine);
                lbl_status.Foreground = Brushes.Red;
                lbl_status.Content = "Error: Please select a Text File (*.txt)";
            }
            
        }
/********************************************************************************************************************************************
*                                                       GO FUNCTION - DOWNLOAD                                                              *
* On button click disable button until the function is finished. Then validate if file exist and build a string array for each line in file *
* while adding & at the end of each line and then triming the string to the first & as to keep only www.youtube.com/watch?asdasdc           *
* So the URL can look like: www.youtube.com/watch?asdasdc&lsit=sadsa&a=asd                                                                  *                                                                 *
********************************************************************************************************************************************/
        private void btn_Go_Click(object sender, RoutedEventArgs e) {
            string filePath = lbl_filePath.Content.ToString();
            if (FileOrDirectoryExists(filePath)) {
                List<string> urlList = new List<string>();
                StreamReader txtFile = new StreamReader(filePath);
                string line;

                while ((line = txtFile.ReadLine()) != null) {
                    line = line + "&";
                    line = line.Trim('&');
                    urlList.Add(line);
                }
                txtFile.Close();
                string[] url = urlList.ToArray();
                //ADD FUNCTION CLEAN URL AS TO HAVE AN ARRAY OF ONLY VALID YOUTUBE LINKS LIKE https://www.youtube.com/watch?v=IG8NfUMlt-k
                url = cleanURL(url);
                //ADD FUNCTION THAT TAKES URL ARRAY AND DOWNLOADS ALL THE FILES
                try {
                    downloadURL(url);
                    lbl_status.Foreground = Brushes.Green;
                    lbl_status.Content = "Sucess! Files downloading...";
                }
                catch (Exception ex) {
                    lbl_status.Foreground = Brushes.Red;
                    lbl_status.Content = "Error: An error occurred during the file download";
                    File.AppendAllText(logPath, DateTime.Now + ": Error: Problem with downloading url" + Environment.NewLine);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                }
            } else {
                lbl_status.Foreground = Brushes.Red;
                lbl_status.Content = "Error: Given File Path Invalid";
                File.AppendAllText(logPath, DateTime.Now + ": Error: Given text file invalid" + Environment.NewLine);
                changeFilePath("");
            }
        }
    }
}
