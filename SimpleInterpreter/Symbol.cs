using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleInterpreter
{
   public class Symbol
   {
      public const int TOKEN_IDENTIFIER = 1; //Variable identifier
      public const int TOKEN_BEGIN = 2;
      public const int TOKEN_END = 3;
      public const int TOKEN_COMMENT = 4;
      public const int TOKEN_LBRACK = 5;
      public const int TOKEN_RBRACK = 6;
      public const int TOKEN_LPAREN = 7;
      public const int TOKEN_RPAREN = 8;
      public const int TOKEN_COMMA = 9;
      public const int TOKEN_ENDLINE = 10;
      public const int TOKEN_EOF = 999;
   }

   public class Constant : Symbol  //Constant identifier
   {
      public const int CONSTANT_INT = 11;
      public const int CONSTANT_DBL = 12;
      public const int CONSTANT_STRNG = 13;
      public const int CONSTANT_BOOL = 14;
   }

   public class Primitive : Symbol //Primitive indentifier
   {
      public const int TOKEN_INTEGER = 15;
      public const int TOKEN_DOUBLE = 16;
      public const int TOKEN_STRING = 17;
      public const int TOKEN_BOOLEAN = 18;
   }

   public class Special : Symbol
   {
      public const int TOKEN_PLUS = 19;
      public const int TOKEN_MINUS = 20;
      public const int TOKEN_MULT = 21;
      public const int TOKEN_DIV = 22;
      public const int TOKEN_EQUALS = 23;
      public const int TOKEN_NEQUALS = 24;
      public const int TOKEN_GREATER = 25;
      public const int TOKEN_GREATEREQ = 26;
      public const int TOKEN_LESS = 27;
      public const int TOKEN_LESSEQ = 28;
      public const int TOKEN_NOT = 29;
      public const int TOKEN_AND = 30;
      public const int TOKEN_OR = 31;
      public const int TOKEN_MOD = 32;
      public const int TOKEN_SQRT = 33;
      public const int TOKEN_POW = 34;
   }

   public class Reserved : Symbol
   {
      //CONDITIONALS
      public const int TOKEN_ELSE = 35;
      public const int TOKEN_ENDIF = 36;
      public const int TOKEN_WHILE = 37;
      public const int TOKEN_EWHILE = 38;
      public const int TOKEN_IF = 39;
      public const int TOKEN_THEN = 40;
      //FUNCTIONS
      public const int TOKEN_DEFFUNC = 40;
      public const int TOKEN_EDEFFUNC = 41;
      public const int TOKEN_DEFARRAY = 42;
      public const int TOKEN_EXIT = 43;
      public const int TOKEN_FOR = 44;
      public const int TOKEN_NEXT = 45;
      public const int TOKEN_READ = 46;
      public const int TOKEN_PRINTLN = 47;
      public const int TOKEN_PRINT = 48;
      public const int TOKEN_LET = 49;
      public const int TOKEN_DEVICE = 50;
      public const int TOKEN_TO = 51;
      public const int TOKEN_SELECT = 52;
   }
}
