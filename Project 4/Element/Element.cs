//////////////////////////////////////////////////////////////////////////////////////////
// Executive.cs - Data Structure for Type and dependency analysis                      //
// Language:    C#, 2017, .Net Framework 4.6.1                                        //
// Application: Project #3, CSE681 Fall 2018                                         //
// Author:      Chandana Rao                                                        //
//  Source:     Jim Fawcett                                                        //
////////////////////////////////////////////////////////////////////////////////////
///
/* Module Operations:
 * ------------------
 * This module defines the Elem class, which holds:
 *   - type: class, struct, enum
 *   - name
 *   - code location: start and end line numbers
 *   - size and complexity metrics: lines of code and scope count
 *  
 */
/* Public Interface Documentation
 * 
 * public class Elem        // holds scope information
 * public override string ToString()
 * 
*/
/* Required Files:
 *   Element.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 02 Nov 2018
 * - first release
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
  public class Elem  
  {
    public string type { get; set; }
    public string name { get; set; }
    public int beginLine { get; set; }
    public int endLine { get; set; }
    public int beginScopeCount { get; set; }
    public int endScopeCount { get; set; }
    public string filename { get; set; }
    public string aliasname { get; set; }

    public override string ToString()
    {
      StringBuilder temp = new StringBuilder();
      temp.Append("{");
      temp.Append(String.Format("{0,-10}", type)).Append(" : ");
      temp.Append(String.Format("{0,-10}", name)).Append(" : ");
      temp.Append(String.Format("{0,-5}", beginLine.ToString()));  
      temp.Append(String.Format("{0,-5}", endLine.ToString()));   
      temp.Append("}");
      return temp.ToString();
    }
  }
}

