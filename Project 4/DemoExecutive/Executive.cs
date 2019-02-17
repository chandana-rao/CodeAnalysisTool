///////////////////////////////////////////////////////////////////////////////////////////
// Executive.cs - Demonstrate Code and Dependency analysis between given files          //
//              - Demostrate the strongly connected components in the dependency graph //
// Language:    C#, 2017, .Net Framework 4.6.1                                        //
// Application: Project #3, CSE681 Fall 2018                                         //
// Author:      Chandana Rao                                                        //
//  Source:     Jim Fawcett                                                        //
////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Inputs the files from the given directory
 * Demonstrates all the requirements for Project 3 
 * Demonstartes the type table
 * Demonstrates the dependency analysis between the files
 * Demnstrates the strongly connected component in the dependency graph
 * 
 */
/* Required Files:
 *   Element.cs
 *   Display.cs
 *   DependencyAnalysis.cs
 *   TypeAnalysis.cs
 *   StrongConnectedComponent.cs
 */
 /*Public InterfaceDocumentation:
  * 
  */
  /* 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 02 Nov 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DepAnalysis;
using StrongComponent;

namespace CodeAnalysis
{
  public class Executive
  {
        public static List<string> tableType = new List<string>();
        //----< process commandline to get file references >-----------------
        static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
            string path = Path.GetFullPath(args[0]);
            if(Directory.Exists(path))
            {
                Console.WriteLine("\nFiles that are present in the directory are:");
                string format = "cs";
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach(FileInfo file in dir.GetFiles("*." + format + "*", SearchOption.AllDirectories))
                {
                    files.Add(file.FullName);
                    Console.WriteLine(file.Name);
                }
                Console.WriteLine("\n\n");
            }
      return files;
    }
        //----< process commandline arguments and print the test files in the file directory >-----------------
        static void ShowCommandLine(string[] args)
    {
      Console.WriteLine(" Commandline arguments are:  ");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      string fullPath = Path.GetFullPath(args[0]);
      Console.WriteLine("\n\n Directory with the test files:  " + fullPath);
    }

    static void Main(string[] args)
    {
            List<CsNode<string, string>> nodes;
            List<string> scc = new List<string>();
            List<List<Elem>> allTables = new List<List<Elem>>();
            Console.WriteLine("********Type-Based Package Dependency Analysis********\n\n");
            Console.WriteLine("Demonstrating Requirement 4:");
            Console.WriteLine("--------------------------------------------------------------------");
            ShowCommandLine(args);
            List<string> files = ProcessCommandline(args);
            allTables = TypeAnalysis.analyse(files);
            tableType =displayRequirement1(allTables);
            nodes = DependencyAnalysis.getTables(allTables, files);
            displayRequirement2(nodes);
            TestGraph t = new TestGraph();
            scc = t.tarjan(nodes);
            displayRequirement3(scc);
            Console.Read();
    }

        //----------------*Displays Requirement 5*-------------------//
       public static List<string> displayRequirement1(List<List<Elem>> allTables)
        {
            
           // Console.WriteLine("Demonstrating Requirement 5:");
            //Console.WriteLine("Displaying the type table for all the files");
            //Console.WriteLine("--------------------------------------------------------------------");
            Display.showMetricsNamespace(allTables);
            Display.showMetricsInterface(allTables);
            Display.showMetricsAlias(allTables);
            Display.showMetricsClass(allTables);
            Display.showMetricsFunction(allTables);
            Display.showMetricsStruct(allTables);
            Display.showMetricsEnum(allTables);
            Display.showMetricsUsing(allTables);
            tableType = Display.showMetricsDelegate(allTables);
            //Console.Write("\n\n");
            return tableType;
        }

        //----------------*Displays Requirement 5*-------------------//
        static void displayRequirement2(List<CsNode<string, string>> nodes)
        {
            Console.WriteLine("\n**Identifying the file dependences with respect to the namespaces and aliases**");
            Console.WriteLine("**This is done by parsing the file twice**");
            Console.WriteLine("**Each file is checked against the types(classe, enum, delegate, interface, struct) of another file**");
            Console.WriteLine("\nDemonstrating Requirement 5:");
            Console.WriteLine("Displaying the dependences between the given input files");
            Console.WriteLine("--------------------------------------------------------------------");
            Display.showDependences(nodes);
        }

        //----------------*Displays Requirement 6*-------------------//
        static void displayRequirement3(List<string> scc)
        {
            Console.WriteLine("Demonstrating Requirement 6:");
            Console.WriteLine("Displaying the strongly connected components for given input files");
            Console.WriteLine("--------------------------------------------------------------------");
            Display.showSCC(scc);
        }
    }
}
