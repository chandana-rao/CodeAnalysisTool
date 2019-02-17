////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App //
// Language:    C#, 2017, .Net Framework 4.6.1                           //
// Application: Project #4, CSE681 Fall 2018                            //
// Author:      Chandana Rao                                           //
//  Source:     Jim Fawcett                                           //        
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a remote FileFolder view.  
 * It supports navigating into subdirectories, both locally and in the remote Server.
 * It also supports viewing local files.
 * 
 * Required Files:
 * MainWindow.xaml
 * 
 * Public Interface Documentation:
 * public partial class MainWindow : Window
 * 
 * 
 * Maintenance History:
 * --------------------
 * - first release 
 * 05 Dec 2018 v1.0
 */
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
using System.Threading;
using MessagePassingComm;

namespace Navigator
{
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null; 
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;
        public List<string> files = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            RemoteUp.IsEnabled = false;
            Add.IsEnabled = false;
            Remove.IsEnabled = false;
            tt.IsEnabled = false;
            dt.IsEnabled = false;
            sc.IsEnabled = false;
            RemoveAll.IsEnabled = false;
            string path = System.IO.Path.GetFullPath("../../../ServerFiles/");
            fullPath.Text = path;
            initializeEnvironment();
            Console.Title = "Client";
            fileMgr = FileMgrFactory.create(FileMgrType.Local);
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            automatedDemo();
        }

        void automatedDemo()
        {
            fullPath.IsEnabled = true;
            guiGetFiles();
            Task.Delay(2000);
            selected.Items.Add("File1.cs");
            selected.Items.Add("File2.cs");
            selected.Items.Add("File3.cs");
            Task.Delay(6000).ContinueWith(_ => { guiTypeTable(); });
            Task.Delay(9000).ContinueWith(_ => { guiDepTable(); });
            Task.Delay(11000).ContinueWith(_ => { guiSCC(); });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
        }

        //----< make Environment equivalent to ClientEnvironment >-------
        void initializeEnvironment()
        {
            Environment.root = ClientEnvironment.root;
            Environment.address = ClientEnvironment.address;
            Environment.port = ClientEnvironment.port;
            Environment.endPoint = ClientEnvironment.endPoint;
        }
        //----< define how to process each message command >-------------

        void dTypeTable(CommMessage msg)
        {
            getTypeTable.Focus();
            string textType = null;
            foreach (string text in msg.arguments)
                textType = textType + text;
            typeAnalysis.Text = textType;
        }

        void dDepTable(CommMessage msg)
        {
            getDepTable.Focus();
            string textType = null;
            foreach (string text in msg.arguments)
                textType = textType + text;
            dependencyAnalysis.Text = textType;
        }

        void dSC(CommMessage msg)
        {
            sComp.Focus();
            string textType = null;
            foreach (string text in msg.arguments)
                textType = textType + text;
            component.Text = textType;
        }

        void dGetFiles(CommMessage msg)
        {
            remoteFiles.Items.Clear();
            foreach (string file in msg.arguments)
            {
                remoteFiles.Items.Add(file);
            }
        }

        void dGetDirs(CommMessage msg)
        {
            remoteDirs.Items.Clear();
            foreach (string dir in msg.arguments)
            {
                remoteDirs.Items.Add(dir);
            }
        }

        void dMoveIntoFolderFiles(CommMessage msg)
        {
            remoteFiles.Items.Clear();
            foreach (string file in msg.arguments)
            {
                remoteFiles.Items.Add(file);
            }
        }

        void dMoveIntoFolderDirs(CommMessage msg)
        {
            remoteDirs.Items.Clear();
            fileMgr.currentPath = msg.back;
            foreach (string dir in msg.arguments)
            {
                remoteDirs.Items.Add(dir);
            }
        }

        void dMoveUpFolderDirs(CommMessage msg)
        {
            remoteDirs.Items.Clear();
            fileMgr.currentPath = msg.back;
            foreach (string dir in msg.arguments)
            {
                remoteDirs.Items.Add(dir);
            }
        }

        void initializeMessageDispatcher()
        {
            messageDispatcher["typeTable"] = (CommMessage msg) =>
            {
                dTypeTable(msg);
            };
            messageDispatcher["depTable"] = (CommMessage msg) =>
            {
                dDepTable(msg);
            };
            messageDispatcher["strongComponent"] = (CommMessage msg) =>
            {
                dSC(msg);
            };
            messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            {
                dGetFiles(msg);
            };
            messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            {
                dGetDirs(msg);
            };
            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                dMoveIntoFolderFiles(msg);
            };
            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                dMoveIntoFolderDirs(msg);
            };
            messageDispatcher["moveUpFolderDirs"] = (CommMessage msg) =>
            {
                dMoveUpFolderDirs(msg);
            };
        }

        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;
                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        //----< shut down comm when the main window closes >-------------
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        //----< move to root of remote directories >---------------------
         
        private List<string> fetchSelectedFiles()
        {
            List<string> mySelectedList = new List<string>();
            foreach (string select in selected.Items)
                mySelectedList.Add(select);
            return mySelectedList;
        }

        void guiTypeTable()
        {
            List<string> selection = fetchSelectedFiles();
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "getTypeTable";
            msg1.arguments = selection;
            comm.postMessage(msg1);
        }

        private void typeTable(object sender, RoutedEventArgs e)
        {
            if (selected.Items.Count >0)
                guiTypeTable();
        }

        void guiDepTable()
        {
            List<string> selection = fetchSelectedFiles();
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "depTable";
            msg1.arguments = selection;
            comm.postMessage(msg1);
        }

        private void depTable(object sender, RoutedEventArgs e)
        {
            if (selected.Items.Count > 0)
                guiDepTable();
        }

        void guiSCC()
        {
            List<string> selection = fetchSelectedFiles();
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "strongComponent";
            msg1.arguments = selection;
            comm.postMessage(msg1);
        }
        private void sC(object sender, RoutedEventArgs e)
        {
            demo.Content = "";
            if (selected.Items.Count > 0)
                guiSCC();
        }
        void guiGetFiles()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "getTopFiles";
            msg1.arguments.Add("");
            comm.postMessage(msg1);
            msg1.show();
            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopDirs";
            comm.postMessage(msg2);
            tt.IsEnabled = true;
            dt.IsEnabled = true;
            sc.IsEnabled = true;
            Add.IsEnabled = true;
            Remove.IsEnabled = true;
            RemoteUp.IsEnabled = true;
            RemoveAll.IsEnabled = true;
        }

        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            guiGetFiles();
        }
        

        //----< move to parent directory of current remote path >--------

        void guiUpClick()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveUpFolderDirs";
            msg1.arguments.Add("");
            msg1.back = fileMgr.currentPath;
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderFiles";
            msg2.arguments.Add(fileMgr.currentPath);
            comm.postMessage(msg2);
        }

        private void RemoteUp_Click(object sender, RoutedEventArgs e)
        {
            guiUpClick();
        }
        //----< move into remote subdir and display files and subdirs >--
       

        void guiDoubleDirClick()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveIntoFolderFiles";
            msg1.arguments.Add(remoteDirs.SelectedValue as string);
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderDirs";
            comm.postMessage(msg2);
        }
        private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            guiDoubleDirClick();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void guiRemove()
        {
            if (selected.SelectedIndex != -1)
                selected.Items.Remove(selected.SelectedItem.ToString());
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            guiRemove();
        }

        void guiAdd()
        {
            if (remoteFiles.SelectedIndex != -1)
            {
                string check = remoteFiles.SelectedItem.ToString();
                if (!selected.Items.Contains(check))
                    selected.Items.Add(remoteFiles.SelectedItem.ToString());
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            guiAdd();
        }

        void guiBack()
        {
            Local.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            guiBack();
        }

        private void RemoveAll_Click(object sender, RoutedEventArgs e)
        {
                selected.Items.Clear();
        }
    }
}
