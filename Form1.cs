using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Explorer
{
    public partial class Form1 : Form
    {
        private List<string> paths = new List<string>();
        private string copy_flag = null;
        int edit_flag = 0;

        public Form1()
        {
            InitializeComponent();
            treeView1.Nodes[0].Name = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            treeView1.Nodes[0].Nodes[0].Name = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            treeView1.Nodes[0].Nodes[1].Name = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            treeView1.Nodes[0].Nodes[2].Name = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            treeView1.Nodes[0].Nodes[3].Name = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            treeView1.Nodes[0].Nodes[4].Name = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            int w = toolStrip1.Size.Width - toolStripTextBox2.Size.Width - 70;
            toolStripTextBox1.Size = new Size(w, toolStripTextBox1.Size.Height);
            this.treeView1.ExpandAll();
            this_pc(1);
        }

        private string drive_name(DriveInfo drive)
        {
            string name;
            if (drive.IsReady && drive.VolumeLabel != String.Empty)
                name = drive.VolumeLabel;
            else
            {
                if (drive.DriveType == DriveType.Fixed)
                    name = "Local Disk";
                else
                    name = drive.DriveType.ToString() + " Drive";
            }
            return name;
        }

        private void Enter_Item(object sender, MouseEventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                FileAttributes attr = File.GetAttributes(@item.Name);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    ProcessDirectory(@item.Name);
                else
                    System.Diagnostics.Process.Start(@item.Name);
            }
        }

        public void ProcessDirectory(string path)
        {
            if (path == "This PC")
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            if (path != null && path != String.Empty)
            {
                FileAttributes attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    toolStripTextBox1.Text = path;
                    this.Text = path;
                    string[] contents = Directory.GetDirectories(path).Concat(Directory.GetFiles(path)).ToArray();
                    this.listView1.Items.Clear();
                    this.listView1.Groups.Clear();
                    this.listView1.Columns.Clear();
                    nIndex = 0;
                    imageList1.Images.Clear();
                    foreach (string item in contents)
                        ProcessFile(item);
                }
                else
                    System.Diagnostics.Process.Start(path);
            }
            else
                this_pc();
        }

        public void ProcessFile(string path)
        {
            ListViewItem temp = new ListViewItem
            {
                Text = Path.GetFileName(path),
                Name = path
            };
            try
            {
                SHFILEINFO shinfo = new SHFILEINFO();
                IntPtr hImgSmall = Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
                Icon myIcon = Icon.FromHandle(shinfo.hIcon);

                imageList1.Images.Add(myIcon);
                temp.ImageIndex = nIndex++;
            }
            catch (Exception) { }
            this.listView1.Items.Add(temp);
        }

        private void backward(object sender, EventArgs e)
        {
            if (toolStripTextBox1.Text != "This PC")
            {
                paths.Add(toolStripTextBox1.Text);
                Path.GetDirectoryName(paths.Last());
                ProcessDirectory(Path.GetDirectoryName(paths.Last()));
            }
        }

        private void forward(object sender, EventArgs e)
        {
            if (paths.Count > 0)
            {
                ProcessDirectory(paths.Last());
                paths.Remove(paths.Last());
            }
        }

        private void this_pc(int check = 0)
        {
            toolStripTextBox1.Text = "This PC";
            this.Text = "This PC";
            this.listView1.Items.Clear();
            this.listView1.Groups.Clear();
            this.listView1.Columns.Clear();
            ListViewGroup local_drivers = new ListViewGroup
            {
                Header = "Devices & drivers",
                Name = "Devices_drivers"
            };
            ListViewGroup network_drivers = new ListViewGroup
            {
                Header = "Network locations",
                Name = "Network_locations"
            };
            ListViewGroup removable_drivers = new ListViewGroup
            {
                Header = "Removable Drive",
                Name = "Removable_drive"
            };
            this.listView1.Groups.AddRange(new ListViewGroup[] { local_drivers, network_drivers, removable_drivers });
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                ListViewGroup group;
                switch (d.DriveType)
                {
                    case DriveType.CDRom:
                        group = local_drivers;
                        break;

                    case DriveType.Fixed:
                        group = local_drivers;
                        TreeNode temp_node = new TreeNode
                        {
                            Text = drive_name(d),
                            Name = d.Name
                        };
                        if (check == 1) this.treeView1.Nodes[0].Nodes.Add(temp_node);
                        break;

                    case DriveType.Network:
                        group = network_drivers;
                        break;

                    case DriveType.Removable:
                        group = removable_drivers;
                        TreeNode temp_node1 = new TreeNode
                        {
                            Text = drive_name(d),
                            Name = d.Name
                        };
                        if (check == 1) this.treeView1.Nodes.Add(temp_node1);
                        break;

                    default:
                        group = local_drivers;
                        break;
                }
                ListViewItem temp = new ListViewItem
                {
                    Text = drive_name(d),
                    Group = group,
                    Name = d.Name
                };
                SHFILEINFO shinfo = new SHFILEINFO();
                IntPtr hImgSmall = Win32.SHGetFileInfo(temp.Name, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
                Icon myIcon = Icon.FromHandle(shinfo.hIcon);

                imageList1.Images.Add(myIcon);
                temp.ImageIndex = nIndex++;
                this.listView1.Items.Add(temp);
            }
        }

        private void Go_Path(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                string path = toolStripTextBox1.Text;
                if (File.Exists(path) || Directory.Exists(path))
                    ProcessDirectory(path);
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void Enter_Item(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                FileAttributes attr = File.GetAttributes(@item.Name);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    ProcessDirectory(@item.Name);
                else
                    System.Diagnostics.Process.Start(@item.Name);
            }
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listView1.SelectedItems.Count == 0)
            {
                contextMenuStrip2.Show(Cursor.Position);
            }
        }

        private void Copy(string file, string destination)
        {
            string FileName = Path.GetFileName(file);
            FileAttributes attr = File.GetAttributes(file);
            destination = Path.Combine(destination, FileName);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (Directory.Exists(destination))
                {
                    if (MessageBox.Show("Folder already exists !\nDo you want to continue copy folder?", "Folder already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Directory.CreateDirectory(destination);
                        string[] dirs = Directory.GetDirectories(file);
                        string[] fils = Directory.GetFiles(file);
                        foreach (string item in dirs)
                            Copy(item, destination);
                        foreach (string item in fils)
                            Copy(item, destination);
                    }
                }
                else
                {
                    Directory.CreateDirectory(destination);
                    string[] dirs = Directory.GetDirectories(file);
                    string[] fils = Directory.GetFiles(file);
                    foreach (string item in dirs)
                        Copy(item, destination);
                    foreach (string item in fils)
                        Copy(item, destination);
                }
            }
            else
            {
                if (File.Exists(destination))
                {
                    if (MessageBox.Show("File already exists !\nDo you want to continue copy file?", "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        File.Copy(file, destination, true);
                    }
                }
                else
                {
                    File.Copy(file, destination, true);
                }
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (copy_flag != null)
            {
                Copy(Clipboard.GetText(), toolStripTextBox1.Text);
                if (copy_flag == "cut")
                {
                    FileAttributes attr = File.GetAttributes(Clipboard.GetText());
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        Directory.Delete(Clipboard.GetText(), true);
                    else
                        File.Delete(Clipboard.GetText());
                }
                ProcessDirectory(toolStripTextBox1.Text);
                copy_flag = null;
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                Clipboard.SetText(item.Name);
            }
            copy_flag = "cut";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                Clipboard.SetText(item.Name);
            }
            copy_flag = "copy";
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                if (MessageBox.Show("Are you sure delete " + Path.GetFileName(item.Name), "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    FileAttributes attr = File.GetAttributes(item.Name);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        Directory.Delete(item.Name, true);
                    else
                        File.Delete(item.Name);
                }
            }
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessDirectory(toolStripTextBox1.Text);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
            {
                foreach (ListViewItem item in this.listView1.SelectedItems)
                {
                    Clipboard.SetText(item.Name);
                }
                copy_flag = "copy";
                return true;
            }
            else if (keyData == (Keys.Control | Keys.X))
            {
                foreach (ListViewItem item in this.listView1.SelectedItems)
                {
                    Clipboard.SetText(item.Name);
                }
                copy_flag = "cut";
                return true;
            }
            else if (keyData == (Keys.Control | Keys.V))
            {
                if (copy_flag != null)
                {
                    Copy(Clipboard.GetText(), toolStripTextBox1.Text);
                    if (copy_flag == "cut")
                    {
                        FileAttributes attr = File.GetAttributes(Clipboard.GetText());
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            Directory.Delete(Clipboard.GetText(), true);
                        else
                            File.Delete(Clipboard.GetText());
                    }
                    ProcessDirectory(toolStripTextBox1.Text);
                    copy_flag = null;
                }
                return true;
            }
            else if (keyData == (Keys.Shift | Keys.Delete))
            {
                foreach (ListViewItem item in this.listView1.SelectedItems)
                {
                    if (MessageBox.Show("Are you sure delete " + Path.GetFileName(item.Name), "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        FileAttributes attr = File.GetAttributes(item.Name);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            Directory.Delete(item.Name, true);
                        else
                            File.Delete(item.Name);
                    }
                }
                ProcessDirectory(toolStripTextBox1.Text);
                return true;
            }
            else if (keyData == (Keys.Back) && edit_flag == 0)
            {
                if (toolStripTextBox1.Text != "This PC")
                {
                    paths.Add(toolStripTextBox1.Text);
                    Path.GetDirectoryName(paths.Last());
                    ProcessDirectory(Path.GetDirectoryName(paths.Last()));
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ribbonButton7_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(toolStripTextBox1.Text + "\\New Folder");
            ProcessDirectory(toolStripTextBox1.Text);
            this.listView1.FindItemWithText("New Folder").BeginEdit();
            edit_flag = 1;
        }

        private void toolStripTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                string path = toolStripTextBox1.Text;
                string keyword = toolStripTextBox2.Text;
                if (path != "This PC" && keyword != String.Empty)
                {
                    this.listView1.Items.Clear();
                    this.listView1.Groups.Clear();
                    this.listView1.Columns.Clear();
                    search(path, keyword);
                }
            }
        }

        private void search(string path, string keyword)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] fils = Directory.GetFiles(path);
                foreach (string item in dirs)
                {
                    search(item, keyword);
                    if (item.Contains(keyword))
                    {
                        SHFILEINFO shinfo = new SHFILEINFO();
                        IntPtr hImgSmall = Win32.SHGetFileInfo(item, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
                        Icon myIcon = Icon.FromHandle(shinfo.hIcon);

                        imageList1.Images.Add(myIcon);
                        ListViewItem temp = new ListViewItem(Path.GetFileName(item));
                        temp.ImageIndex = nIndex++;
                        this.listView1.Items.Add(temp);
                    }
                }
                foreach (string item in fils)
                    if (item.Contains(keyword))
                    {
                        SHFILEINFO shinfo = new SHFILEINFO();
                        IntPtr hImgSmall = Win32.SHGetFileInfo(item, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
                        Icon myIcon = Icon.FromHandle(shinfo.hIcon);

                        imageList1.Images.Add(myIcon);
                        ListViewItem temp = new ListViewItem(Path.GetFileName(item));
                        temp.ImageIndex = nIndex++;
                        this.listView1.Items.Add(temp);
                    }
            }
            catch (Exception) { }
        }

        private void ribbonOrbMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Enter_Item_Tree(object sender, MouseEventArgs e)
        {
            ProcessDirectory(this.treeView1.SelectedNode.Name);
        }

        private void textDocuementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.CreateText(toolStripTextBox1.Text + "\\New Text Document.txt");
            ProcessDirectory(toolStripTextBox1.Text);
            /*this.listView1.FindItemWithText("New Text Document.txt").BeginEdit();
            edit_flag = 1;*/
        }

        private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            listView1.LargeImageList.ImageSize = new Size(60, 60);
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void mediumIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            listView1.LargeImageList.ImageSize = new Size(30, 30);
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
            listView1.LargeImageList.ImageSize = new Size(15, 15);
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void tilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile;
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void autoArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.AutoArrange == true)
                listView1.AutoArrange = false;
            else if (listView1.AutoArrange == false)
                listView1.AutoArrange = true;
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void ascToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Sorting = SortOrder.Ascending;
        }

        private void descToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Sorting = SortOrder.Descending;
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Sorting = SortOrder.None;
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            int w = toolStrip1.Size.Width - toolStripTextBox2.Size.Width - 70;
            toolStripTextBox1.Size = new Size(w, toolStripTextBox1.Size.Height);
            listView1.Size = new Size(this.Width - 178, this.Height - ribbon1.Height - 68);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].BeginEdit();
                edit_flag = 1;
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                string newf = Path.Combine(toolStripTextBox1.Text, e.Label);
                string oldf = Path.Combine(toolStripTextBox1.Text, listView1.SelectedItems[0].Text);
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
            }
            edit_flag = 0;
            ProcessDirectory(toolStripTextBox1.Text);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolStripTextBox1.Text != "This PC")
            {
                string path = Path.Combine(toolStripTextBox1.Text, listView1.SelectedItems[0].Text);
                Properties_D prop = new Properties_D(path);
                prop.Show();
            }
        }
    }

    public class NativeTreeView : TreeView
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        protected override void CreateHandle()
        {
            base.CreateHandle();
            SetWindowTheme(this.Handle, "explorer", null);
        }
    }

    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    internal class Win32
    {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
    }
}