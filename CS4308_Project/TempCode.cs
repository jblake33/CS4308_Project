/*

//Create a parse tree containing x - y + 4
ParseTreeNode subsubexpr = new ParseTreeNode("<arithmetic_expression>");
subsubexpr.AddChild("<literal_integer>");
subsubexpr.children[0].AddChild("4");

ParseTreeNode subexpr = new ParseTreeNode("<arithmetic_expression>");
subexpr.AddChild("<id>");
subexpr.children[0].AddChild("y");
subexpr.AddChild("<arithmetic_op>");
subexpr.children[1].AddChild("+");
subexpr.children.Add(subsubexpr);

ParseTreeNode head = new ParseTreeNode("<arithmetic_expression>");
head.AddChild("<id>");
head.children[0].AddChild("x");
head.AddChild("<arithmetic_op>");
head.children[1].AddChild("-");
head.children.Add(subexpr);

parseTree = head;
//End of parse tree creation...

symbol_table.Add("x", 10); //Add a key to the symbol table, the variable name is the unique identifier...
symbol_table.Add("y", -90);
symbol_table["x"] = 10; //This assigns the value 10 to the variable x in the symbol table. It can be replaced with another value....

//print a parse tree to the console
PrintParseTree(parseTree, 0);
			//Print the parse tree to console, recursive method
			static void PrintParseTree(ParseTreeNode node, int spacing)
            {
				for (int i = 0; i < spacing; i++)
                {
					Console.Write(" ");
                }
				Console.WriteLine(node.data);
				foreach(ParseTreeNode n in node.children)
                {
					PrintParseTree(n, spacing+1);
                }
            }

*/