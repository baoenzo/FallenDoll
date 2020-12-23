using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FallenDoll
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ofd.Filter = "UEXP|*.uexp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string pathUexp = ofd.FileName;
                BinaryReader rd = new BinaryReader(File.OpenRead(pathUexp));// rd file này // file uexp
                BinaryWriter wt = new BinaryWriter(File.Create(pathUexp + "_new"));// m sẽ tạo file mới uexp_new
                List<string> lstLines = File.ReadAllLines(Path.GetDirectoryName(ofd.FileName) + @"\dump.txt").ToList();
                rd.BaseStream.Seek(0x0, SeekOrigin.Begin);
                byte[] start = rd.ReadBytes(75);
                wt.Write(start);
                foreach (string item in lstLines)
                {

                    wt.Write(rd.ReadInt64());
                    long c = rd.ReadInt64();
                    string textTemp = item.Replace("[0]", "\0");
                    if (c!= 22)
                    {
                        wt.Write(rd.ReadInt64());
                        wt.Write(rd.ReadInt64());
                        wt.Write(rd.ReadInt64());
                    }
                    else
                    {
                        if (item != "null" && IsChinese(textTemp))
                        {
                            byte[] txt = Encoding.Unicode.GetBytes(textTemp);
                            wt.Write(txt.Length / -2);
                            wt.Write(txt);
                        }
                    }
                    break;

                }

                wt.Close();
                rd.Close();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ofd.Filter = "UEXP Unreal 4|*.uexp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string pathUE = @"C:\Users\Baoenzo\Desktop\1\TableTranslation.uexp";
                string pathUA = @"C:\Users\Baoenzo\Desktop\1\TableTranslation.uasset";
                List<string> temps = new List<string>();
                FileStream fs = new FileStream(pathUE, FileMode.Open);
                using (var reader = new BinaryReader(fs))
                {
                    reader.BaseStream.Seek(0x35, SeekOrigin.Begin);
                    for (int i = 0; i < 1353; i++)
                    {
                        reader.ReadInt64(); //unk
                        long t = reader.ReadInt64();
                        if (t != 22)
                        {
                            reader.ReadInt64();
                            reader.ReadInt64();
                            reader.ReadInt64();
                        }
                        else
                        {
                            reader.ReadInt64();
                        }
                        //5 bytes
                        reader.ReadByte();
                        int sizeText = reader.ReadInt32();
                        if (sizeText < 0)
                        {
                            sizeText = sizeText * -2;
                            byte[] data = reader.ReadBytes(sizeText);
                            string a = Encoding.Unicode.GetString(data).Replace("\0", "[0]");
                            if (a.Length != 0)
                                temps.Add(a);
                            else
                                temps.Add("null");
                        }
                        else
                        {
                            byte[] data = reader.ReadBytes(sizeText);
                            string a = Encoding.Default.GetString(data).Replace("\0", "[0]");
                            if (a.Length != 0)
                                temps.Add(a);
                            else
                                temps.Add("null");
                        }

                    }
                }

                try
                {
                    File.WriteAllLines(Path.GetDirectoryName(ofd.FileName) + @"/dump.txt", temps);
                    MessageBox.Show("Done!!", "Done!");
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public bool IsChinese(string text)
        {
            return text.Any(c => (uint)c >= 0x4E00 && (uint)c <= 0x2FA1F);
        }
    }
}
