/*
 * Class:       CS 4308 Section 
 * Term:        Spring 2023
 * Name:        John Blake
 * Instructor:   Sharon Perry
 * Project:     Deliverable P2 Parser 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CS4308_Project
{
	//ParseTreeNode is a class that will be used in creating the parse tree, which gets passed to the interpreter.
	public class ParseTreeNode
    {
		public string data;
		public List<ParseTreeNode> children;
		
		//Constructor
		public ParseTreeNode(string d)
        {
			data = d;
			children = new List<ParseTreeNode>();
        }

		//Add a child to this node's list of children.
		public void AddChild(string d)
        {
			children.Add(new ParseTreeNode(d));
        }

		//Get data from this node.
		public string GetData()
        {
			return data;
        }

		//Checks if node is a non-terminal: if it's wrapped in <...> then it's a nonterminal. otherwise its a terminal
		public bool IsLeaf()
        {
			if (data[0] == '<' && data[data.Length - 1] == '>')
            {
				return true;
            }
			else
            {
				return false;
            }
        }
    }
    public class Parser
    {
		//This is the parser's list of: line number, token ID, lexeme (each row corresponds to a lexeme).
		static List<string[]>? parserList;
		
		//This is a list that holds the error msgs produced by the parser
		public static List<string> errorMsgList = new List<string>();

		//This is a "list" used to print the grammar (like a "stack trace" of where the parser is currently at)
		static List<string> stackTrace = new List<string>();

		//The ROOT node of the parse tree that the parser generates "in the background". This will get passed to the interpreter.
		public static ParseTreeNode? parseTree;
		public void RunParser()
        {
			//Start by running the scanner
			Scanner scan = new Scanner();
			Console.WriteLine("-------------SCANNER----------------");
			scan.RunScanner();
			parserList = Scanner.parserList;

			Console.WriteLine("Line\tToken\tLexeme");
			foreach(string[] s in parserList)
            {
				foreach(string s2 in s)
                {
					Console.Write(s2 + "\t");
                }
				Console.WriteLine();
            }
			//Check if the program doesn't start with function <id>() or end with the end keyword.
			if (!Regex.IsMatch(parserList[0][2] + " " + parserList[1][2] + parserList[2][2], @"function [a-zA-Z]\(\)") || parserList[parserList.Count-1][2] != "end")
            {
				Bad_start_errorMsg();
				return; //this ends the parsing process
            }
			//Start parsing if no immediate errors were found
			Console.WriteLine("-------------PARSER----------------");

			//Special "flag" value to handle if an exception was thrown (fatal error that means the parser can't proceed)
			bool wasErrorFound = false;
			try
			{
				Program();
			}
			catch (Exception ex)
            {
				Console.ForegroundColor = ConsoleColor.Red;
				wasErrorFound = true;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.White;
            }
			
			//print out the parsing errors to the console
			if (errorMsgList.Count > 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				foreach (string errorMsg in errorMsgList)
				{
					Console.WriteLine("\n" + errorMsg);
				}
				Console.ForegroundColor = ConsoleColor.White;
			}
			else
			{
				//if no errors were found
				if (!wasErrorFound)
				{
					Console.WriteLine("\nNo errors found while parsing.");
				}
			}
			
		}
		
		//Below are methods that outline the grammar, as outlined in 4308-Project-Assignment.docx

		////<program> → function id () <block> end
		static void Program()
        {
			//Calls to the UpdStackTrace method are responsible for just printing the grammar to the terminal as parsing is occurring.
			UpdStackTrace(false, 0, "program-> ", false);
			UpdStackTrace(false, 1, "function", false);
			UpdStackTrace(false, 2, "<id>", false);
			UpdStackTrace(false, 3, "()", false);
			UpdStackTrace(false, 4, "<block>", false);
			UpdStackTrace(false, 5, "end", true);

			//This is the root node of the parse tree.
			parseTree = new ParseTreeNode("<program>");

			//Elements get added to the parse tree before they are removed from the parserList.
			parseTree.AddChild("function");
			ParseNext("function");
			
			UpdStackTrace(true, 2, parserList[0][2], true);
			
			//Calls to parse off what would be a terminal are handled with a special case:
			parseTree.AddChild("<id>");
			parseTree.children[1] = new ParseTreeNode(parserList[0][2]);
			ParseNext("500");
			
			parseTree.AddChild("()");
			ParseNext("()");

			parseTree.AddChild("<block>");
			Block(4, parseTree.children[2]);

			parseTree.AddChild("end");
			ParseNext("end");
        }
		//<block> → <statement> | <statement> <block>
		static void Block(int index, ParseTreeNode node)
		{
			UpdStackTrace(true, index, "<statement>", true);
			int oldLen = stackTrace.Count;
			node.AddChild("<statement>");
			Stmt(index, node.children[0]);
			//Recursive call. Keep looping until a end is found. Or, if any errors were found, stop looping (parsing).
			if (!CheckNextToken("end") && !CheckNextToken("else") && errorMsgList.Count == 0)
            {
				UpdStackTrace(false, index + 1 + stackTrace.Count - oldLen, "<block>", true);
				node.AddChild("<block>");
				Block(index + stackTrace.Count - oldLen, node.children[1]);
            }
			else
            {
				return;
            }
		}
		//<statement> → <if_statement> | <assignment_statement> | <while_statement> | <print_statement> | <repeat_statement>
		static void Stmt(int index, ParseTreeNode node)
        {
			if (CheckNextToken("if"))
            {
				UpdStackTrace(true, index, "<if_statement>", true);
				node.AddChild("<if_statement>");
				If_stmt(index, node.children[0]);
            }
			else if (CheckNextToken("500"))
            {
				UpdStackTrace(true, index, "<assignment_statement>", true);
				node.AddChild("<assignment_statement>");
				Asgn_stmt(index, node.children[0]);
			}
			else if (CheckNextToken("while"))
            {
				UpdStackTrace(true, index, "<while_statement>", true);
				node.AddChild("<while_statement>");
				While_stmt(index, node.children[0]);
            }
			else if (CheckNextToken("print("))
            {
				UpdStackTrace(true, index, "<print_statement>", true);
				node.AddChild("<print_statement>");
				Print_stmt(index, node.children[0]);
			}
			else if (CheckNextToken("repeat"))
            {
				UpdStackTrace(true, index, "<repeat_statement>", true);
				node.AddChild("<repeat_statement>");
				Repeat_stmt(index, node.children[0]);
            }
			else
            {
				errorMsgList.Add(new string("Unknown statement starting at line " + parserList[0][0]));
            }
        }

		// <if_statement> → if <boolean_expression> then <block> else <block> end
		static void If_stmt(int index, ParseTreeNode node)
        {
			UpdStackTrace(true, index, "if", false);
			UpdStackTrace(false, index + 1, "<boolean_expression>", false);
			UpdStackTrace(false, index + 2, "then", false);
			UpdStackTrace(false, index + 3, "<block>", false);
			UpdStackTrace(false, index + 4, "else", false);
			UpdStackTrace(false, index + 5, "<block>", false);
			UpdStackTrace(false, index + 6, "end", true);

			int oldLen = stackTrace.Count;
			node.AddChild("if");
			ParseNext("if");
			node.AddChild("<boolean_expression>");
			Bool_expr(index + 1, node.children[1]);
			node.AddChild("then");
			ParseNext("then");
			node.AddChild("<block>");
			Block(index + 3 + stackTrace.Count - oldLen, node.children[3]);
			node.AddChild("else");
			ParseNext("else");
			node.AddChild("<block>");
			Block(index + 5 + stackTrace.Count - oldLen, node.children[5]);
			node.AddChild("end");
			ParseNext("end");
        }

		//<while_statement> → while <boolean_expression> do <block> end
		static void While_stmt(int index, ParseTreeNode node)
        {
			UpdStackTrace(true, index, "while", false);
			UpdStackTrace(false, index + 1, "<boolean_expression>", false);
			UpdStackTrace(false, index + 2, "do", false);
			UpdStackTrace(false, index + 3, "<block>", false);
			UpdStackTrace(false, index + 4, "end", true);

			int oldLen = stackTrace.Count;
			node.AddChild("while");
			ParseNext("while");
			node.AddChild("<boolean_expression>");
			Bool_expr(index + 1, node.children[1]);
			node.AddChild("do");
			ParseNext("do");
			node.AddChild("<block>");
			Block(index + 3 + stackTrace.Count - oldLen, node.children[3]);
			node.AddChild("end");
			ParseNext("end");
		}

		//<assignment_statement> -> id <assignment_op> <arithmetic_expression> 
		static void Asgn_stmt(int index, ParseTreeNode node)
        {
			UpdStackTrace(true, index, "<id>", false);
			UpdStackTrace(false, index + 1, "<assignment_operator>", false);
			UpdStackTrace(false, index + 2, "<arithmetic_expression>", true);

			UpdStackTrace(true, index, parserList[0][2], true);
			node.AddChild("<id>");
			node.children[0].AddChild(parserList[0][2]);
			ParseNext("500");
			// = would have a normal arithmetic expr on the right
			if (CheckNextToken("200"))
            {
				UpdStackTrace(true, index + 1, parserList[0][2], true);
				node.AddChild("<assignment_op>");
				node.children[1].AddChild("=");
				ParseNext("200");
				node.AddChild("<arithmetic_expression>");
				Arithmetic_expr(index + 2, node.children[2]);
			}
			// += would have a special arithmetic expr on the right, <id> += <expr> <expr>
			else if (CheckNextToken("205"))
			{
				UpdStackTrace(true, index + 1, parserList[0][2], true);
				node.AddChild("<increment_op>");
				node.children[1].AddChild("+=");
				ParseNext("205");
				UpdStackTrace(false, index + 3, "<arithmetic_expression>", true);
				int oldLen = stackTrace.Count;
				node.AddChild("<arithmetic_expression>");
				Arithmetic_expr(index + 2, node.children[2]);
				node.AddChild("<arithmetic_expression>");
				Arithmetic_expr(index + 3 + stackTrace.Count - oldLen, node.children[3]);
			}
			// -= would have a special arithmetic expr on the right, <id> -= <expr> <expr>
			else if (CheckNextToken("206"))
			{
				UpdStackTrace(true, index + 1, parserList[0][2], true);
				node.AddChild("<decrement_op>");
				node.children[1].AddChild("-=");
				ParseNext("206");
				UpdStackTrace(false, index + 3, "<arithmetic_expression>", true);
				int oldLen = stackTrace.Count;
				node.AddChild("<arithmetic_expression>");
				Arithmetic_expr(index + 2, node.children[2]);
				node.AddChild("<arithmetic_expression>");
				Arithmetic_expr(index + 3 + stackTrace.Count - oldLen, node.children[3]);
			}
        }

		//<repeat_statement> -> repeat <block> until <boolean_expression>
		static void Repeat_stmt(int index, ParseTreeNode node)
        {
			UpdStackTrace(true, index, "repeat", false);
			UpdStackTrace(false, index + 1, "<block>", false);
			UpdStackTrace(false, index + 2, "until", false);
			UpdStackTrace(false, index + 3, "<boolean_expression>", true);

			int oldLen = stackTrace.Count;
			node.AddChild("repeat");
			ParseNext("repeat");
			node.AddChild("<block>");
			Block(index + 1, node.children[1]);
			node.AddChild("until");
			ParseNext("until");
			node.AddChild("<boolean_expression>");
			Bool_expr(index + 3 + stackTrace.Count - oldLen, node.children[3]);
        }

		//<print_statement> → print( <arithmetic_expression> )
		static void Print_stmt(int index, ParseTreeNode node)
        {
			UpdStackTrace(true, index, "print(", false);
			UpdStackTrace(false, index+1, "<arithmetic_expression>", false);
			UpdStackTrace(false, index+2, ")", true);

			node.AddChild("print(");
			ParseNext("print(");
			node.AddChild("<arithmetic_expression>");
			Arithmetic_expr(index + 1, node.children[1]);
			node.AddChild(")");
			ParseNext(")");
        }

		//<boolean_expression> → <relative_op> <arithmetic_expression> <arithmetic_expression>
		static void Bool_expr(int index, ParseTreeNode node)
        {
			//Has to return if a match is found with one of the logic ops, otherwise an error is added
			for (int i = 300; i <= 305; i++)
            {
				if (CheckNextToken(""+i))
				{
					UpdStackTrace(true, index, "<relative_op>", false);
					UpdStackTrace(false, index + 1, "<arithmetic_expression>", false);
					UpdStackTrace(false, index + 2, "<arithmetic_expression>", true);

					UpdStackTrace(true, index, parserList[0][2], true);
					node.AddChild("<relative_op>");
					node.children[0].AddChild(parserList[0][2]);
					ParseNext("" + i);
					node.AddChild("<arithmetic_expression>");
					Arithmetic_expr(index + 1, node.children[1]);
					node.AddChild("<arithmetic_expression>");
					Arithmetic_expr(index + 2, node.children[2]);
					return;
				}
			}
			errorMsgList.Add("Logic operator expected first at line " + parserList[0][0] + ", syntax is <boolean_expression> → <relative_op> <arithmetic_expression> <arithmetic_expression>");
		}

		//<arithmetic_expression> → <id> | <literal_integer> | <arithmetic_op> <arithmetic_expression> <arithmetic_expression>
		//This grammar is probably wrong. Modified: <arithmetic_expression> → <id> | <literal_integer> | <arithmetic_op> <arithmetic_expression>
		static void Arithmetic_expr(int index, ParseTreeNode node)
        {
			if (CheckNextToken("500") || CheckNextToken("100"))
            {
				//Check if next token is ID
				if (CheckNextToken("500"))
                {
					UpdStackTrace(true, index, "<id>", true);
					UpdStackTrace(true, index, parserList[0][2], true);
					node.AddChild("<id>");
					node.children[0].AddChild(parserList[0][2]);
					ParseNext("500");
                }
				//Check if next token is integer literal
				else if (CheckNextToken("100"))
                {
					UpdStackTrace(true, index, "<literal_integer>", true);
					UpdStackTrace(true, index, parserList[0][2], true);
					node.AddChild("<literal_integer>");
					node.children[0].AddChild(parserList[0][2]);
					ParseNext("100");
                }

				//Check if next token is op (after parsing the 500 or 100 first)
				for (int i = 201; i <= 204; i++)
				{
					if (CheckNextToken(""+i))
					{
						UpdStackTrace(false, index + 1, "<arithmetic_op>", false);
						UpdStackTrace(false, index + 2, "<arithmetic_expr>", true);
						UpdStackTrace(true, index + 1, parserList[0][2], true);
						node.AddChild("<arithmetic_op>");
						node.AddChild("<arithmetic_expr>");
						node.children[1].AddChild(parserList[0][2]);
						ParseNext("" + i);
						Arithmetic_expr(index+2, node.children[2]);
					}
				}
            }
			else
            {
				errorMsgList.Add("Error on line " + parserList[0][0] + ": syntax for arithmetic expressions is <arithmetic_expression> → <id> | <literal_integer> | <arithmetic_op> <arithmetic_expression> <arithmetic_expression>");
            }
        }
		
		//This error prints to the screen if the program doesn't start with function y() or end with the end keyword.
		//This error is special since the program can't be parsed properly without this formatting.
		static void Bad_start_errorMsg()
        {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("\nParsing failed. Program must start with ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("function x()");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(" as the first line and end with ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("end");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(" as the last line.");
			Console.ForegroundColor = ConsoleColor.White;
		}
		//Checks if next token in parser list is val <- val can either be a token ID or a lexeme
		static bool CheckNextToken(string val)
        {
			int temp;
			if (parserList.Count == 0)
            {
				return false;
            }
			//Attemt to parse val (is val a token ID?)
			if (Int32.TryParse(val, out temp))
            {
				if (parserList[0][1] == val)
                {
					return true;
                }
				else
                {
					return false;
                }
            }
			// If val is not a token ID then it is a lexeme (such as a keyword)
			else
            {
				if (parserList[0][2] == val)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
        }
		//Parses the token val from the list. Since this is a LL parsing, the "next token" is always the first element in our parser list
		static void ParseNext(string val)
        {
			if (CheckNextToken(val))
            {
				parserList.RemoveAt(0);
            }
			else { 
				throw new Exception("Parser attempted to parse the token " + val + " on line " + parserList[0][0] + " in the parserList which failed.");
			}
        }
		//Responsible for printing the grammar to the terminal
		static void UpdStackTrace(bool replace, int i, string s, bool print)
		{
			if (replace)
			{
				//Check if the term at index i is a nonterminal (which makes sense to replace)
				if (stackTrace[i].Contains('<') && stackTrace[i].Contains('>'))
				{
					stackTrace[i] = s;
				}
				//Otherwise the term at index i is a terminal (which does NOT make sense to  replace)
				else
				{
					throw new Exception("Parser attempted to replace " + stackTrace[i] + " at index " + i + " in the stackTrace with " + s + " which failed.");
				}
			}
			else { stackTrace.Insert(i, s); }
			//This prints the stackTrace list to the console, if the passed parameter print is true.
			if (print)
			{
				foreach (string z in stackTrace)
				{
					Console.Write(z + " ");
				}
				Console.WriteLine();
			}
		}
	}
}//*/
