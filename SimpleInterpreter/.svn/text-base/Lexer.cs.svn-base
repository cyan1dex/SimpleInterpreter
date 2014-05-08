using System;
using System.Text;
using System.IO;

namespace SimpleInterpreter
{
   public class Lexer : Symbol
   {
      public Lexer(StreamReader f)
      {
         input = new CharBuffer(f, 1);
      }

      public Lexer(string s)
      {
         input = new CharBuffer(s, 1);
      }

      private int curChar(int i) { return input.curChar(i); }
      private void consume() { input.consume(); }
      private void match(int c) { consume(); }
      public int getPos() { return input.getPos(); }
      public void setPos(int i) { input.setPos(i); }

      public Token getToken()
      {
         Token retval;
         for (; ; )
         {
            retval = null;

            if (curChar(1) == '#') //Consume line comments
            {
               while (curChar(1) != '\r')
               {
                  consume();
               }
            }
            else if (curChar(1) == ' ' || curChar(1) == '\t' || curChar(1) == '\n')
            {
               consume(); //Consume whitespace
            }
            else if (curChar(1) == '\r')
            {
               if (saveLineBreak == false)
               { consume(); lineBreakCount++; }
               else
               { consume(); retval = new Token(TOKEN_ENDLINE, "\r"); }
            }

            if (curChar(1) > 126)
               throw new Exception("Char out of acceptable ASCII range");
            else if (char.IsLetter((char)curChar(1)))
            { retval = identifier(); } //char is identifier
            else if (char.IsNumber((char)curChar(1)))
            { retval = number(); } //Char is a number 
            else if (curChar(1) == '"') //else char is a special symbol below
            { retval = tstring(); }
            else if (curChar(1) == '!')
            { retval = not(); }
            else if (curChar(1) == '>')
            { retval = greater(); }
            else if (curChar(1) == '<')
            { retval = less(); }
            else if (curChar(1) == '=')
            { consume(); retval = new Token(Special.TOKEN_EQUALS, "="); }
            else if (curChar(1) == '+')
            { consume(); retval = new Token(Special.TOKEN_PLUS, "+"); }
            else if (curChar(1) == '-')
            { consume(); retval = new Token(Special.TOKEN_MINUS, "-"); }
            else if (curChar(1) == '*')
            { consume(); retval = new Token(Special.TOKEN_MULT, "*"); }
            else if (curChar(1) == '$')
            { consume(); retval = new Token(Special.TOKEN_SQRT, "$"); }
            else if (curChar(1) == '%')
            { consume(); retval = new Token(Special.TOKEN_MOD, "%"); }
            else if (curChar(1) == '^')
            { consume(); retval = new Token(Special.TOKEN_POW, "^"); }
            else if (curChar(1) == '/')
            { consume(); retval = new Token(Special.TOKEN_DIV, "/"); }
            else if (curChar(1) == '[')
            { consume(); retval = new Token(TOKEN_LBRACK, "["); }
            else if (curChar(1) == ']')
            { consume(); retval = new Token(TOKEN_RBRACK, "]"); }
            else if (curChar(1) == '(')
            { consume(); retval = new Token(TOKEN_LPAREN, "("); }
            else if (curChar(1) == ')')
            { consume(); retval = new Token(TOKEN_RPAREN, ")"); }
            else if (curChar(1) == ',')
            { consume(); retval = new Token(TOKEN_COMMA, ","); }
            else if (curChar(1) == -1)
            { consume(); retval = new Token(TOKEN_EOF, "<eof>"); }
            else consume();
            if (retval != null) return retval;
         }
      }

