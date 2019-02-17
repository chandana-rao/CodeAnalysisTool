///////////////////////////////////////////////////////////////////////////////////////////
//IRuleAndAction.cs - Interfaces & abstract bases for rules and actions                //
// Language:    C#, 2017, .Net Framework 4.6.1                                        //
// Application: Project #3, CSE681 Fall 2018                                         //
// Author:      Chandana Rao                                                        //
//  Source:     Jim Fawcett                                                        //
////////////////////////////////////////////////////////////////////////////////////
/*
/*
 * Module Operations:
 * ------------------
 * This module defines the following classes:
 *   IRule   - interface contract for Rules
 *   ARule   - abstract base class for Rules that defines some common ops
 *   IAction - interface contract for rule actions
 *   AAction - abstract base class for actions that defines common ops
 */
/* Required Files:
 *   IRuleAndAction.cs
 *   
 *   Public Interface Documentation:
 *  public interface IAction
*	public abstract class AAction : IAction
*	static public Action<string> actionDelegate
*	public interface IRule
*	public static bool displayStack
*	static public Action<string> actionDelegate;
*	public void add(IAction action)
*	public void doActions(Lexer.ITokenCollection semi)
*	public int indexOfType(Lexer.ITokenCollection semi)

 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 02 Nov 2018
 * - first release
 *
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace CodeAnalysis
{

  public interface IAction
  {
    void doAction(Lexer.ITokenCollection semi);
  }
  public abstract class AAction : IAction
  {
    static bool displaySemi_ = false;   
    static bool displayStack_ = false;  

    protected Repository repo_;

    static public Action<string> actionDelegate;

    public abstract void doAction(Lexer.ITokenCollection semi);

    public static bool displaySemi 
    {
      get { return displaySemi_; }
      set { displaySemi_ = value; }
    }
    public static bool displayStack 
    {
      get { return displayStack_; }
      set { displayStack_ = value; }
    }

    public virtual void display(Lexer.ITokenCollection semi)
    {
      if(displaySemi)
        for (int i = 0; i < semi.size(); ++i)
          Console.Write("{0} ", semi[i]);
    }
  }
  // contract for parser rules

  public interface IRule
  {
    bool test(Lexer.ITokenCollection semi);
    void add(IAction action);
  }
  // abstract rule base implementing common functions

  public abstract class ARule : IRule
  {
    private List<IAction> actions;
    static public Action<string> actionDelegate;

    public ARule()
    {
      actions = new List<IAction>();
    }
    public void add(IAction action)
    {
      actions.Add(action);
    }
    abstract public bool test(Lexer.ITokenCollection semi);
    public void doActions(Lexer.ITokenCollection semi)
    {
      foreach (IAction action in actions)
        action.doAction(semi);
    }
    public int indexOfType(Lexer.ITokenCollection semi)
    {
      int indexCL;
      semi.find("class", out indexCL);
      int indexIF;
      semi.find("interface", out indexIF);
      int indexST;
      semi.find("struct", out indexST);
      int indexEN;
      semi.find("enum", out indexEN);
      int indexDE;
      semi.find("delegate", out indexDE);

      int index = Math.Max(indexCL, indexIF);
      index = Math.Max(index, indexST);
      index = Math.Max(index, indexEN);
      index = Math.Max(index, indexDE);
      return index;
    }
  }
}

