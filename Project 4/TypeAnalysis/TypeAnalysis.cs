//////////////////////////////////////////////////////////////////////////////////////
// Executive.cs Demonstrate Type analysis between given files                      //
// Language:    C#, 2017, .Net Framework 4.6.1                                    //
// Application: Project #3, CSE681 Fall 2018                                     //
// Author:      Chandana Rao                                                    //
//  Source:     Jim Fawcett                                                    //
////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Creates type table for each input file
 * Types are detected using the rules and actions defined in RulesandActions.cs
 * Makes use of parser to parse through each input file and detect the type
 * 
 */
/* Required Files:
 *   Executive.cs
 *   Element.cs
 *   Display.cs
 *   Parser.cs
 *   ScopeStack.cs
 *   RulesandActions.cs
 *   IRuleandAction.cs
 *   Semi.cs
 *   Toker.cs
 *   ITokenCollection.cs
 */
/*Public InterfaceDocumentation:
*   public class TypeAnalysis                                           -class that performs type analysis for the input files
*   public static List<List<Elem>> analyse(List<string> files)          - generates type table for the input files
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
using Lexer;
using System.IO;

namespace CodeAnalysis
{
    public class TypeAnalysis
    {
        //------------Performs type analysis for the input files-----------------//
        public static List<List<Elem>> analyse(List<string> files)
        {
            List<List<Elem>> allTables = new List<List<Elem>>();
            foreach (string file in files)
            {
                ITokenCollection semi = Factory.create();
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi, System.IO.Path.GetFileName(file));
                Parser parser = builder.build();
                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                allTables.Add(table);
                semi.close();
            }
            return allTables;
        }

    }

#if (Test_Stub)

    public class Test
    {
        static void Main(string[] args)
        {
            List<List<Elem>> allTables = new List<List<Elem>>();
            List<string> files = ProcessCommandline(args);
            allTables = TypeAnalysis.analyse(files);
            display(allTables);
            Console.Read();
        }

            static void display(List<List<Elem>> allTables)
            {
                Console.WriteLine("Displaying the type table for all the files");
                Console.WriteLine("--------------------------------------------------------------------");
                Display.showMetricsNamespace(allTables);
                Display.showMetricsClass(allTables);
                Display.showMetricsFunction(allTables);
                Display.showMetricsAlias(allTables);
                Display.showMetricsEnum(allTables);
                Display.showMetricsStruct(allTables);
                Display.showMetricsDelegate(allTables);
                Display.showMetricsUsing(allTables);
                Display.showMetricsInterface(allTables);
                Console.Write("\n\n");
                return;
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
