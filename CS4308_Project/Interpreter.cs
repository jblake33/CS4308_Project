/*
 * Class:       CS 4308 Section 
 * Term:        Spring 2023
 * Name:        John Blake
 * Instructor:   Sharon Perry
 * Project:     Deliverable P3 Interpreter  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;


namespace CS4308_Project
{
    public class Interpreter
    {
		//An array to store variable names. As they are discovered while scanning the program file, they are added here.
		static Dictionary<string, int> symbol_table = new Dictionary<string, int>();

		//This is the parser's list of: line number, token ID, lexeme (each row corresponds to a lexeme).
		static List<string[]>? parserList;

		//This is a list that holds the error msgs produced by the parser
		static List<string> errorMsgList = new List<string>();

		//This is a "list" used to print the grammar (like a "stack trace" of where the parser is currently at)
		static List<string> stackTrace = new List<string>();

		//The ROOT node of the parse tree that the parser generates "in the background". This will get passed to the interpreter.
		public static ParseTreeNode? parseTree;
		public static void Main(String[] args)
		{
			Parser parser = new Parser();
			parser.RunParser();
			parseTree = Parser.parseTree;
			Console.WriteLine("-------------INTERPRETER----------------");
			if (Parser.errorMsgList.Count > 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Errors detected when parsing, interpreter aborted");
				Console.ForegroundColor = ConsoleColor.White;
			}
			else
			{
				Program(parseTree);
			}
		}
		
		// Takes a string and checks if it is wrapped in angle brackets. (ie a nonterminal)
		public bool IsTerminal(string s)
        {
			if (s[0] == '<' && s[s.Length - 1] == '>')
            {
				return false;
            }
			else
            {
				return true;
            }
        }
		//Contents below are modified Parser.cs classes and methods
		
		//Below are methods that outline the grammar, as outlined in 4308-Project-Assignment.docx
		////<program> → function id () <block> end
		static void Program(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Program"); //debug!!
			Block(node.children[2]);
		}
		//<block> → <statement> | <statement> <block>
		//This expands in a recursive fashion, a block can have any number of statements (but at least one).
		static void Block(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Block, has # children: " + node.children.Count); //debug!!
			
			Stmt(node.children[0]);
			if (node.children.Count == 2)
            {
				Block(node.children[1]);
            }
		}
		//<statement> → <if_statement> | <assignment_statement> | <while_statement> | <print_statement> | <repeat_statement>
		static void Stmt(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Stmt"); //debug!!
			string stmt_type = node.children[0].data;
			switch(stmt_type)
            {
				case "<if_statement>":
					If_stmt(node.children[0]);
					break;
				case "<assignment_statement>":
					Asgn_stmt(node.children[0]);
					break;
				case "<while_statement>":
					While_stmt(node.children[0]);
					break;
				case "<print_statement>":
					Print_stmt(node.children[0]);
					break;
				case "<repeat_statement>":
					Repeat_stmt(node.children[0]);
					break;
			}
		}

		// <if_statement> → if <boolean_expression> then <block> else <block> end
		static void If_stmt(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to If_stmt"); //debug!!
			if (Bool_expr(node.children[1]))
            {
				Block(node.children[3]);
            }
			else
            {
				Block(node.children[5]);
            }
		}

		//<while_statement> → while <boolean_expression> do <block> end
		//This method is recursive by nature.
		static void While_stmt(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to While_stmt"); //debug!!
			if (Bool_expr(node.children[1]))
			{
				Block(node.children[3]);
				While_stmt(node);
			}
		}

		//<assignment_statement> -> id <assignment_op> <arithmetic_expression> 
		static void Asgn_stmt(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Asgn_stmt"); //debug!!
			//Get the id name
			string id = node.children[0].children[0].data;
			//Check if the ID is already in symbol table
			if (symbol_table.ContainsKey(id))
            {
				symbol_table[id] = Arithmetic_expr(node.children[2]);
            }
			//if it isn't, then add it
			else
            {
				symbol_table.Add(id, Arithmetic_expr(node.children[2]));
            }
		}

		//<repeat_statement> -> repeat <block> until <boolean_expression>
		//Syntax is that of a do while loop, it is treated as such.
		static void Repeat_stmt(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Repeat_stmt"); //debug!!
			Block(node.children[1]);
			if (Bool_expr(node.children[3]))
            {
				Repeat_stmt(node);
            }
		}

		//<print_statement> → print( <arithmetic_expression> )
		static void Print_stmt(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Print_stmt"); //debug!!
			Console.WriteLine(Arithmetic_expr(node.children[1]));
		}

		//<boolean_expression> → <relative_op> <arithmetic_expression> <arithmetic_expression>
		static bool Bool_expr(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Bool_expr"); //debug!!
			string op = node.children[0].children[0].data;
			int leftHS = Arithmetic_expr(node.children[1]);
			int rightHS = Arithmetic_expr(node.children[2]);
			switch(op)
            {
				case "==":
					return (leftHS == rightHS);
				case "~=":
					return (leftHS != rightHS);
				case "<":
					return (leftHS < rightHS);
				case ">":
					return (leftHS > rightHS);
				case "<=":
					return (leftHS <= rightHS);
				case ">=":
					return (leftHS >= rightHS);
				default:
					return true;
			}
		}

		//Returns an integer value, computed from a arithmetic expression (e.g. 4+3/x*r)
		static int Arithmetic_expr(ParseTreeNode node)
		{
			//Console.WriteLine("Passed: " + node.data + " to Arith_expr"); //debug!!
			string expr = Create_Arith_Expr(node);
			DataTable dt = new DataTable();
			var v = dt.Compute(expr, "");
			dt.Dispose(); //dispose dt resources, this is a quick n' easy solution
			expr = v.ToString();
			double m = Double.Parse(expr);
			m = Math.Round(m);
			return (int)m;
		}

		// Recursive method that takes a node as the branch's head, output is a string corresponging to the expression, e.g. x+y-4/2
		static string Create_Arith_Expr(ParseTreeNode node)
		{
			string s = "";
			if (node.children.Count > 0)
			{
				foreach (ParseTreeNode child in node.children)
				{
					s += Create_Arith_Expr(child);
				}
				return s;
			}
			else
			{
				if (Regex.IsMatch(node.data, @"[a-zA-Z]"))
				{
					return "" + symbol_table[node.data];
				}
				else
				{
					return node.data;
				}
			}
		}
	}
}//*/
