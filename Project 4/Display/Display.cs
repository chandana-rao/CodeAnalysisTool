///////////////////////////////////////////////////////////////////////////////////////////
// Display.cs - Manage Display properties                                               //
//              - Demostrate the strongly connected components in the dependency graph //
// Language:    C#, 2017, .Net Framework 4.6.1                                        //
// Application: Project #3, CSE681 Fall 2018                                         //
// Author:      Chandana Rao                                                        //
//  Source:     Jim Fawcett                                                        //
////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * Display manages static public properties used to control what is displayed and
 * provides static helper functions to send information to MainWindow and Console.
 * 
 * Public Interface
 * ================
 *  public static class StringExt
*	public static string Truncate(this string value, int maxLength)
*	static public class Display
*	static public void showMetricsNamespace(List<List<Elem>> allTable)
*	static public void showMetricsClass(List<List<Elem>> allTable)
*	static public void showMetricsInterface(List<List<Elem>> allTable)
*	static public void showMetricsFunction(List<List<Elem>> allTable)
*	static public void showMetricsAlias(List<List<Elem>> allTable)
*	static public void showMetricsDelegate(List<List<Elem>> allTable)
*	static public void showMetricsStruct(List<List<Elem>> allTable)
*	static public void showMetricsEnum(List<List<Elem>> allTable)
*	static public void showMetricsUsing(List<List<Elem>> allTable)
*	static public void showDependences(List<CsNode<string, string>> nodes)
*	static public void showSCC(List<string> scc)
*	static public void displaySemiString(string semi)
*	static public void displayString(Action<string> act, string str)
*	static public void displayString(string str, bool force=false)
*	static public void displayRules(Action<string> act, string msg)
*	static public void displayActions(Action<string> act, string msg)
*	static public void displayFiles(Action<string> act, string file)
*	static public void displayDirectory(Action<string> act, string file)
 * 
 * 
 */
