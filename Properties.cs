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

namespace Windows_Explorer
{
    public partial class Properties_D : Form
    {
        private DirectoryInfo dir;

        public Properties_D(string path)
        {
            InitializeComponent();
            dir = new DirectoryInfo(path);
            textBox1.Text = dir.Name;
            this.Text = dir.Name + " Properties";
            label3.Text = dir.GetType().ToString();
            label5.Text = dir.FullName;
            long size1 = 0, size2 = 0;
            FileAttributes attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                size1 = size2 = DirSize(dir);
            else
                size1 = size2 = new FileInfo(path).Length;
            string fSize = size1 + " Bytes";
            if (size1 >= 1024)
            {
                size1 /= 1024;
                fSize = size1 + " Kilobyte (" + size2.ToString() + " Bytes)";
            }
            if (size1 >= 1024)
            {
                size1 /= 1024;
                fSize = size1 + " Megabyte (" + size2.ToString() + " Bytes)";
            }
            if (size1 >= 1024)
            {
                size1 /= 1024;
                fSize = size1 + " Gigabyte (" + size2.ToString() + " Bytes)";
            }
            if (size1 >= 1024)
            {
                size1 /= 1024;
                fSize = size1 + " Terabyte (" + size2.ToString() + " Bytes)";
            }
            label7.Text = fSize;
            attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                size1 = size2 = DirSize(dir);
            else
                size1 = size2 = new FileInfo(path).Length;
            fSize = size1 + " Bytes";
            if (size1 >= 1000)
            {
                size1 /= 1000;
                fSize = size1 + " Kilobyte (" + size2.ToString() + " Bytes)";
            }
            if (size1 >= 1000)
            {
                size1 /= 1000;
                fSize = size1 + " Megabyte (" + size2.ToString() + " Bytes)";
            }
            if (size1 >= 1000)
            {
                size1 /= 1000;
                fSize = size1 + " Gigabyte (" + size2.ToString() + " Bytes)";
            }
            if (size1 >= 1000)
            {
                size1 /= 1000;
                fSize = size1 + " Terabyte (" + size2.ToString() + " Bytes)";
            }
            label9.Text = fSize;
            label11.Text = dir.CreationTime.ToString();
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            try
            {
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }
            }
            catch (Exception) { }
            return size;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string newf = Path.Combine(Path.GetDirectoryName(dir.FullName), textBox1.Text);
            string oldf = dir.FullName;
            FileAttributes attr = File.GetAttributes(oldf);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (Directory.Exists(newf))
                {
                    if (MessageBox.Show("Folder already exists !\nDo you want to continue rename Folder?", "Folder already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        Directory.Move(oldf, newf);
                }
                else if (File.Exists(newf))
                    MessageBox.Show("Can't use this name");
                else
                    Directory.Move(oldf, newf);
            }
            else
            {
                if (File.Exists(newf))
                {
                    if (MessageBox.Show("File already exists !\nDo you want to continue rename file?", "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        File.Move(oldf, newf);
                }
                else if (Directory.Exists(newf))
                    MessageBox.Show("Can't use this name");
                else
                    File.Move(oldf, newf);
            }
            Close();
        }
    }
}