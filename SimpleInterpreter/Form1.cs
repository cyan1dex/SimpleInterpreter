﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;


namespace SimpleInterpreter
{
   public partial class Form1 : Form
   {
      Parser parser = null;

      public Form1()
      {
         InitializeComponent();
         Win32.AllocConsole();
      }

      private void executeBtn_Click(object sender, EventArgs e)
      {
         try
         {
            Lexer lexer = new Lexer(txtInput.Text);
            parser = new Parser(lexer);
            Object obj = parser.Evaluate();

            Console.WriteLine("\nSuccessful run");
         }

         catch (Exception error)
         {
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(error, true);
            textBox1.Text = "LINE " + (parser.getCount + parser.lineBreakcount) + ":  " + error.Message.ToUpper();
         }
      }

      private void clrOutput_Click(object sender, EventArgs e)
      {
         txtInput.Text = "";
         textBox1.Text = "";
         Console.Clear();
      }

      private void openBtn_Click(object sender, EventArgs e)
      {
         OpenFileDialog openFileDialog1 = new OpenFileDialog();
         openFileDialog1.Filter = "Simple Interpreter Files|*.s";
         openFileDialog1.Title = "Select a Simple Interpreter File";

         if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            System.IO.StreamReader sr = new
               System.IO.StreamReader(openFileDialog1.FileName);
            txtInput.Text = sr.ReadToEnd();
            fileNameBox.Text = openFileDialog1.SafeFileName;

            sr.Close();
         }
      }
   }

   public class Win32
   {
      [DllImport("kernel32.dll")]
      public static extern Boolean AllocConsole();

      [DllImport("kernel32.dll")]
      public static extern Boolean FreeConsole();
   }
}
