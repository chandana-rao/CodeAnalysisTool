////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //  
// Language:    C#, 2017, .Net Framework 4.6.1                           //
// Application: Project #4, CSE681 Fall 2018                            //
// Author:      Chandana Rao                                           //
//  Source:     Jim Fawcett                                           //        
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * RequiredFiles:
 *   Element.cs
 *   Display.cs
 *   DependencyAnalysis.cs
 *   TypeAnalysis.cs
 *   StrongConnectedComponent.cs
 * 
 * Public Interface Documentation:
 * public class NavigatorServer
 *  
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 - 05 Dec 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using CodeAnalysis;
using DepAnalysis;
using StrongComponent;
using System.IO;


namespace Navigator
{
    public class NavigatorServer
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;

        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        public NavigatorServer()
        {
            initializeEnvironment();
            Console.Title = "Server";
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            Environment.root = ServerEnvironment.root;
            Environment.address = ServerEnvironment.address;
            Environment.port = ServerEnvironment.port;
            Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher1()
        {
            Func<CommMessage, CommMessage> connect = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "connect";
                return reply;
            };
            messageDispatcher["connect"] = connect;

            Func<CommMessage, CommMessage> typeTable = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "typeTable";
                reply.arguments = typeAnalysis(msg.arguments);
                return reply;
            };
            messageDispatcher["getTypeTable"] = typeTable;

            Func<CommMessage, CommMessage> depTable = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "depTable";
                reply.arguments = depAnalysis(msg.arguments);
                return reply;
            };
            messageDispatcher["depTable"] = depTable;
        }
        void initializeDispatcher2()
        {
            Func<CommMessage, CommMessage> strongComponent = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "strongComponent";
                reply.arguments = strongComponentList(msg.arguments);
                return reply;
            };
            messageDispatcher["strongComponent"] = strongComponent;

            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopFiles"] = getTopFiles;

            Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopDirs"] = getTopDirs;
        }
        void initializeDispatcher3()
        {
            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;
            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                localFileMgr.pathStack.Push(localFileMgr.currentPath);
                localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                reply.back = msg.arguments[0];
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;
            Func<CommMessage, CommMessage> moveUpFolderDirs = (CommMessage msg) =>
            {
                if (msg.back == "")
                    return null;
                localFileMgr.pathStack.Pop();
                localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveUpFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                reply.back = localFileMgr.currentPath;
                return reply;
            };
            messageDispatcher["moveUpFolderDirs"] = moveUpFolderDirs;
        }

        static void Main(string[] args)
        {
            TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                NavigatorServer server = new NavigatorServer();
                server.initializeDispatcher1();
                server.initializeDispatcher2();
                server.initializeDispatcher3();
                server.comm = new Comm(ServerEnvironment.address, ServerEnvironment.port);
                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    if (reply == null)
                        continue;
                    reply.show();
                    server.comm.postMessage(reply);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }
        }

        List<string> filePath(List<string> files)
        {
            List<string> allFiles = new List<string>();
            List<string> checkFiles = new List<string>();
            string path = Path.GetFullPath("../../../ServerFiles/");
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo fileAll in dir.GetFiles("*." + "cs", SearchOption.AllDirectories))
            {
                allFiles.Add(fileAll.FullName);
                foreach (string file in files)
                {
                    if (fileAll.FullName.Contains(file))
                        checkFiles.Add(fileAll.FullName);
                }
            }
            return checkFiles;
        }

        List<string> typeAnalysis(List<string> files)
        {
            List<List<Elem>> allTables = new List<List<Elem>>();
            List<string> checkFiles = new List<string>();
            List<string> tableType = new List<string>();
            checkFiles = filePath(files);
            allTables = TypeAnalysis.analyse(checkFiles);
            tableType = displayRequirement(allTables);
            return tableType;
        }

        List<string> displayRequirement(List<List<Elem>> allTables)
        {
            List<string> tableType = new List<string>();
            Display.showMetricsNamespace(allTables);
            Display.showMetricsInterface(allTables);
            Display.showMetricsAlias(allTables);
            Display.showMetricsClass(allTables);
            Display.showMetricsFunction(allTables);
            Display.showMetricsStruct(allTables);
            Display.showMetricsEnum(allTables);
            Display.showMetricsUsing(allTables);
            tableType = Display.showMetricsDelegate(allTables);
            return tableType;
        }

        List<string> depAnalysis(List<string> files)
        {
            List<string> depType = new List<string>();
            List<List<Elem>> allTables = new List<List<Elem>>();
            List<string> checkFiles = new List<string>();
            List<string> tableType = new List<string>();
            checkFiles = filePath(files);
            allTables = TypeAnalysis.analyse(checkFiles);
            List<CsNode<string, string>> nodes = DependencyAnalysis.getTables(allTables, checkFiles);
            depType = Display.showDependences(nodes);
            return depType;
        }

        List<string> strongComponentList(List<string> files)
        {
            List<string> depType = new List<string>();
            List<List<Elem>> allTables = new List<List<Elem>>();
            List<string> checkFiles = new List<string>();
            List<string> tableType = new List<string>();
            List<string> scc = new List<string>();
            List<string> sc = new List<string>();
            List<string> SCC = new List<string>();
            List<CsNode<string, string>> nodes = new List<CsNode<string, string>>();
            checkFiles = filePath(files);
            allTables = TypeAnalysis.analyse(checkFiles);
            nodes = DependencyAnalysis.getTables(allTables, checkFiles);
            TestGraph t = new TestGraph();
            scc = t.tarjan(nodes);
            SCC = Display.showSCC(scc);
            return SCC;
        }
    }
}
