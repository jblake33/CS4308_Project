/*
 * Class:       CS 4308 Section 
 * Term:        Spring 2023
 * Name:        John Blake
 * Instructor:   Sharon Perry
 * Project:     Deliverable P1 Scanner  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;

namespace CS4308_Project
{
	// Use a regular expression (regex) for defining what a type is.
	public class Scanner
	{
		//This list is what gets passed from the scanner to the parser.
		public static List<string[]> parserList = new List<string[]>();

		//This is the name of the program file to be scanned. If it doesn't exist in the proper location, an empty "MyProgramEditor.txt" should be created
		public static string DEFAULT_FILENAME = "MyProgramEditor.txt";
		public string filename = (string)DEFAULT_FILENAME.Clone();

        //Keywords are reserved words that may not be used as variable names. https://docs.julialang.org/en/v1/base/base/#Keywords
        static string[] keyword_table =
		{
"baremodule","begin","break","catch","const","continue","do","else","elseif","end","export","false","finally","for","function","global","if","import","let","local","macro","module","quote","return", "struct","then","true","try","using","while"
		};
		
		//An array to store variable names. As they are discovered while scanning the program file, they are added here.
		static List<string> symbol_table = new List<string>();

		//Tokens for the scanner to use, when assigning to lexemes.
		static ScannerToken INT_LITERAL = new ScannerToken(@"[-]?(?:0|[1-9](?:_*[0-9])*)[lL]?", 100);

		static ScannerToken EQUALS_OP = new ScannerToken(@"=", 200);
		static ScannerToken ADD_OP = new ScannerToken(@"+", 201);
		static ScannerToken SUB_OP = new ScannerToken(@"-", 202);
		static ScannerToken MULT_OP = new ScannerToken(@"*", 203);
		static ScannerToken DIV_OP = new ScannerToken(@"/", 204);
		static ScannerToken INCR_OP = new ScannerToken(@"+=", 205);
		static ScannerToken DECR_OP = new ScannerToken(@"-=", 206);
        
        static ScannerToken EQUAL_TO_BT = new ScannerToken(@"==", 300);
		static ScannerToken GREATER_THAN_BT = new ScannerToken(@">", 301);
		static ScannerToken LESS_THAN_BT = new ScannerToken(@"<", 302);
		static ScannerToken NOT_EQUAL_TO_BT = new ScannerToken(@"~=", 303);
		static ScannerToken GREATER_EQUAL_TO_BT = new ScannerToken(@">=", 304);
		static ScannerToken LESS_EQUAL_TO_BT = new ScannerToken(@"<=", 305);

		static ScannerToken IDENT = new ScannerToken(@"[a-zA-Z]", 500);

		// List of operator tokens, to evaluate basic arithmetic expressions like x = x + 1
		static ScannerToken[] scannerOpTokens = { EQUALS_OP, ADD_OP, SUB_OP, MULT_OP, DIV_OP, INCR_OP, DECR_OP };
		// List of bitwise tokens, to evaluate true/false expressions, like the 'x > 1' in while(x > 1)
		static ScannerToken[] scannerBitwiseTokens = {EQUAL_TO_BT, GREATER_THAN_BT, LESS_THAN_BT, NOT_EQUAL_TO_BT, GREATER_EQUAL_TO_BT, LESS_EQUAL_TO_BT };
		
		//public static void Main(String[] args)
		public void RunScanner()
        {
			//define variables to use for scanning
			int lineNum = 0;
			string line;
			string[] lexemes;
			int token = 0;
			List<string> errorMsgList = new List<string>();

			Console.WriteLine("Beginning scan of file: " + filename);
			
			//create file stream
			try
			{
				FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);	// If the MyProgramEditor.txt file doesn't exist, it will create the (empty) file
				StreamReader sr = new StreamReader(fs);
				
				//read and scan the program line by line
				while (!sr.EndOfStream)
				{
					//get the next line.
					line = sr.ReadLine();
					lineNum++;

					//remove white space at the beginning and end
					line = line.Trim();

					//remove comments if there are any at the end of the line
					if (line.Contains("//"))
					{
						line = line.Substring(0, line.IndexOf("//"));
					}
					//if the line is empty (after removing white space and comments)
					if (String.IsNullOrEmpty(line)) {
						
					}
					else
                    {
						//Check if line contains a function declaration, like print().
						if (line.Contains('('))
                        {
							//Check if line is a function declaration
							if (Regex.IsMatch(line, "function [a-zA-Z]" + @"\(\)"))
                            {
								parserList.Add(new string[] { ""+lineNum, ""+1, "function" });
								parserList.Add(new string[] { "" + lineNum, "" + 500, ""+line[9] });
								parserList.Add(new string[] { "" + lineNum, "" + 1, "()" });
							}
							//... otherwise it is a print statement
							else if (Regex.IsMatch(line, @"print" + @"\( {0,}\S+( \S+){0,} {0,}\)"))
                            {
								parserList.Add(new string[] { "" + lineNum, "" + 2, "print(" });
								line = line.Substring(line.IndexOf("(") + 1, line.IndexOf(")") - line.IndexOf("(") - 1);
								line = line.Trim();
								string[] vs = line.Split(' ');
								foreach (string v in vs)
								{
									token = EvaluateLexeme(v);
									switch (token)
									{
										case -1:
											errorMsgList.Add("Error at line " + lineNum + ": " + v + " not recognized");
											break;
										case 0:
											//do nothing
											break;
										default:
											parserList.Add(new string[] { "" + lineNum, "" + token, v });
											break;
									}
								}
								
								parserList.Add(new string[] { "" + lineNum, "" + 2, ")" });
							}
							else { errorMsgList.Add("Error at line " + lineNum + ": " + line + " contains unknown parentheses, or not formatted properly. \nParentheses are only to be used as: function <id>() or print(<expr>)."); }
                        }
						else if (line.Contains(' '))
						{
							lexemes = line.Split(' ');
								
							foreach(string l in lexemes)
                            {
								token = EvaluateLexeme(l);
                                switch (token)
                                {
									//Error occured, lexeme not recognized!
									case -1:
										errorMsgList.Add("Error at line " + lineNum + ": " + l + " not recognized.");
										break;
									//Empty lexeme
									case 0:
										break;
									//Lexeme is valid
									default:
										parserList.Add(new string[] { "" + lineNum, "" + token, l });
										break;
                                }
                            }
						}
						else
                        {
							token = EvaluateLexeme(line);
							switch (token)
							{
								//Error occured
								case -1:
									errorMsgList.Add("Error at line " + lineNum + ": " + line + " not recognized");
									break;
								//empty lexeme
								case 0:
									break;
								//Lexeme is valid
								default:
                                    parserList.Add(new string[] { "" + lineNum, "" + token, line });
									break;
							}
						}
                    }
				}

				//close file stream
				sr.Close();
				fs.Close();

				//print any errors found
				if (errorMsgList.Count> 0)
                {
					Console.ForegroundColor = ConsoleColor.Red;
					foreach (string errorMsg in errorMsgList)
                    {
						Console.WriteLine("\n"+errorMsg);
                    }
					Console.ForegroundColor = ConsoleColor.White;
                }
				else
                {
					Console.WriteLine("\nNo errors found while scanning");
                }

				
			}
			catch (FileNotFoundException) { Console.WriteLine("invalid file path"); return; } 
			catch (IOException) { Console.WriteLine("IO error occured"); return; }
			catch (Exception ex) { Console.WriteLine("unknown error occured: " + ex.Message); return; }
        }
		//Takes a lexeme and determines what token to assign it, such as keyword, literal,...
		//IF this returns negative number an error occured. 
		//IF this returns 0 then the lexeme was empty, do not include in the array that gets passed to the parser.
		//IF this returns 1 then the lexeme is a keyword.
		public static int EvaluateLexeme(string lexeme)
        {
			if (lexeme == null || lexeme == "")
            {
				return 0;
            }
			else
            {
				//check if lexeme is a keyword, keywords all have the ID of 001  =  1.
				foreach (string keyword in keyword_table)
                {
					
					if (keyword == lexeme)
                    {
						
						return 1;
                    }
					
				}
				//if not, check if lexeme is some operator
				foreach (ScannerToken s in scannerOpTokens)
                {
					if (s.pattern == lexeme) {
						return s.ID;
                    }
                }
				
				//if not, check if lexeme is some bitwise op 
				foreach (ScannerToken s in scannerBitwiseTokens)
                {
					if (s.pattern == lexeme)
                    {
						return s.ID;
                    }
                }
                //if not, check if lexeme is a int literal
				if (Regex.IsMatch(lexeme, INT_LITERAL.pattern))
                {
					return INT_LITERAL.ID;
                }
				//if not, check if lexeme is an identifier. Identifiers must be a SINGLE letter, [a-zA-Z].
				if (Regex.IsMatch(lexeme, IDENT.pattern) && lexeme.Length == 1)
                {
					if (!symbol_table.Contains(lexeme))
					{

						symbol_table.Add(lexeme);
					}
					return IDENT.ID;
                }
				
                
				//failed to assign a token to the lexeme, unknown lexeme.
				return -1;
            }
        }
	}
}
