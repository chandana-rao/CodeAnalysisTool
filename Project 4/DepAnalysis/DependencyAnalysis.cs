/////////////////////////////////////////////////////////////////////////////
// Executive.cs - Performs Dependency analysis between input files        //
// Language:    C#, 2017, .Net Framework 4.6.1                           //
// Application: Project #3, CSE681 Fall 2018                            //
// Author:      Chandana Rao                                           //
//  Source:     Jim Fawcett                                           //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Takes in the type table of all the input files
 * Performs the dependency analysis on these files
 * Checks for the name of namespace, "using", and aliases and compares the files
 * Passes the dependences from the analysis to acceptGraph() in StrongConnectedComponent.cs to generate the graph
 * Returns the graph with the nodes to Executive
 * 
 */
/* Required Files:
 * Executive.cs
 *   Element.cs
 *   StrongConnectedComponent.cs
 *   Semi.cs
 *   TypeAnalysis.cs
 *   ITokenCollection.cs
 */
/*
*  PublicInterfaceDocumentation:
*   public class CompareFiles
*	public class DependencyAnalysis
*	public static List<CsNode<string, string>> getTables(List<List<Elem>> allTables, List<string> files)	//Gets the type table from the Executive.cs for the dependency analysis
*	public static List<CompareFiles> getNamespace(List<List<Elem>> allTables)	//creates a list of namespaces in all the files
*	public static List<CompareFiles> getUsing(List<List<Elem>> allTables)		//creates a list of type "using" in all the files
*	static public class listOfTypes
*	public static List<string> listClasses(string filename, List<List<Elem>> allTables)	//generates a list of classes of the file whose namespace name matches with other file with the same namespace or "using"
*	public static List<string> listInterfaces(string filename, List<List<Elem>> allTables)	//generates a list of interfaces of the file whose namespace name matches with other file with the same namespace or "using"
*	public static List<string> listStructs(string filename, List<List<Elem>> allTables)	//generates a list of structs of the file whose namespace name matches with other file with the same namespace or "using"
*	public static List<string> listEnums(string filename, List<List<Elem>> allTables)	//generates a list of enums of the file whose namespace name matches with other file with the same namespace or "using"
*	static public class Passes
*	public static void pass1(List<CompareFiles> ns, List<CompareFiles> us, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)	//checks if any file has "using" the namespace of other file
*	public static void pass2(List<CompareFiles> ns, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)				//checks if two files have the same namespace
*	public static void pass3(List<CompareFiles> ns, List<CompareFiles> us, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)	//checks if any file has alias name as the namespace of other file
*	static public class SecondParsing
*	public static void secondParse(string file1, string file2, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)			//Parses the file for the second time for further analysis
*	public static bool secondParseClass(ITokenCollection semi, string file1, string file2, List<string> classList, List<string> files, List<Tuple<string, string>> graph)	//checks if the class of one file is present on the other file with same namespace or using or alias name
*	public static bool secondParseInterface(ITokenCollection semi, string file1, string file2, List<string> interfaceList, List<string> files, List<Tuple<string, string>> graph)	//checks if the interface of one file is present on the other file with same namespace or using or alias name
*	public static bool secondParseStruct(ITokenCollection semi, string file1, string file2, List<string> structList, List<string> files, List<Tuple<string, string>> graph)		//checks if the struct of one file is present on the other file with same namespace or using or alias name
*	public static bool secondParseEnum(ITokenCollection semi, string file1, string file2, List<string> enumList, List<string> files, List<Tuple<string, string>> graph)		//checks if the enum of one file is present on the other file with same namespace or using or alias name
* 
*/
/*
* Maintenance History:
* --------------------
* ver 1.1 : 03 Nov 2018
* -Updated the dependency between the files for delegates
* 
* ver 1.0 : 02 Nov 2018
* - first release
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalysis;
using Lexer;
using StrongComponent;
using System.IO;

namespace DepAnalysis
{
    public class CompareFiles
    {
        public string Filename { get; set; }
        public string Typename { get; set; }
        public string Aliasname { get; set; }
    }
    public class DependencyAnalysis
    {
        //----< Gets the type table from the Executive.cs for the dependency analysis >-----------------
        public static List<CsNode<string, string>> getTables(List<List<Elem>> allTables, List<string> files)
        {
            List<CsNode<string, string>> nodes;
            var graph = new List<Tuple<string, string>>();
            List<CompareFiles> ns = new List<CompareFiles>();
            List<CompareFiles> us = new List<CompareFiles>();
            ns = getNamespace(allTables);
            us = getUsing(allTables);
            
            Passes.pass1(ns, us, allTables, files, graph);
            Passes.pass2(ns, allTables, files, graph);
            Passes.pass3(ns, us, allTables, files, graph);
            TestGraph t = new TestGraph();
            nodes =t.acceptGraph(graph, files);
            return nodes;
        }

        //----< creates a list of namespaces in all the files>-----------------
        public static List<CompareFiles> getNamespace(List<List<Elem>> allTables)
        {
            List<CompareFiles> ns = new List<CompareFiles>();
            CompareFiles store;
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.type == "namespace")
                    {
                        store = new CompareFiles();
                        store.Filename = e.filename;
                        store.Typename = e.name;
                        ns.Add(store);
                    }

                }
            return ns;
        }

        //----< creates a list of type "using" type in all the files>-----------------
        public static List<CompareFiles> getUsing(List<List<Elem>> allTables)
        {
            List<CompareFiles> us = new List<CompareFiles>();
            CompareFiles store;
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.type == "using" || e.type == "alias")
                    {
                        store = new CompareFiles();
                        store.Filename = e.filename;
                        store.Typename = e.name;
                        if(e.type == "alias" && !e.aliasname.StartsWith("System"))
                            store.Aliasname = e.aliasname;
                        us.Add(store);
                    }
                }
            return us;
        }
    }

    static public class listOfTypes
    {
        //----< generates a list of classes of the file whose namespace name matches with other file with the same namespace or "using">-----------------
        public static List<string> listClasses(string filename, List<List<Elem>> allTables)
        {
            List<string> classList = new List<string>();
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.filename == filename && e.type == "class")
                        classList.Add(e.name);
                }
            return classList;
        }

        //----< generates a list of interfaces of the file whose namespace name matches with other file with the same namespace or "using">-----------------
        public static List<string> listInterfaces(string filename, List<List<Elem>> allTables)
        {
            List<string> interfaceList = new List<string>();
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.filename == filename && e.type == "interface")
                        interfaceList.Add(e.name);
                }
            return interfaceList;
        }

        //----< generates a list of structs of the file whose namespace name matches with other file with the same namespace or "using">-----------------
        public static List<string> listStructs(string filename, List<List<Elem>> allTables)
        {
            List<string> structList = new List<string>();
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.filename == filename && e.type == "struct")
                        structList.Add(e.name);
                }
            return structList;
        }

        //----< generates a list of enums of the file whose namespace name matches with other file with the same namespace or "using">-----------------
        public static List<string> listEnums(string filename, List<List<Elem>> allTables)
        {
            List<string> enumList = new List<string>();
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.filename == filename && e.type == "enum")
                        enumList.Add(e.name);
                }
            return enumList;
        }

        //----< generates a list of delegates of the file whose namespace name matches with other file with the same namespace or "using">-----------------
        public static List<string> listDelegates(string filename, List<List<Elem>> allTables)
        {
            List<string> delegateList = new List<string>();
            foreach (List<Elem> table in allTables)
                foreach (Elem e in table)
                {
                    if (e.filename == filename && e.type == "delegate")
                        delegateList.Add(e.name);
                }
            return delegateList;
        }
    }

    static public class Passes
    {
        //-----<checks if any file has "using" the namespace of other file>--------//
        public static void pass1(List<CompareFiles> ns, List<CompareFiles> us, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)
        {
            foreach (var name in ns)
            {
                foreach (var use in us)
                {
                    if (name.Typename == use.Typename)
                    {
                        SecondParsing.secondParse(name.Filename, use.Filename, allTables, files, graph);
                    }
                }
            }
        }

        //-----<checks if two files have the same namespace>--------//
        public static void pass2(List<CompareFiles> ns, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)
        {
            foreach (var name in ns)
            {
                foreach (var name1 in ns)
                {
                    if (name.Filename != name1.Filename && name.Typename == name1.Typename)
                    {
                        
                        SecondParsing.secondParse(name.Filename, name1.Filename, allTables, files, graph);
                    }
                }
            }
        }

        //-----<checks if any file has alias name as the namespace of other file>--------//
        public static void pass3(List<CompareFiles> ns, List<CompareFiles> us, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)
        {
            foreach (var name in ns)
            {
                foreach (var use in us)
                {
                    if (name.Typename == use.Aliasname)
                    {
                        SecondParsing.secondParse(name.Filename, use.Filename, allTables, files, graph);
                    }
                }
            }
        }
    }
       
    static public class SecondParsing
    {
        //----------<Parses the file for the second time for further analysis>---------//
        public static void secondParse(string file1, string file2, List<List<Elem>> allTables, List<string> files, List<Tuple<string, string>> graph)
        {
            List<string> classList = new List<string>();
            List<string> interfaceList = new List<string>();
            List<string> structList = new List<string>();
            List<string> enumList = new List<string>();
            List<string> delegateList = new List<string>();
            string filecheck = null;
            foreach (var file in files)
            {
                if (file.Contains(file2))
                {
                    filecheck = file;
                    break;
                }
            }
            ITokenCollection semi = Factory.create();
            if (!semi.open(filecheck as string))
            {
                Console.Write("\n  Can't open {0}\n\n", file2);
            }
            classList = listOfTypes.listClasses(file1, allTables);
            interfaceList = listOfTypes.listInterfaces(file1, allTables);
            structList = listOfTypes.listStructs(file1, allTables);
            enumList = listOfTypes.listEnums(file1, allTables);
            delegateList = listOfTypes.listDelegates(file1, allTables);
            bool check = false;
            check = secondParseClass(semi, file1, file2, classList, files, graph);
            if (!check)
            {
                semi.open(filecheck as string);
                check = secondParseInterface(semi, file1, file2, interfaceList, files, graph);
            }
            if (!check)
            {
                semi.open(filecheck as string);
                check = secondParseStruct(semi, file1, file2, structList, files, graph);
            }
            if (!check)
            {
                semi.open(filecheck as string);
                check = secondParseEnum(semi, file1, file2, enumList, files, graph);
            }
            if (!check)
            {
                semi.open(filecheck as string);
                check = secondParseDelegate(semi, file1, file2, delegateList, files, graph);
            }
        }

        //---------<checks if the class of one file is present on the other file with same namespace or using or alias name>---------//
        public static bool secondParseClass(ITokenCollection semi, string file1, string file2, List<string> classList, List<string> files, List<Tuple<string, string>> graph)
        {
            int n = classList.Count;
            while (semi.get().Count > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    if (semi.Contains(classList[i]))
                    {
                        graph.Add(new Tuple<string, string>(file2, file1));
                        return true;
                    }
                }
            }
            return false;
        }

        //---------<checks if the interface of one file is present on the other file with same namespace or using or alias name>---------//
        public static bool secondParseInterface(ITokenCollection semi, string file1, string file2, List<string> interfaceList, List<string> files, List<Tuple<string, string>> graph)
        {
            int il = interfaceList.Count;
            while (semi.get().Count > 0)
            {
                for (int i = 0; i < il; i++)
                {
                    if (semi.Contains(interfaceList[i]))
                    {
                        graph.Add(new Tuple<string, string>(file2, file1));
                        return true;
                    }
                }
            }
            return false;
        }

        //---------<checks if the struct of one file is present on the other file with same namespace or using or alias name>---------//
        public static bool secondParseStruct(ITokenCollection semi, string file1, string file2, List<string> structList, List<string> files, List<Tuple<string, string>> graph)
        {
            int il = structList.Count;
            while (semi.get().Count > 0)
            {
                for (int i = 0; i < il; i++)
                {
                    if (semi.Contains(structList[i]))
                    {
                        graph.Add(new Tuple<string, string>(file2, file1));
                        return true;
                    }
                }
            }
            return false;
        }

        //---------<checks if the enum of one file is present on the other file with same namespace or using or alias name>---------//
        public static bool secondParseEnum(ITokenCollection semi, string file1, string file2, List<string> enumList, List<string> files, List<Tuple<string, string>> graph)
        {
            int il = enumList.Count;
            while (semi.get().Count > 0)
            {
                for (int i = 0; i < il; i++)
                {
                    if (semi.Contains(enumList[i]))
                    {
                        graph.Add(new Tuple<string, string>(file2, file1));
                        return true;
                    }
                }
            }
            return false;
        }

        //---------<checks if the enum of one file is present on the other file with same namespace or using or alias name>---------//
        public static bool secondParseDelegate(ITokenCollection semi, string file1, string file2, List<string> delegateList, List<string> files, List<Tuple<string, string>> graph)
        {
            int il = delegateList.Count;
            while (semi.get().Count > 0)
            {
                for (int i = 0; i < il; i++)
                {
                    if (semi.Contains(delegateList[i]))
                    {
                        graph.Add(new Tuple<string, string>(file2, file1));
                        return true;
                    }
                }
            }
            return false;
        }
    }

#if (Test_Stub)
    class DepAn
    { 
        static void Main(string[] args)
        {
            List<CsNode<string, string>> nodes;
            List<List<Elem>> allTables = new List<List<Elem>>();
            List<string> files = ProcessCommandline(args);
            allTables = TypeAnalysis.analyse(files);
            nodes = DependencyAnalysis.getTables(allTables, files);
            display(nodes);
            Console.Read();
        }

        static void display(List<CsNode<string, string>> nodes)
        {
            Console.WriteLine("Demonstrating Dependency analysis\n");
                foreach (var node in nodes)
                {
                    Console.WriteLine("File " + node.name + " depends on: ");
                    for (int i = 0; i < node.children.Count; ++i)
                    {
                        Console.Write("    " + node.children[i].targetNode.name + "\n");
                    }
                    Console.WriteLine();
                }
        }

            static List<string> ProcessCommandline(string[] args)
            {
                List<string> files = new List<string>();
                string path = Path.GetFullPath(args[0]);
                if (Directory.Exists(path))
                {
                    string format = "cs";
                    DirectoryInfo dir = new DirectoryInfo(path);
                    foreach (FileInfo file in dir.GetFiles("*." + format + "*", SearchOption.AllDirectories))
                    {
                        files.Add(file.FullName);
                    }
                }

                return files;
            }
    }
#endif
}
