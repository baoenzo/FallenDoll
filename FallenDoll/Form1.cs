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
                byte[] start = rd.ReadBytes(53); 
                wt.Write(start);
                foreach (string item in lstLines)
                {
                    long unk2 = rd.ReadInt64();
                    wt.Write(unk2);
                    long c = rd.ReadInt64();
                    string textTemp = "";
                    if (item != "null")
                    {
                        textTemp = item.Replace("[0]", "\0");
                    }
                    if (c != 22) // Text rỗng
                    {
                        wt.Write(rd.ReadInt64());
                        wt.Write(c);
                        wt.Write(rd.ReadInt64());
                        long d = rd.ReadInt64();
                        wt.Write(d);
                        wt.Write(rd.ReadBytes((int)d + 1));
                    }
                    else
                    {
                        long dem = rd.ReadInt64();
                        byte[] txt = Encoding.Unicode.GetBytes(textTemp);
                        wt.Write(c);
                        wt.Write(WriteInt64BE((long)(txt.Length + 4)));
                        wt.Write(false);
                        wt.Write(txt.Length / -2);
                        wt.Write(txt);
                        long next = rd.BaseStream.Position + dem + 1;
                        rd.BaseStream.Seek(next, SeekOrigin.Begin);
                    }
                }
                wt.Write(WriteInt64BE(17));
                byte[] b =  { 0xC1, 0x83, 0x2A, 0x9E };
                wt.Write(b);
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public byte[] WriteInt64BE(long a)
        {
            byte[] buffer = BitConverter.GetBytes(a);
            return buffer;
        }
    }
}