/*
 * Required Files:
 *   Executive.cs
 *   Element.cs
 *   StrongConnectedComponent.cs

 * Maintenance History
 * ===================
 * ver 1 : 02 Nov 2018
 *   - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using StrongComponent;

namespace CodeAnalysis
{
  public static class StringExt
  {
    public static string Truncate(this string value, int maxLength)
    {
      if (string.IsNullOrEmpty(value)) return value;
      return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
  }

  static public class Display
  {
    static Display()
    {
      showFiles = true;
      showDirectories = true;
      showActions = false;
      showRules = false;
      useFooter = false;
      useConsole = false;
      goSlow = false;
      width = 33;
    }
    static public bool showFiles { get; set; }
    static public bool showDirectories { get; set; }
    static public bool showActions { get; set; }
    static public bool showRules { get; set; }
    static public bool showSemi { get; set; }
    static public bool useFooter { get; set; }
    static public bool useConsole { get; set; }
    static public bool goSlow { get; set; }
    static public int width { get; set; }
    static public List<string> tableType = new List<string>();
        
        //----< displays the type "namespace" in the given files >--------------------
    static public void showMetricsNamespace(List<List<Elem>> allTable)
    {
        tableType = new List<string>();
        tableType.Add("Namespace:\n");
        foreach (List <Elem> table in allTable)
        foreach (Elem e in table)
        {
          if (e.type == "namespace")
            tableType.Add("[" + e.filename + ", " + e.name + "]" +"\n");
        }
    }

        //----< displays the type "class" in the given files >--------------------
    static public void showMetricsClass(List<List<Elem>> allTable)
    {
        tableType.Add("Class:\n");
        foreach (List<Elem> table in allTable)
            foreach (Elem e in table)
            {
                if (e.type == "class")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" +"\n");
            }
    }

        //----< displays the type "interface" in the given files >--------------------
    static public void showMetricsInterface(List<List<Elem>> allTable)
    {
        tableType.Add("Interface:\n");
        foreach (List<Elem> table in allTable)
            foreach (Elem e in table)
            {
                if (e.type == "interface")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" +"\n");
            }
    }

        //----< displays the type "function" in the given files >--------------------
    static public void showMetricsFunction(List<List<Elem>> allTable)
    {
        tableType.Add("Function:\n");
        foreach (List<Elem> table in allTable)

            foreach (Elem e in table)
            {
                if (e.type == "function")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" + "\n");
            }
    }

        //----< displays the type "aliases" in the given files >--------------------
    static public void showMetricsAlias(List<List<Elem>> allTable)
    {
        tableType.Add("Aliases:\n");
        foreach (List<Elem> table in allTable)

            foreach (Elem e in table)
            {
                if (e.type == "alias")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" +"\n");
            }
    }

        //----< displays the type "delegate" in the given files >--------------------
    static public List<string> showMetricsDelegate(List<List<Elem>> allTable)
    {
        tableType.Add("Delegates:\n");
        foreach (List<Elem> table in allTable)
        {
            foreach (Elem e in table)
            {
                if (e.type == "delegate")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" +"\n");
            }
        }
        return tableType;
    }

        //----< displays the type "structs" in the given files >--------------------
    static public void showMetricsStruct(List<List<Elem>> allTable)
    {
        tableType.Add("Structs:\n");
        foreach (List<Elem> table in allTable)

            foreach (Elem e in table)
            {
                if (e.type == "struct")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" + "\n");
            }
    }

        //----< displays the type "enums" in the given files >--------------------
    static public void showMetricsEnum(List<List<Elem>> allTable)
    {
        tableType.Add("Enum:\n");
        foreach (List<Elem> table in allTable)

            foreach (Elem e in table)
            {
                if (e.type == "enum")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" + "\n");
            }
    }

        //----< displays the type "using" in the given files >--------------------
    static public void showMetricsUsing(List<List<Elem>> allTable)
    {
        tableType.Add("Using: \n"); 
        foreach (List<Elem> table in allTable)

            foreach (Elem e in table)
            {
                if (e.type == "using")
                    tableType.Add("[" + e.filename + ", " + e.name + "]" + "\n");
            }
    }

        //----< displays the dependences between the files >--------------------
    static public List<string> showDependences(List<CsNode<string, string>> nodes)
    {
        List<string> depTable = new List<string>();
        foreach (var node in nodes)
        {
            depTable.Add("File " + node.name + " depends on: ");
            for (int i = 0; i < node.children.Count; ++i)
            {
                depTable.Add("    " + node.children[i].targetNode.name + "\n");
            }
            depTable.Add("\n");
        }
        return depTable;
    }

        //----< displays the strongly connected component >--------------------
    static public List<string> showSCC(List<string> scc)
    {
        List<string> sc = new List<string>();
        for (int i=0; i<scc.Count; i++)
        {
            sc.Add(i + 1 + ". " + scc[i] +"\n");
        }
        return sc;
    }
        //----< display a semiexpression on Console >--------------------

    static public void displaySemiString(string semi)
    {
      if (showSemi && useConsole)
      {
        Console.Write("\n");
        System.Text.StringBuilder sb = new StringBuilder();
        for (int i = 0; i < semi.Length; ++i)
          if (!semi[i].Equals('\n'))
            sb.Append(semi[i]);
        Console.Write("\n  {0}", sb.ToString());
      }
    }

    //----< display, possibly truncated, string >--------------------

    static public void displayString(Action<string> act, string str)
    {
      if (goSlow) Thread.Sleep(200); 
      if (act != null && useFooter)
        act.Invoke(str.Truncate(width));
      if (useConsole)
        Console.Write("\n  {0}", str);
    }
    //----< display string, possibly overriding client pref >--------

    static public void displayString(string str, bool force=false)
    {
      if (useConsole || force)
        Console.Write("\n  {0}", str);
    }
    //----< display rules messages >---------------------------------

    static public void displayRules(Action<string> act, string msg)
    {
      if (showRules)
      {
        displayString(act, msg);
      }
    }
    //----< display actions messages >-------------------------------

    static public void displayActions(Action<string> act, string msg)
    {
      if (showActions)
      {
        displayString(act, msg);
      }
    }
    //----< display filename >---------------------------------------

    static public void displayFiles(Action<string> act, string file)
    {
      if (showFiles)
      {
        displayString(act, file);
      }
    }
    //----< display directory >--------------------------------------

    static public void displayDirectory(Action<string> act, string file)
    {
      if (showDirectories)
      {
        displayString(act, file);
      }
    }

#if(TEST_DISPLAY)
    static void Main(string[] args)
    {
      Console.Write("\n  Tested by Executive \n\n");
    }
#endif
  }
}
