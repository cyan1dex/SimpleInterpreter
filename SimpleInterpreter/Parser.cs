using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace SimpleInterpreter
{
   public class Parser : Symbol
   {
      private int getPos() { return input.getPos(); }
      private int tokenValue(int i) { return input.tokenValue(i).Type; }
      private Token tokenAssignment(int i) { return input.tokenValue(i); }
      private void consume(int val) { input.consume(val); }
      private void consume() { input.consume(1); }
      private void discard(int c) { consume(c); }

      public Parser(Lexer t)
      {
         variables = new Hashtable(8192);
         stk = new Stack();
         input = new TokenBuffer(t, 2); //Parse two tokens at a time
         streamOut = null;
      }

      public Parser(TokenBuffer t)
      {
         variables = new Hashtable(8192);
         stk = new Stack();
         input = t;
         streamOut = null;
      }

      private void setPos(int i)
      {
         input.setPos(i);
         for (int ii = 0; ii < 2; ii++)
         {
            consume();
         }
      }

      public int getCount
      {
         get { return input.getCount; }
      }

      public object Evaluate()
      {
         if (tokenValue(1) == TOKEN_BEGIN) //TODO: UTILIZE BEGIN AND END TOKENS
         { beginOk = true; discard(1); }

         while (tokenValue(1) != TOKEN_EOF && beginOk == true)
         {
            if (tokenValue(1) == Reserved.TOKEN_IF)
               parseIf();
            else if (tokenValue(1) == Reserved.TOKEN_READ && tokenValue(2) == TOKEN_IDENTIFIER)
               readIn();
            else if (tokenValue(1) == Reserved.TOKEN_LET && tokenValue(2) == TOKEN_IDENTIFIER)
               assignVariable();
            else if (tokenValue(1) == TOKEN_IDENTIFIER && tokenValue(2) == Special.TOKEN_EQUALS)
               assignVariable();
            else if (tokenValue(1) == TOKEN_IDENTIFIER && tokenValue(2) == TOKEN_LBRACK)
               assignArray();
            else if (tokenValue(1) == TOKEN_IDENTIFIER && tokenValue(2) == TOKEN_LPAREN)
            {
               arrayHandler();
               discard(1);
            }
            else if (tokenValue(1) == Reserved.TOKEN_PRINTLN)
               println();
            else if (tokenValue(1) == Reserved.TOKEN_PRINT)
               print();
            else if (tokenValue(1) == Primitive.TOKEN_INTEGER)
               initializeVariable();
            else if (tokenValue(1) == Primitive.TOKEN_BOOLEAN)
               initializeVariable();
            else if (tokenValue(1) == Primitive.TOKEN_STRING)
               initializeVariable();
            else if (tokenValue(1) == Primitive.TOKEN_DOUBLE)
               initializeVariable();
            else if (tokenValue(1) == Reserved.TOKEN_DEFARRAY)
               defArray();
            else if (tokenValue(1) == Reserved.TOKEN_WHILE)
               parseWhile();
            else if (tokenValue(1) == Reserved.TOKEN_EWHILE) //End While
               break;
            else if (tokenValue(1) == Reserved.TOKEN_FOR)
               parseFor();
            else if (tokenValue(1) == Reserved.TOKEN_NEXT)
               break;
            else if (tokenValue(1) == Reserved.TOKEN_EXIT) //EXIT used for exiting from IF statement, set ifExit to true
            {
               ifExit = true;
               break;
            }
            else if (tokenValue(1) == TOKEN_END) //End program or function
            {
               discard(1);
               //if (tokenValue(1) == TOKEN_ENDLINE && tokenValue(2) == TOKEN_EOF)
               //{ exitOk = true; }
            }
            else if (tokenValue(1) == Reserved.TOKEN_DEFFUNC)
               createFX();
            else
            {
               Expression();
               discard(1);
            }
         }

         if (beginOk == false)
            throw new Exception("Missing BEGIN token");
         else
            return current;
      }

      #region branching
      void parseFor() //For Loop
      {
         int retPos;
         int depth;
         string nameVar;
         discard(1);
         nameVar = tokenAssignment(1).Text;
         forAssignVar();
         int iFor = Convert.ToInt32(current);
         retPos = getPos();   // save retval
         discard(1);
         Expression();
         int iTo = Convert.ToInt32(current);
         while (iFor <= iTo)
         {
            discard(1);
            if (tokenValue(1) == TOKEN_ENDLINE)
               discard(1);
            Evaluate();
            if (ifExit)
            {
               ifExit = false;
               break;
            }
            setPos(retPos); // return to start of loop
            iFor++;
            setVariable(nameVar, (object)iFor, null);
         }
         depth = 1;
         do
         {

            discard(1);
            if (tokenValue(1) == Reserved.TOKEN_FOR) depth++;
            if (tokenValue(1) == Reserved.TOKEN_NEXT) depth--;
         } while (depth > 0); //while in inner loop
         discard(1);
      }

      void parseWhile() //While Loop
      {
         int depth;
         int retPos;

         retPos = getPos();
         discard(1);
         Expression();
         bool dowhile = bool.Parse(current.ToString());
         while (dowhile)
         {
            discard(1);
            Evaluate();
            if (ifExit)
            {
               ifExit = false;
               break;
            }
            setPos(retPos - 1); //Return position needs to be offset -1 to correctly loop
            //discard(1);
            Expression();
            dowhile = bool.Parse(current.ToString());
         }
         // skip body to endwhile
         depth = 1;
         do
         {
            discard(1);
            if (tokenValue(1) == Reserved.TOKEN_WHILE) depth++;
            if (tokenValue(1) == Reserved.TOKEN_EWHILE) depth--;
         } while (depth > 0);

         discard(1);
      }

      void parseIf() //If Statement
      {
         discard(1);
         Expression();
         bool doIf = bool.Parse(current.ToString());
         discard(1);

         if (tokenValue(1) == Reserved.TOKEN_THEN)
         {
            discard(1);
         }
         else
            throw new Exception("Missing THEN statement");

         if (doIf) //If statement is true, execute statement
         {
            Evaluate();
         }
         else //If statement is false, skip statement
         {
            int depth = 1;
            input.saveLineBreak = true;
            while (tokenValue(2) != TOKEN_ENDLINE || depth > 0)
            {
               if (tokenValue(1) == Reserved.TOKEN_IF)
                  depth++;
               if (tokenValue(2) == Reserved.TOKEN_ENDLINE)
               { depth--; discard(2); if (depth == 0) break; }
               if (tokenValue(2) != Reserved.TOKEN_ENDLINE)
                  discard(1); //discard all tokens till next linebreak
            }
            input.saveLineBreak = false;
            { lineBreakcount++; }//discard linebreak
         }
         discard(1);
      }
      #endregion
      #region arrays
      void assignArray()
      {
         string aname = tokenAssignment(1).Text;
         discard(1);
         discard(1);
         Expression();

         discard(1);
         object ind = current;
         discard(1);
         discard(1);
         Expression();
         object value = current;
         setVariable(aname, value, ind);
         // discard next

         discard(1);
      }

      void defArray()
      {
         discard(1);
         string aname = tokenAssignment(1).Text;
         discard(TOKEN_IDENTIFIER);
         discard(1); //discard Left bracket
         Expression();
         discard(1); //discard array size
         ArrayList value = new ArrayList();

         for (int i = 0; i < (int)current; i++) //create array of declared size
         {
            value.Add(null);
         }
         addVariable(aname, value);

         discard(1); //discard RIght bracket
      }

      void add(ArrayList name, object val) //Add name to the array
      {
         name.Add(val);
      }

      object length(ArrayList name) //Get array size
      {
         return name.Count;
      }

      object arrayHandler()
      {
         ArrayList fparams = new ArrayList();
         string fname = tokenAssignment(1).Text;
         string parameter = "";

         object retval = null;
         discard(1);
         while (tokenValue(2) != TOKEN_RPAREN)
         {
            discard(1);
            if (tokenValue(1) != TOKEN_COMMA)
            {
               parameter = tokenAssignment(1).Text;
               Expression();
               fparams.Add(current);
            }
         }
         string fnameup = fname.ToUpper();

         if (fnameup.Equals("ADD")) //Add to the array
         {
            add((ArrayList)fparams[0], fparams[1]);
            discard(1);
         }
         else if (fnameup.Equals("LENGTH")) //get size of array
         {
            retval = length((ArrayList)fparams[0]);
            discard(1);
         }
         else
         {
            discard(1);
            retval = executeFX(fname, parameter);  //this parameter is radius
         }
         return retval; //if name.Equals 
      }
      #endregion
      #region factoring

      void SignedFactor()
      {
         if (tokenValue(1) == Special.TOKEN_MINUS)
         {
            current = (int)0; //assign current as 0
            push(current);
            Subtract(); //Negate value
         }
         else if (tokenValue(1) == Special.TOKEN_NOT)
         {
            current = (bool)false; //current is false
            push(current);
            Not();
         }
         else Factor();
      }

      void Factor()   //factor operations
      {
         if (tokenValue(1) == TOKEN_LPAREN)
         {
            discard(1);
            Expression();
            discard(1);
         }
         else if (tokenValue(1) == TOKEN_IDENTIFIER)
         {
            if (tokenValue(2) == TOKEN_LBRACK)//identifier is for an array
            {
               string name = tokenAssignment(1).Text; //save array name
               discard(1); //discard identifier
               discard(1); //discard left bracket
               Expression(); //get array position
               discard(1);//discard array position
               current = getVariable(name, current); //get value at array position
               discard(1);//discard right bracket
               discard(1);//discard right bracket
            }
            else if (tokenValue(2) == TOKEN_LPAREN)
               current = arrayHandler();
            else current = getVariable(tokenAssignment(1).Text, null);
         }
         else if (tokenValue(1) == Constant.CONSTANT_DBL) current = Double.Parse(tokenAssignment(1).Text);
         else if (tokenValue(1) == Constant.CONSTANT_INT) current = Int32.Parse(tokenAssignment(1).Text);
         else if (tokenValue(1) == Constant.CONSTANT_BOOL) current = bool.Parse(tokenAssignment(1).Text);
         else if (tokenValue(1) == Constant.CONSTANT_STRNG) current = tokenAssignment(1).Text;
         else throw new Exception((tokenValue(1)).ToString() + " type is undefined ");
      }
      #endregion
      #region arithmetic
      void FirstOrderOp()
      {
         while (tokenValue(2) == Special.TOKEN_MULT || tokenValue(2) == Special.TOKEN_DIV || tokenValue(2) == Special.TOKEN_MOD || 
            tokenValue(2) == Special.TOKEN_SQRT || tokenValue(2) == Special.TOKEN_POW)
         {
            push(current);
            if (tokenValue(2) == Special.TOKEN_MULT) Multiply();
            else if (tokenValue(2) == Special.TOKEN_DIV) Divide();
            else if (tokenValue(2) == Special.TOKEN_MOD) Modulus();
            else if (tokenValue(2) == Special.TOKEN_SQRT) SqrRoot();
            else if (tokenValue(2) == Special.TOKEN_POW) Pow();
         }
      }

      void Pow()
      {
         Object old;
         discard(1);

         discard(1);
         Factor();
         old = pop();
         if ((current is int) && (old is int)) current = Math.Pow((int)old, (int)current);
         else if ((current is int) && (old is double)) current = Math.Pow((double)old, (int)current);
         else if ((current is double) && (old is int)) current = Math.Pow((int)old, (double)current);
         else if ((current is double) && (old is double)) current = Math.Pow((double)old, (double)current);
      }

      void Multiply()
      {
         Object old;
         discard(1);

         discard(1);
         Factor();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old * (int)current;
         else if ((current is int) && (old is double)) current = (double)old * (int)current;
         else if ((current is double) && (old is int)) current = (int)old * (double)current;
         else if ((current is double) && (old is double)) current = (double)old * (double)current;
      }

      void Divide()
      {
         Object old;
         discard(1);

         discard(1);
         Factor();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old / (int)current;
         else if ((current is int) && (old is double)) current = (double)old / (int)current;
         else if ((current is double) && (old is int)) current = (int)old / (double)current;
         else if ((current is double) && (old is double)) current = (double)old / (double)current;
      }

      void Modulus()
      {
         Object old;
         discard(1);

         discard(1);
         Factor();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old % (int)current;
         else if ((current is int) && (old is double)) current = (double)old % (int)current;
         else if ((current is double) && (old is int)) current = (int)old % (double)current;
         else if ((current is double) && (old is double)) current = (double)old % (double)current;
      }

      void SqrRoot()
      {
         //Object old;
         discard(1);

         //discard(1);
         //Factor();
         //old = pop();
         if ((current is int)) current = Math.Sqrt((int)current);
         else if ((current is double)) current = Math.Sqrt((double)current);
      }

      void SecondOrderOp()
      {
         while (tokenValue(2) == Special.TOKEN_PLUS || tokenValue(2) == Special.TOKEN_MINUS)
         {
            push(current);
            if (tokenValue(2) == Special.TOKEN_PLUS) Add();
            else if (tokenValue(2) == Special.TOKEN_MINUS) Subtract();
         }
      }

      void Add()
      {
         Object old;
         discard(1);

         discard(1);
         Factor(); FirstOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old + (int)current;
         else if ((current is int) && (old is double)) current = (double)old + (int)current;
         else if ((current is double) && (old is int)) current = (int)old + (double)current;
         else if ((current is double) && (old is double)) current = (double)old + (double)current;
         else if ((current is string) && (old is string)) current = (string)old + (string)current;
      }

      void Subtract()
      {
         Object old;
         if (tokenValue(2) == Special.TOKEN_MINUS) discard(1);

         discard(1);
         Factor(); FirstOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old - (int)current;
         else if ((current is int) && (old is double)) current = (double)old - (int)current;
         else if ((current is double) && (old is int)) current = (int)old - (double)current;
         else if ((current is double) && (old is double)) current = (double)old - (double)current;
      }
      #endregion
      #region relationalOperators
      void RelationalOp()
      {
         while (tokenValue(2) == Special.TOKEN_GREATER || tokenValue(2) == Special.TOKEN_GREATEREQ ||
                tokenValue(2) == Special.TOKEN_LESS || tokenValue(2) == Special.TOKEN_LESSEQ ||
                 tokenValue(2) == Special.TOKEN_EQUALS || tokenValue(2) == Special.TOKEN_NEQUALS)
         {
            push(current);
            if (tokenValue(2) == Special.TOKEN_GREATER) Greater();
            else if (tokenValue(2) == Special.TOKEN_GREATEREQ) GreaterEq();
            else if (tokenValue(2) == Special.TOKEN_LESS) Less();
            else if (tokenValue(2) == Special.TOKEN_LESSEQ) LessEq();
            else if (tokenValue(2) == Special.TOKEN_EQUALS) Equals();
            else NEquals();
         }
      }

      void Greater()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old > (int)current;
         else if ((current is int) && (old is double)) current = (double)old > (int)current;
         else if ((current is double) && (old is int)) current = (int)old > (double)current;
         else if ((current is double) && (old is double)) current = (double)old > (double)current;
      }

      void GreaterEq()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old >= (int)current;
         else if ((current is int) && (old is double)) current = (double)old >= (int)current;
         else if ((current is double) && (old is int)) current = (int)old >= (double)current;
         else if ((current is double) && (old is double)) current = (double)old >= (double)current;

      }

      void Less()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old < (int)current;
         else if ((current is int) && (old is double)) current = (double)old < (int)current;
         else if ((current is double) && (old is int)) current = (int)old < (double)current;
         else if ((current is double) && (old is double)) current = (double)old < (double)current;
      }

      void LessEq()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old <= (int)current;
         else if ((current is int) && (old is double)) current = (double)old <= (int)current;
         else if ((current is double) && (old is int)) current = (int)old <= (double)current;
         else if ((current is double) && (old is double)) current = (double)old <= (double)current;
      }

      void Equals()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old == (int)current;
         else if ((current is int) && (old is double)) current = (double)old == (int)current;
         else if ((current is double) && (old is int)) current = (int)old == (double)current;
         else if ((current is double) && (old is double)) current = (double)old == (double)current;
         else if ((current is bool) && (old is bool)) current = (bool)old == (bool)current;
      }

      void NEquals()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp();
         old = pop();
         if ((current is int) && (old is int)) current = (int)old != (int)current;
         else if ((current is int) && (old is double)) current = (double)old != (int)current;
         else if ((current is double) && (old is int)) current = (int)old != (double)current;
         else if ((current is double) && (old is double)) current = (double)old != (double)current;
      }
      #endregion
      #region booleanOps
      void BooleanOp()
      {
         while (tokenValue(2) == Special.TOKEN_AND || tokenValue(2) == Special.TOKEN_OR || tokenValue(2) == Special.TOKEN_NOT)
         {
            push(current);
            if (tokenValue(2) == Special.TOKEN_AND) And();
            else if (tokenValue(2) == Special.TOKEN_OR) Or();
            else if (tokenValue(2) == Special.TOKEN_NOT) Not();
         }
      }

      void And()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp(); RelationalOp();
         old = pop();
         if ((current is bool) && (old is bool)) current = (bool)old && (bool)current;
      }

      void Or()
      {
         Object old;
         discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp(); RelationalOp();
         old = pop();
         if ((current is bool) && (old is bool)) current = (bool)old || (bool)current;
      }

      void Not()
      {
         Object old;
         if (tokenValue(2) == Special.TOKEN_NOT) discard(1);

         discard(1);
         SignedFactor(); FirstOrderOp(); SecondOrderOp(); RelationalOp();
         old = pop();
         if (current is bool) current = !(bool)current;
      }
      #endregion
      #region Variables
      void assignVariable()
      {
         string name = tokenAssignment(2).Text;
         discard(1);
         discard(1);
         if (tokenValue(1) == Special.TOKEN_EQUALS)
            discard(1);
         Expression(); //value of object variable determined by expression
         object value = current;

         setVariable(name, value, null);
         discard(1);
      }

      void initializeVariable()
      {
         string name = tokenAssignment(2).Text;
         discard(1);
         discard(1);

         if (name.Length > 32)
         { throw new Exception("IDENTIFIER may not exceed 32 characters in length"); }
         else
            variables.Add(name, null);

         while (tokenValue(1) == TOKEN_COMMA) //if there is a COMMA initialize next variable
         {
            discard(1);
            name = tokenAssignment(1).Text;
            discard(1);
            if (name.Length > 32)
            { throw new Exception("IDENTIFIER may not exceed 32 characters in length"); }
            else
               variables.Add(name, null);
         }
      }

      private object getVariable(string name, object index)
      {
         if (name.IndexOf('.') > 0)
         {
            string[] s = name.Split(new char[] { '.' });
         }

         if (variables.ContainsKey(name))
         {
            if (index == null)
            { return variables[name]; }
            else
               return ((ArrayList)variables[name])[(int)index];
         }
         else throw new Exception("Undeclared identifier: " + name); //Hoffman required
      }

      public void addVariable(string name, object value)
      {
         if (variables.ContainsKey(name))
            throw new Exception("Variabled already declared: " + name);
         else
            variables.Add(name, value);
      }

      private void setVariable(String name, object val, object index)
      {
         if (name.IndexOf('.') > 0)
         {
            string[] s = name.Split(new char[] { '.' });
            return;
         }
         if (index == null)
         {
            if (hasVariable(name)) variables[name] = val;
            else addVariable(name, val);
         }
         else
            if (hasVariable(name)) ((ArrayList)variables[name])[(int)index] = val;
      }

      private bool hasVariable(String name)
      {
         return variables.ContainsKey(name);
      }

      void forAssignVar() //Variable assignment and initialization for FOR loops
      {
         string name = tokenAssignment(1).Text;
         discard(1);
         discard(1);
         Expression();
         object value = current;
         setVariable(name, value, null);
         discard(1);
      }
      #endregion
      #region printStatements
      void println()
      {
         if (tokenValue(2) == Constant.CONSTANT_STRNG || tokenValue(2) == TOKEN_IDENTIFIER)
         {
            discard(1);
            Expression();
            String temp = Convert.ToString(current);

            while (tokenValue(2) == TOKEN_COMMA)
            {
               input.saveLineBreak = true;
               discard(1);
               discard(1);
               Expression();
               temp += current;
               if (tokenValue(2) == Reserved.TOKEN_ENDLINE)
               {
                  discard(1);
                  discard(1);
                  input.saveLineBreak = false;
               }
            }
            if (temp.Length > 80)
            { throw new Exception("String values may not exceed 80 characters"); }
            if (streamOut == null) Console.WriteLine("{0}", temp);
            else streamOut.WriteLine("{0}", temp);
         }
         else //Else it is an empty string
         {
            discard(1);
            Console.WriteLine();
         }
      }

      void print()
      {
         if (tokenValue(2) == Constant.CONSTANT_STRNG || tokenValue(2) == TOKEN_IDENTIFIER)
         {
            discard(1);
            Expression();
            String temp = Convert.ToString(current);

            while (tokenValue(2) == TOKEN_COMMA)
            {
               input.saveLineBreak = true;
               discard(1);
               discard(1);
               Expression();
               temp += current;
               if (tokenValue(2) == Reserved.TOKEN_ENDLINE)
               {
                  discard(1);
                  discard(1);
                  input.saveLineBreak = false;
               }
            }
            if (temp.Length > 80)
            { throw new Exception("String values may not exceed 80 characters"); }
            if (streamOut == null) Console.Write("{0}", temp);
            else streamOut.Write("{0}", temp);
         }
      }
      #endregion
      #region functionStatements
      void createFX()
      {
         discard(1);//discard function
         string fname = tokenAssignment(1).Text;// cannot make it a variable, cant call it as an identifier e.g. doWork()
         discard(1); //discard method name
         discard(1); //discard Left Paran
         //addVariable(tokenAssignment(1).Text, null);
         String parameter = tokenAssignment(1).Text; //this is generic method parameter
         discard(1); //discard parameter
         discard(1); //discard Right Paran
         function += "Let " + fname + " = ";
         discard(1); //discard return token
         while (tokenValue(1) != Reserved.TOKEN_ENDLINE)
         {
            input.saveLineBreak = true;

            function += tokenAssignment(1).Text + " ";
            discard(1);
         }
         input.saveLineBreak = false;
         discard(1);//discard linebreak
         Method x = new Method(fname, function, parameter);
         function = ""; //reset function
         methodList.Add(x);
      }

      object executeFX(String name, String parameter) //execute desired function
      {
         object retval = null; //functions return val
         foreach (Method cur in methodList)
         {
            if (cur.fname == name) //locate correct method from list
            {
               String x = "Begin \rLet " + cur.parameter + " = " + parameter + " \r";
               cur.function = x + cur.function + " \rend";
               Lexer lexer = new Lexer(cur.function);
               Parser parser = new Parser(lexer);
               parser.variables = (Hashtable)this.variables.Clone();
               retval = parser.Evaluate();
            }
         }
         return retval;
      }
      #endregion

      void readIn()
      {
         String name = tokenAssignment(2).Text;
         discard(1);
         discard(1);
         Object value = Convert.ToInt32(Console.ReadLine());

         Console.Clear();
         setVariable(name, value, null);
      }

      void Expression() //Evaluate which type of expression is being parsed
      {
         SignedFactor();
         FirstOrderOp();
         SecondOrderOp();
         RelationalOp();
         BooleanOp();
      }
      void push(object o)
      {
         stk.Push(o);
      }

      object pop()
      {
         return stk.Pop();
      }

      private TokenBuffer input;
      public Hashtable variables; //hashtable for symbols
      private Stack stk;
      private object current;
      private List<Method> methodList = new List<Method>();
      private bool ifExit, beginOk;
      public bool exitOk;
      private StreamWriter streamOut;
      public int lineBreakcount = 0;
      public int lexerLineBreakCount = 0;
      String function;
   }


   public class Method
   {
      public Method(String fname, String function, String parameter)
      {
         this.fname = fname;
         this.function = function;
         this.parameter = parameter; //generic method parameter r 
      }

      public String fname;
      public String function;
      public String parameter;
   }
}