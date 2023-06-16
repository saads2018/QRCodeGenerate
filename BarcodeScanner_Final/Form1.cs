using BarcodeScanner_Final.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeScanner_Final
{
    public partial class Form1 : Form
    {
        Image img;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;


        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern Int32 ReleaseDC(IntPtr hwnd);

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadPrinters();
            img = pictureBox2.Image;
            LoadTable();
        }

        private void LoadPrinters()
        {
            try
            {
                this.cbxInvPrinter.Items.Add("None");
                this.cbxkitchenprinter.Items.Add("None");
                foreach (String printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {

                    this.cbxInvPrinter.Items.Add(printer);

                    this.cbxkitchenprinter.Items.Add(printer);
                }

                string json = File.ReadAllText("printer.json");
                var printer1 = JsonConvert.DeserializeObject<Printer>(json);

                if (printer1 == null)
                {
                    Printer printer2 = new Printer();
                    printer2.PrinterName = this.cbxInvPrinter.Items[0].ToString();
                    printer2.Width = 450;
                    printer2.Height = 300;
                    string Json = JsonConvert.SerializeObject(printer2, Formatting.Indented);
                    File.WriteAllText("printer.json", Json);

                    this.cbxInvPrinter.SelectedItem = printer2.PrinterName;
                    textBox3.Text = printer2.Width.ToString();
                    textBox4.Text = printer2.Height.ToString();
                }
                else
                {
                    this.cbxInvPrinter.SelectedItem = printer1.PrinterName;
                    textBox3.Text = printer1.Width.ToString();
                    textBox4.Text = printer1.Height.ToString();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "FrmConfiguration LoadPrinters", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadTable()
        {
            dataGridView1.Rows.Clear();
            List<Barcodes> barcodesList = JsonConvert.DeserializeObject<List<Barcodes>>(File.ReadAllText("barcodes.json"));
            if (barcodesList != null)
            {
                int i = 0;
                foreach (Barcodes barcodes in barcodesList)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = barcodes.Date;
                    dataGridView1.Rows[i].Cells[1].Value = barcodes.Barcode;
                    dataGridView1.Rows[i].Cells[2].Value = barcodes.QRCode;
                    i += 1;
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(dataGridView1.CurrentRow.Cells[2].Value.ToString()))
                {
                    byte[] bytes = Convert.FromBase64String(dataGridView1.CurrentRow.Cells[2].Value.ToString());
                    textBox1.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    Image image;
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        image = Image.FromStream(ms);
                    }
                    pictureBox2.Image = image;
                }

            }
            catch
            {
                MessageBox.Show("Please select an appropriate cell!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                generateQRCode();
                PrintQRCode();
                textBox1.Text = "";
                pictureBox2.Image = img;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            File.WriteAllText("barcodes.json", "");
            LoadTable();
        }

        private void generateQRCode()
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Please enter a barcode!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            pictureBox2.Image = qrcode.Draw(textBox1.Text, 100);
            string base64String = ConvertImage();

            Barcodes barcodes = new Barcodes();
            barcodes.Barcode = textBox1.Text;
            barcodes.QRCode = base64String;
            barcodes.Date = DateTime.Now.ToString();

            List<Barcodes> barcodesList = JsonConvert.DeserializeObject<List<Barcodes>>(File.ReadAllText("barcodes.json"));
            if (barcodesList == null)
            {
                barcodesList = new List<Barcodes>();
            }

            barcodesList.Add(barcodes);
            string json = JsonConvert.SerializeObject(barcodesList, Formatting.Indented);

            File.WriteAllText("barcodes.json", json);
            using (StreamWriter streamWriter = File.AppendText("logs.txt"))
            {
                streamWriter.Write("\n# " + DateTime.Now.ToString() + "   " + textBox1.Text);
            }
            LoadTable();
        }

        public string ConvertImage()
        {
            Bitmap bitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.DrawToBitmap(bitmap, new Rectangle(0, 0, pictureBox2.Width, pictureBox2.Height));
            MemoryStream objStream = new MemoryStream();
            bitmap.Save(objStream, ImageFormat.Jpeg);
            return Convert.ToBase64String(objStream.ToArray());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            generateQRCode();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            pictureBox2.Image = img;
        }

        private void PrintQRCode()
        {

            if (cbxInvPrinter.Text.Trim().Length != 0 && cbxInvPrinter.SelectedIndex != 0 && pictureBox2.Image != img)
            {

                Printer printer = new Printer();
                printer.PrinterName = this.cbxInvPrinter.Text;
                printer.Width = Int32.Parse(this.textBox3.Text);
                printer.Height = Int32.Parse(this.textBox4.Text);

                string json = JsonConvert.SerializeObject(printer, Formatting.Indented);
                File.WriteAllText("printer.json", json);

                PrintDocument print = new PrintDocument();
                try
                {
                    print.PrinterSettings.Copies = (short)Int32.Parse(textBox2.Text);
                    print.PrinterSettings.PrinterName = cbxInvPrinter.Text;
                    print.PrintPage += Print_PrintPage; ;
                    print.Print();
                }
                catch
                {
                    MessageBox.Show("Please enter the appropriate details", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else if (pictureBox2.Image == img)
                MessageBox.Show("Please generate a QR Code", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                MessageBox.Show("Please select a Printer", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void Print_PrintPage(object sender, PrintPageEventArgs e)
        {
            Bitmap bitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.DrawToBitmap(bitmap, new Rectangle(0, 0, pictureBox2.Width, pictureBox2.Height));

            /* Rectangle m = e.MarginBounds;

             if ((double)pictureBox2.Width / (double)pictureBox2.Height > (double)m.Width / (double)m.Height) // image is wider
             {
                 m.Height = (int)((double)pictureBox2.Height / (double)pictureBox2.Width * (double)m.Width);
             }
             else
             {
                 m.Width = (int)((double)pictureBox2.Width / (double)pictureBox2.Height * (double)m.Height);
             }*/


            Single xDpi, yDpi;

            IntPtr dc = GetDC(IntPtr.Zero);

            using (Graphics g = Graphics.FromHdc(dc))
            {
                xDpi = g.DpiX;
                yDpi = g.DpiY;
            }

            e.Graphics.DrawImage(bitmap, 10, 10, Int32.Parse(textBox3.Text), Int32.Parse(textBox4.Text));
            bitmap.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrintQRCode();
            textBox1.Text = "";
            pictureBox2.Image = img;
        }
    }
}