      Token identifier() //if char is a letter build string from the chars & match its identifier
      {
         StringBuilder s = new StringBuilder();
         while (char.IsLetter((char)curChar(1)) || char.IsNumber((char)curChar(1))
                                   || (char)curChar(1) == '_'
                                   || (char)curChar(1) == '.')
         {
            s.Append((char)curChar(1)); //if char is a letter append to a string
            consume();             //than consume from input
         }
         string identity = s.ToString(); //convert to string

         string idUpper = identity.ToUpper(); //convert to uppercase
         if (idUpper.Equals("TRUE") || idUpper.Equals("FALSE"))
            return new Token(Constant.CONSTANT_BOOL, identity);
         else if (idUpper.Equals("AND"))
            return new Token(Special.TOKEN_AND, "AND");
         else if (idUpper.Equals("OR"))
            return new Token(Special.TOKEN_OR, "OR");
         else if (idUpper.Equals("IF"))
            return new Token(Reserved.TOKEN_IF, "IF");
         else if (idUpper.Equals("THEN"))
            return new Token(Reserved.TOKEN_THEN, "THEN");
         else if (idUpper.Equals("WHILE"))
            return new Token(Reserved.TOKEN_WHILE, "WHILE");
         else if (idUpper.Equals("ENDWHILE"))
            return new Token(Reserved.TOKEN_EWHILE, "ENDWHILE");
         else if (idUpper.Equals("RETURN"))
            return new Token(Reserved.TOKEN_EDEFFUNC, "RETURN"); //return for function
         else if (idUpper.Equals("DECLARE"))
            return new Token(Reserved.TOKEN_DEFARRAY, "DECLARE"); //Declare array
         else if (idUpper.Equals("FUNCTION"))
            return new Token(Reserved.TOKEN_DEFFUNC, "FUNCTION");
         else if (idUpper.Equals("LET"))
            return new Token(Reserved.TOKEN_LET, "LET");
         else if (idUpper.Equals("TO"))
            return new Token(Reserved.TOKEN_TO, "TO");
         else if (idUpper.Equals("FOR"))
            return new Token(Reserved.TOKEN_FOR, "FOR"); //For loop, For i To X; next
         else if (idUpper.Equals("NEXT"))
            return new Token(Reserved.TOKEN_NEXT, "NEXT");
         else if (idUpper.Equals("EXIT"))
            return new Token(Reserved.TOKEN_EXIT, "EXIT");
         else if (idUpper.Equals("PRINT"))
            return new Token(Reserved.TOKEN_PRINT, "PRINT");
         else if (idUpper.Equals("BEGIN"))
            return new Token(TOKEN_BEGIN, "BEGIN");
         else if (idUpper.Equals("END"))
            return new Token(TOKEN_END, "END");
         else if (idUpper.Equals("PRINTLN"))
            return new Token(Reserved.TOKEN_PRINTLN, "PRINTLN");
         else if (idUpper.Equals("INTEGER"))
            return new Token(Primitive.TOKEN_INTEGER, "INTEGER");
         else if (idUpper.Equals("DOUBLE"))
            return new Token(Primitive.TOKEN_DOUBLE, "DOUBLE");
         else if (idUpper.Equals("BOOLEAN"))
            return new Token(Primitive.TOKEN_BOOLEAN, "BOOLEAN");
         else if (idUpper.Equals("STRING"))
            return new Token(Primitive.TOKEN_STRING, "STRING");
         else if (idUpper.Equals("READ"))
            return new Token(Reserved.TOKEN_READ, "READ");

         return new Token(TOKEN_IDENTIFIER, identity);
      }

      Token tstring()
      {
         StringBuilder s = new StringBuilder();
         consume();
         char c = (char)curChar(1);
         while (curChar(1) != -1 && c != '"')
         {
            s.Append(c);
            consume();
            c = (char)curChar(1);
         }

         consume();
         return new Token(Constant.CONSTANT_STRNG, s.ToString());
      }

      Token not()
      {
         StringBuilder s = new StringBuilder();
         char c = (char)curChar(1);
         s.Append(c);
         consume();
         c = (char)curChar(1);
         if (c == '=') // !=
         {
            s.Append(c);
            consume();
            return new Token(Special.TOKEN_NEQUALS, s.ToString()); //!=
         }
         else
            return new Token(Special.TOKEN_NOT, s.ToString()); // '!'
      }

      Token greater()
      {
         StringBuilder s = new StringBuilder();
         char c = (char)curChar(1);
         s.Append(c);
         consume();
         c = (char)curChar(1);
         if (c == '=') // >=
         {
            s.Append(c);
            consume();
            return new Token(Special.TOKEN_GREATEREQ, s.ToString()); //>=
         }
         else
            return new Token(Special.TOKEN_GREATER, s.ToString());  // '>'
      }

      Token less()
      {
         StringBuilder s = new StringBuilder();
         char c = (char)curChar(1);
         s.Append(c);
         consume();
         c = (char)curChar(1);
         if (c == '=')// <=
         {
            s.Append(c);
            consume();
            return new Token(Special.TOKEN_LESSEQ, s.ToString()); // <=
         }
         else
            return new Token(Special.TOKEN_LESS, s.ToString()); // '<'
      }

      Token number()
      {
         StringBuilder s = new StringBuilder();
         string stemp;
         char c = (char)curChar(1);
         while (char.IsNumber(c) || c == '.')
         {
            s.Append(c);
            consume();
            c = (char)curChar(1);
         }
         stemp = s.ToString();

         if (stemp.IndexOf('.') > 0) //if there is a decimal than the constant is a double
            return new Token(Constant.CONSTANT_DBL, stemp);
         else
            return new Token(Constant.CONSTANT_INT, stemp);
      }

      Token comment()
      {
         StringBuilder s = new StringBuilder();
         match('#');
         while (curChar(1) != '\n')
         {
            s.Append((char)curChar(1));
            consume();
         }
         return new Token(TOKEN_COMMENT, s.ToString());
      }

      private CharBuffer input;
      public bool saveLineBreak;
      public int lineBreakCount = 0;
   }

   public class Token
   {
      private string text;
      private int type;

      public Token(int type, string text)
      {
         this.type = type;
         this.text = text;
      }
      public string Text
      {
         get { return (text); }
         set { text = value; }
      }
      public int Type
      {
         get { return (type); }
         set { type = value; }
      }
   }
}