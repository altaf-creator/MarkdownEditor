using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
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
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using CefSharp;
using Markdig;
using Path = System.IO.Path;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Drawing;

namespace MarkdownEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string path;
        public string rootPath;
        public string tempPath = Path.GetTempPath() + "markdowneditor_temp.html";
        private string prevTxt;
        public bool isOpened = false;
        public bool isModified = true;

        private bool isExplorerOpen = true;
        private bool isSourceOpen = true;
        private bool isPreviewOpen = true;

        public MainWindow()
        {
            InitializeComponent();
            prevTxt = txtRaw.Text;
            rootPath = "";
            path = "";

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(.5f);
            dispatcherTimer.Start();
            File.WriteAllText(tempPath, "");
            browser.Load(tempPath);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (prevTxt != txtRaw.Text)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                string font = "<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">\r\n<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>\r\n<link href=\"https://fonts.googleapis.com/css2?family=Roboto+Slab:wght@400;700&display=swap\" rel=\"stylesheet\">";
                // CSS styling. Please forgive me.
                string style = @"<style>
                                    body {
                                        font-family: 'Roboto Slab', serif; 
                                        background-color: #2a2d31; 
                                        border-radius: 5px; 
                                        color: #FFFFFF;
                                    } 
                                    a {
                                        color: #b4ccfe;
                                    } 
                                    a:hover {
                                        color: #869fd5;
                                    } 
                                    a:focus {
                                        color: #4978df;
                                    } 
                                    img {
                                        max-width: 100%; 
                                        border-radius: 5px;
                                    } 
                                    blockquote {
                                        margin: 20px 0 30px; 
                                        padding-left: 20px;
                                        border-left: 3px solid #FFFFFF
                                    } 
                                    pre {
                                        padding: 5px; 
                                        background-color: #474850; 
                                        border-radius: 5px; 
                                        overflow: scroll; 
                                        overflow-y: auto; 
                                        overflow-x: auto;
                                    } 
                                    p > code {
                                        padding: 5px; 
                                        background-color: #474850; 
                                        border-radius: 5px;
                                    } 
                                    p {
                                        word-break: normal;
                                        white-space: normal;
                                    } 
                                    td, th {
                                        border: 1px solid #474850;  
                                        padding: 8px; 
                                        margin: 0;
                                    } 
                                    th {  
                                        padding-top: 10px;  
                                        padding-bottom: 10px;  
                                        text-align: center;  
                                        background-color: #474850;
                                    } 
                                    table {
                                        border-collapse: collapse;
                                    } 
                                    ::-webkit-scrollbar {
                                        width: 10px;
                                    } 
                                    ::-webkit-scrollbar-track {
                                        background: #00000000; 
                                    } 
                                    ::-webkit-scrollbar-thumb {
                                        border-radius: 5px;
                                        background: #b1b1b18f; 
                                    } 
                                    ::-webkit-scrollbar-thumb:hover {
                                        background: #b1b1b1b6; 
                                    } 
                                    ::-webkit-scrollbar-thumb:active {
                                        background: #b1b1b152; 
                                    } 
                                    h1 {
                                        border-bottom: solid white 1px; 
                                        padding-bottom: 0.8rem;
                                    }
                                </style>";
                var result = Markdown.ToHtml(txtRaw.Text, pipeline);

                File.WriteAllText(tempPath, font + style + result);

                string htmlPath = "file:///" + tempPath;

                if (browser.Address != htmlPath.Replace('\\', '/'))
                {
                    browser.Load(tempPath);
                }
                else
                {
                    browser.Reload();
                }

                prevTxt = txtRaw.Text;
            }

            if (isOpened)
            {
                isModified = txtRaw.Text != File.ReadAllText(path);
                Title = Path.GetFileName(path) + " - MarkdownEditor";

                if (isModified)
                {
                    Title = Path.GetFileName(path) + "*" + " - MarkdownEditor";
                }
                else
                {
                    Title = "MarkdownEditor - a basic markdown viewer and editor";
                }
            }
            else
            {
                isModified = false;
            }

            string formattedPath = path;

            pathTxt.Text = formattedPath.Substring(rootPath.Length);
            browser.LoadError += browser_LoadError;
        }

        // ============================================

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = txtRaw.Text;

            if (!isOpened)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Markdown (*.md)|*.md|Text (*.txt)|*.txt|All (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    path = saveFileDialog.FileName;
                    File.WriteAllText(path, text);
                    isOpened = true;
                    isModified = false;

                    addListButton(path);
                }
            }
            else
            {
                File.WriteAllText(path, text);
            }
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = txtRaw.Text;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Markdown (*.md)|*.md|Text (*.txt)|*.txt|All (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
                File.WriteAllText(path, text);

                addListButton(path);
            }

        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("There are unsaved changes. Are you sure you want to open another file?", "Unsaved Changes", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All (*.*)|*.*";

                    if (openFileDialog.ShowDialog() == true)
                    {
                        path = openFileDialog.FileName;
                        rootPath = "";
                        txtRaw.Text = File.ReadAllText(path);

                        addListButton(path);

                        isOpened = true;
                    }
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    path = openFileDialog.FileName;
                    rootPath = "";
                    txtRaw.Text = File.ReadAllText(path);

                    addListButton(path);

                    isOpened = true;
                }
            }
        }

        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = Markdig.Markdown.ToHtml(txtRaw.Text);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "HTML (*.html)|*.html|Text (*.txt)|*.txt|All (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, text);
            }
        }

        private void fileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("There are unsaved changes. Are you sure you want to open another file?", "Unsaved Changes", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var element = (FrameworkElement)sender;
                    string tooltip = element.ToolTip.ToString();

                    txtRaw.Text = File.ReadAllText(tooltip);
                    path = tooltip;
                    isOpened = true;
                }
            }
            else
            {
                var element = (FrameworkElement)sender;
                string tooltip = element.ToolTip.ToString();

                txtRaw.Text = File.ReadAllText(tooltip);
                path = tooltip;
                isOpened = true;
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            filePanel.Items.Clear();
        }

        private void openFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("There are unsaved changes. Are you sure you want to open another file?", "Unsaved Changes", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var dialog = new VistaFolderBrowserDialog();
                    dialog.Description = "Select Folder";
                    dialog.UseDescriptionForTitle = true;

                    if (dialog.ShowDialog() == true)
                    {
                        path = dialog.SelectedPath;
                        rootPath = dialog.SelectedPath;

                        string[] files = Directory.GetFiles(path, "*.md", SearchOption.AllDirectories);

                        filePanel.Items.Clear();

                        for (int i = files.Length - 1; i >= 0; i--)
                        {
                            string str = files[i];

                            str = str.Substring(path.Length);

                            addListButton(files[i]);
                        }
                    }
                }
            }
            else
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Select Folder";
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == true)
                {
                    path = dialog.SelectedPath;
                    rootPath = dialog.SelectedPath;

                    string[] files = Directory.GetFiles(path, "*.md", SearchOption.AllDirectories);

                    filePanel.Items.Clear();

                    for (int i = files.Length - 1; i >= 0; i--)
                    {
                        string str = files[i];

                        str = str.Substring(path.Length);

                        addListButton(files[i]);
                    }
                }
            }
        }

        private void newBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("There are unsaved changes. Are you sure you want to make a new file?", "Unsaved Changes", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    path = "";
                    rootPath = "";
                    txtRaw.Text = "";
                    isOpened = false;
                }
            }
            else
            {
                path = "";
                rootPath = "";
                txtRaw.Text = "";
                isOpened = false;
            }
        }

        private void aboutBtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = new AboutWindow();
            window.Show();
        }

        // ============================================

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            File.Delete(tempPath);

            if (isModified)
            {
                var result = MessageBox.Show("There are unsaved changes. Are you sure you want to quit?", "Unsaved Changes", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        public void browser_LoadError(object sender, EventArgs e)
        {
            browser.Load(tempPath);
        }

        void addListButton(string filePath)
        {
            var fileButton = new Button();
            fileButton.Content = Path.GetFileName(filePath);
            fileButton.ToolTip = filePath;
            fileButton.Click += fileBtn_Click;
            filePanel.Items.Insert(0, fileButton);
        }

        // ============================================

        private void explorerWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isExplorerOpen)
            {
                fileColumn.Width = new GridLength(0);
                explorerWindowBtn.Foreground = System.Windows.Media.Brushes.Gray;
                isExplorerOpen = false;
            }
            else
            {
                fileColumn.Width = new GridLength(300);
                explorerWindowBtn.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                isExplorerOpen = true;
            }
        }

        private void mainWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isSourceOpen)
            {
                mainColumn.Width = new GridLength(0);
                mainWindowBtn.Foreground = System.Windows.Media.Brushes.Gray;
                isSourceOpen = false;
            }
            else
            {
                mainColumn.Width = new GridLength(1, GridUnitType.Star);
                mainWindowBtn.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                isSourceOpen = true;
            }
        }

        private void previewWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isPreviewOpen)
            {
                previewColumn.Width = new GridLength(0);
                previewWindowBtn.Foreground = System.Windows.Media.Brushes.Gray;
                isPreviewOpen = false;
            }
            else
            {
                previewColumn.Width = new GridLength(1, GridUnitType.Star);
                previewWindowBtn.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                isPreviewOpen = true;
            }
        }
    }
}
