using System;
using System.Windows.Forms;
using JpgVerifyTool.Forms;

namespace JpgVerifyTool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}