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

namespace findrefile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            string path = folderBrowserDialog1.SelectedPath;
            dataGridView1.Visible = false;
            listBox1.Visible = true;
            listBox1.Items.Clear();
            AccessHelper.addHashFile(FindFile(path));

        }

        public List<HashFile> FindFile(string dirPath) //参数dirPath为指定的目录

        {
            List<HashFile> listHashFile = new List<HashFile>();
            DirectoryInfo Dir=null;
            DirectoryInfo[] di=null;
            try
            {
                Dir = new DirectoryInfo(dirPath);
                di = Dir.GetDirectories();
            }
            catch (Exception ex) { }
            if (di != null)
            {
                if (di.Length > 0 && Dir.GetFiles().Length == 0)
                //      for(int i=0;i< Directory.GetFiles(dirPath).Length; i++) { 
                // foreach(string fi in Directory.GetFiles(dirPath)) { 

                {

                    try

                    {

                        foreach (DirectoryInfo d in Dir.GetDirectories())//查找子目录

                        {
                            //文件夾就繼續遞歸
                            //FindFile(fi + "\\");
                            FindFile(Dir + "\\" + d.ToString() + "\\");

                            //文件就添加到listbox


                            //listBox1.Items.Add(Dir + "\\" + d.ToString() + "\\"); //listBox1中填加目录名



                        }
                    }
                    catch (Exception e)

                    {

                        // MessageBox.Show(e.Message);


                    }
                }
                else if (di.Length == 0 && Dir.GetFiles().Length > 0)
                {
                    //文件             
                    foreach (FileInfo fi in Dir.GetFiles())
                    {
                        System.Security.Cryptography.HashAlgorithm hash = System.Security.Cryptography.HashAlgorithm.Create();
                        FileStream fs = new System.IO.FileStream(fi.FullName, FileMode.Open);
                        string filehash = BitConverter.ToString(hash.ComputeHash(fs));
                        HashFile hashfile = new HashFile();
                        hashfile.Hashcode = filehash;
                        hashfile.Path = fi.FullName;
                        listHashFile.Add(hashfile);
                        //AccessHelper.addHashFile(filehash, fi.FullName);
                        listBox1.Items.Add(fi.FullName + "&&" + filehash); //listBox1中填加文件名 
                                                                           // li.Add(fi.FullName);
                        fs.Close();

                    }

                    // listBox1.Items.Add(dirPath); //listBox1中填加文件名 

                }
                else if (Dir.GetFiles().Length > 0 && di.Length > 0)
                {
                    //既有文件夾，又有文件
                    foreach (DirectoryInfo d in di)
                    {
                        FindFile(d.FullName + "\\");
                    }
                    foreach (FileInfo fi in Dir.GetFiles())
                    {

                        System.Security.Cryptography.HashAlgorithm hash = System.Security.Cryptography.HashAlgorithm.Create();
                        FileStream fs = new System.IO.FileStream(fi.FullName, FileMode.Open);
                        string filehash = BitConverter.ToString(hash.ComputeHash(fs));
                        HashFile hashfile = new HashFile();
                        hashfile.Hashcode = filehash;
                        hashfile.Path = fi.FullName;
                        listHashFile.Add(hashfile);

                        //AccessHelper.addHashFile(filehash, fi.FullName);
                        listBox1.Items.Add(fi.FullName + "&&" + filehash);
                        fs.Close();

                    }
                }
            }
            return listHashFile;
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Visible = false;
            dataGridView1.Visible = true;
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = AccessHelper.findrefile();
          
           

        }

    
        private void button3_Click(object sender, EventArgs e)
        {
            int count = 0;
            //List<string> pathlist = new List<string>();
            for (int i = 0; i < this.dataGridView1.Rows.Count-1; i++) {//datagridView1总是会多出一行空内容的，没调试清楚

                if ((bool) this.dataGridView1.Rows[i].Cells["是否删除"].Value) {
                    //pathlist.Add(this.dataGridView1.Rows[i].Cells["path"].Value.ToString());
                    //根据路径删除文件
                    string path = this.dataGridView1.Rows[i].Cells["path"].Value.ToString();
                     File.Delete(path);

                    //删除成功后，清除记录
                  count += AccessHelper.delFile(path);

                }
            }
            if (count > 0) { MessageBox.Show("成功删掉" + count + "个文件"); }
            //重新显示列表
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = AccessHelper.findAllfile();


        }

        private void button4_Click(object sender, EventArgs e)
        {
          int i= AccessHelper.delAllFile();
            MessageBox.Show("清除" + i + "条记录！");
        }
    }
}


