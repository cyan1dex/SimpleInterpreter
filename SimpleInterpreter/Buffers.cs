using System;
using System.Text;
using System.IO;

namespace SimpleInterpreter
{
   public class CharBuffer
   {
      public CharBuffer(StreamReader sr, int depth)
      {
         buf = new int[depth];	//buffer to hold tokens
         Code = sr.ReadToEnd();
         this.depth = (depth > Code.Length ? Code.Length : depth);  //Is the depth greater than the length of what is left to be parsed
         setPos(0);
      }

      public CharBuffer(string s, int depth) //Depth is passed in which a length of 2 initially
      {
         buf = new int[depth];	//buffer to hold tokens
         Code = s;
         this.depth = (depth > Code.Length ? Code.Length : depth);
         setPos(0);
      }

      public int getPos()
      {
         return (ipos - 2); // first symbol in buffer accordance ipos==2 
      }
      public void setPos(int ind)
      {
         this.ipos = ind;
         buf[depth - 1] = Code[ipos++];
      }

      public int curChar(int i)
      {
         if (i >= 1 && i <= depth)
         {
            return buf[i - 1];
         }
         return 0;
      }

      public void consume()
      {
         for (int i = 0; i < depth - 1; i++)
         {
            buf[i] = buf[i + 1];  // oldvalue=newvalue
         }
         try
         {
            if (ipos == Code.Length) // set new value
               buf[depth - 1] = -1;	// read next symbol frm buffer, end buffers
            else
               buf[depth - 1] = Code[ipos++];
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
         }
      }

      private int[] buf;
      private int depth;
      private string Code;
      private int ipos;  // point to position in string buffer
   }

   public class TokenBuffer
   {
      public TokenBuffer(Lexer lex, int k)
      {
         this.k = k;
         buf = new Token[k];
         lexer = lex;
         try
         {
            for (int i = 0; i < k; i++)
            {
               buf[i] = lexer.getToken();
            }
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
         }
      }
      public void setPos(int i)
      {
         lexer.setPos(i);
      }
      public int getPos() { return lexer.getPos(); }

      public Token tokenValue(int i)
      {
         if (i >= 1 && i <= k)
         {
            return buf[i - 1];
         }
         return null;
      }

      public void consume(int val)
      {
         if (saveLineBreak == true)
            lexer.saveLineBreak = true;
         else
            lexer.saveLineBreak = false;

         for (int i = val - 1; i < k - 1; i++)
         {
            buf[i] = buf[i + 1];
         }
         try
         {
            buf[k - 1] = lexer.getToken();
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
         }
      }

      public int getCount
      {
         get { return lexer.lineBreakCount; }
      }

      private Token[] buf;
      private Lexer lexer;
      private int k;
      public bool saveLineBreak;
      public int lineBreakCount = 0;
   }
}
