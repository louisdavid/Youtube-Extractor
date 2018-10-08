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
            lbl_filePath.Content = ofd.FileName;
            try {
                Properties.Settings.Default.lbl_filePath = ofd.FileName;
                Properties.Settings.Default.Save();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
