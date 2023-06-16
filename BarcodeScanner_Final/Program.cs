using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeScanner_Final
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists("barcodes.json"))
            {
                File.AppendAllText("barcodes.json", "");
            }
            if (!File.Exists("logs.txt"))
            {
                File.AppendAllText("logs.txt", "-----Date/Time---------Barcode------");
            }
            if (!File.Exists("printer.json"))
            {
                File.AppendAllText("printer.json", "");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
