using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lecturer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        long size;

        readonly string[] sizes = { "B", "KB", "MB", "GB", "TB" };

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            double Speed;
            if (!Double.TryParse(textBox2.Text.Replace(".", ","), out Speed) || Speed<0) { MessageBox.Show("Введите положительное число!\nМожно дробное.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (string.IsNullOrEmpty(textBox4.Text)) { MessageBox.Show("Введите команду, по которой будет срабатывать скрипт!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            List<string> list = new List<string>();
            list.Add(":?:" + textBox4.Text + "::");
            list.Add("Sleep, 1000");
            for (int i = 0; i<textBox1.Lines.Length; i++)
            {
                
                double Lenght = Double.Parse(textBox1.Lines[i].Replace(" ", "").Replace(".", "").Replace(",", "").Replace("!", "").Replace("?", "").Replace(";", "").Replace(":", "").Length.ToString());
                double Time = default(double);
                Time = Lenght / Speed * 1000;
                if (Time < 1000) { Time = 1200; }
                
                list.Add("SendInput {f6}" + textBox1.Lines[i].Replace("!", "{!}") + "{Enter}");
                if (i == textBox1.Lines.Length - 1) { list.Add("Sleep, " + 1000); }
                else { list.Add("Sleep, " + Math.Round(Time)); }
            }
            list.Add("Return");
            textBox3.Lines = list.ToArray(); 
         
        }
        private bool CheckForUpdates()
        {
            try
            {
                HttpWebResponse res = (HttpWebResponse)HttpWebRequest.Create("http://sumjest.ru/programsinfo/programs.txt").GetResponse();
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new StreamReader(res.GetResponseStream(), encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] linea = line.Split(';');

                        if (line.Split(';')[0].Contains("Lecturer"))
                        {
                            Version v;
                            if (Version.TryParse(line.Split(';')[1], out v)) { if (v.CompareTo(Version.Parse(Application.ProductVersion)) > 0) { return true; } }
                            
                        }
                        
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(CheckForUpdates()) menuStrip1.Items.Add("Вышла новая версия программы!", null, onNewClick);
        }
        private string getNumFileName(string filename, int number)
        {
            return Path.GetFileNameWithoutExtension(filename) + " (" + number + ")" + Path.GetExtension(filename);
        }
        private void onNewClick(object sender, EventArgs args)
        {
            try
            {
                HttpWebResponse res = (HttpWebResponse)HttpWebRequest.Create("http://sumjest.ru/programsinfo/lecturerchanges.txt").GetResponse();
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new StreamReader(res.GetResponseStream(), encoding))
                {

                    DialogResult dr = MessageBox.Show(reader.ReadToEnd(), "Загрузить новую версию?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dr == DialogResult.Yes)
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            var data = client.DownloadData("http://sumjest.ru/files/Lecturer.zip");
                            string[] fileNameA = "http://sumjest.ru/files/Lecturer.zip".Split('/');
                            string fileName = fileNameA[fileNameA.Length - 1];
        
                            SaveFileDialog sfd = new SaveFileDialog();
                            string extension = Path.GetExtension(fileName);

                            sfd.Filter = "Zip-archive | *" + extension;
                            sfd.DefaultExt = extension.Replace(".", "");
                            sfd.InitialDirectory = Application.StartupPath;
                            if (File.Exists(Application.StartupPath + @"\" + fileName))
                            {
                                for (int i = 1; i < int.MaxValue; i++)
                                {
                                    if (File.Exists(Application.StartupPath + @"\" + getNumFileName(fileName, i)))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        sfd.FileName = getNumFileName(fileName, i);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                sfd.FileName = fileName;
                            }


                            DialogResult result = sfd.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                download("http://sumjest.ru/files/Lecturer.zip", sfd.FileName);
                                System.Diagnostics.Process.Start("explorer.exe", /*Path.GetDirectoryName(sfd.FileName) + */"/select, " + sfd.FileName);
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + " " + "http://sumjest.ru/files/Lecturer.zip", ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
        
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            label4.Text = getSize(size * args.ProgressPercentage / 100) + " / " + getSize(size);
        }
        private void download(string url, string path)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)HttpWebRequest.Create(url).GetResponse();
                size = response.ContentLength;
                response.Close();

                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                client.DownloadFileAsync(new Uri(url), path);

                label4.Text = "0 / " + getSize(size);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + url, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Completed(object sender, AsyncCompletedEventArgs args)
        {
            size = 0;
            label4.Text = "";
            MessageBox.Show("Файл скачан!", "Готово!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private string getSize(long sizeoffile)
        {
            double len = double.Parse(sizeoffile.ToString());

            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }


            return String.Format("{0:0.##} {1}", Math.Round(len, 2), sizes[order]);
        }

        private void informationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }
    }
}
