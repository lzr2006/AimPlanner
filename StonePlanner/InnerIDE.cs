using MDI;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StonePlanner
{
    public partial class InnerIDE : Form
    {
        #region 初始化
        public InnerIDE()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            textBox_Pars.Visible = false;
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool ReleaseCapture();
        protected string fileNameString = "",fName = "",iFileName = "";
        internal ListBox lb;
        protected Boolean stOpenRem = false;
        internal List<string> nInput;
        protected int EPH;
        protected Main main;
        private void panel_Top_MouseDown(object sender, MouseEventArgs e)
        {
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int HTCAPTION = 2;

            if (e.Button == MouseButtons.Left)  // 按下的是鼠标左键   
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);// 拖动窗体  
            }
        }
      
        private void InnerIDE_Load(object sender, EventArgs e)
        {

        }
        #endregion
        #region 语法高亮器
        private void pictureBox_T_Exit_Click(object sender, EventArgs e)
        {
            Dispose();
        }
        private void showLineNo()
        {
            //获得当前坐标信息
            Point p = this.richTextBox_Main.Location;
            int crntFirstIndex = this.richTextBox_Main.GetCharIndexFromPosition(p);
            int crntFirstLine = this.richTextBox_Main.GetLineFromCharIndex(crntFirstIndex);
            Point crntFirstPos = this.richTextBox_Main.GetPositionFromCharIndex(crntFirstIndex);
            p.Y += this.richTextBox_Main.Height;
            int crntLastIndex = this.richTextBox_Main.GetCharIndexFromPosition(p);
            int crntLastLine = this.richTextBox_Main.GetLineFromCharIndex(crntLastIndex);
            Point crntLastPos = this.richTextBox_Main.GetPositionFromCharIndex(crntLastIndex);
            //准备画图
            Graphics g = this.panel1.CreateGraphics();
            Font font = new Font(this.richTextBox_Main.Font, this.richTextBox_Main.Font.Style);
            SolidBrush brush = new SolidBrush(Color.Green);
            //刷新画布
            Rectangle rect = this.panel1.ClientRectangle;
            brush.Color = this.panel1.BackColor;
            g.FillRectangle(brush, 0, 0, this.panel1.ClientRectangle.Width, this.panel1.ClientRectangle.Height);
            brush.Color = Color.White;//重置画笔颜色
            //绘制行号
            int lineSpace = 0;
            if (crntFirstLine != crntLastLine)
            {
                lineSpace = (crntLastPos.Y - crntFirstPos.Y) / (crntLastLine - crntFirstLine);
            }
            else
            {
                lineSpace = Convert.ToInt32(this.richTextBox_Main.Font.Size);
            }
            int brushX = this.panel1.ClientRectangle.Width - Convert.ToInt32(font.Size * 3);
            int brushY = (crntLastPos.Y - Convert.ToInt32(font.Size * 0.21f)) + Convert.ToInt32(font.Size * 0.21f);//惊人的算法啊！！
            for (int i = crntLastLine; i >= crntFirstLine; i--)
            {
                g.DrawString((i + 1).ToString(), font, brush, brushX, brushY);
                brushY -= lineSpace;
            }
            g.Dispose();
            font.Dispose();
            brush.Dispose();
        }

        private void richTextBox_Main_TextChanged(object sender, EventArgs e)
        {
            showLineNo();
        }

        private void richTextBox_Main_VScroll(object sender, EventArgs e)
        {
            showLineNo();
        }

        //关键字集
        public static List<string> AllClass()
        {
            List<string> list = new List<string>();
            list.Add("EXIT");
            list.Add("SLEEP");
            list.Add("SET");
            list.Add("SIGN");
            list.Add("REPEAT");
            list.Add("END");
            list.Add("ADD");
            return list;
        }

        //设定颜色
        public static void MySelect(System.Windows.Forms.RichTextBox tb, int i, string s, Color c, bool font)
        {
            try
            {
                tb.Select(i - s.Length, s.Length);
                tb.SelectionColor = c;
                //以下是把光标放到原来位置，并把光标后输入的文字重置
                tb.Select(i, 0);
                tb.SelectionFont = new Font(" Courier New ", 10.5f, (FontStyle.Regular));
                tb.SelectionColor = Color.Black;
            }
            catch { }
        }

        //返回搜索字符
        public static string GetLastWord(string str, int i)
        {
            string x = str;
            Regex reg = new Regex(@"\S+[a-z]+\S*|[a-z]+\S*|\S+[a-z]*", RegexOptions.RightToLeft);
            x = reg.Match(x).Value;
            Regex reg2 = new Regex(@"\s");
            x = reg2.Replace(x, "");
            return x;
        }
 
        private void richTextBox_Main_KeyDown(object sender, KeyEventArgs e)
        {
            //语法高亮
            string s = GetLastWord(richTextBox_Main.Text, richTextBox_Main.SelectionStart);
            if (AllClass().IndexOf(s) > -1)
            {
                MySelect(richTextBox_Main, richTextBox_Main.SelectionStart, s, Color.CadetBlue, true);
            }
            if (stOpenRem)
            {
                //语法提示
                RichTextBox tb = (RichTextBox)sender;
                if (true)
                {
                    //搜索ListBox是否已经被创建
                    Control[] c = tb.Controls.Find("mylb", false);
                    if (c.Length > 0)
                        ((ListBox)c[0]).Dispose();  //如果被创建则释放
                    lb = new ListBox();
                    lb.Name = "mylb";
                    string lastWord = GetLastWord(richTextBox_Main.Text, 1);
                    foreach (var item in AllClass())
                    {
                        if (item.Contains(lastWord))
                            lb.Items.Add(item);
                    }
                    lb.Show();
                    lb.TabIndex = 100;
                    lb.Location = tb.GetPositionFromCharIndex(tb.SelectionStart);
                    lb.Left += 10;
                    tb.Controls.Add(lb);
                    //添加事件
                    lb.Focus();
                    lb.KeyPress += new KeyPressEventHandler(this.ListBoxRemind_Enter);
                }
            }
        }
        private void richTextBox_Main_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 20)
            {
                lb.Dispose();
            }
            if (e.KeyChar == 37)
            {
                richTextBox_Main.Focus();
            }
        }
        #endregion
        #region 语法提示框关键事件
        protected void ListBoxRemind_Enter(object sender,KeyPressEventArgs e) 
        {
            if (e.KeyChar == 13) 
            {
                string lastWord = GetLastWord(richTextBox_Main.Text,1);
                int lastWordLength = lastWord.Length;
                string temp = richTextBox_Main.Text.Substring(0, richTextBox_Main.Text.Length - lastWord.Length);
                richTextBox_Main.Text = temp;
                richTextBox_Main.Text += lb.SelectedItem;
            }
            if (e.KeyChar == 32)
            {
                lb.Dispose();
            }
            if (e.KeyChar == 37)
            {
                richTextBox_Main.Focus();
            }
        }
        #endregion
        #region 语法解析器
        protected int KDP = 0, rw = 0;
        protected List<List<string>> rx = new List<List<string>>();
        protected void SyntaxParser(string row, int dwStatus = 0)
        {
            try {
                nInput = new List<string>();
                if (dwStatus == 0)
                {
                    richTextBox_Output.Text += $"\nlocal@user>{row}";
                }
                //语法解析
                Regex parser = new Regex(@"\S+[a-z]+\S*|[a-z]+\S*|\S+[a-z]*");
                MatchCollection result = parser.Matches(row);
                foreach (var item in result)
                {
                    nInput.Add(item.ToString());
                }
                //识别
                //方式：顺序识别
                //IDE多步语法解析器
                #region FOR
                if (nInput[0] == "END")
                {
                    rw = 0;
                    for (int i = 0; i < KDP; i++)
                    {
                        foreach (var item in rx)
                        {
                            string tempRow = "";
                            int j = 0;
                            foreach (var txt in item)
                            {
                                if (j != 0) tempRow += " ";
                                tempRow += txt;
                                j++;
                            }
                            //提前更新一下吧...
                            tempRow = tempRow.Replace("[EPH]", EPH.ToString());
                            SyntaxParser(tempRow, 0);
                            Thread.Sleep(100);
                        }

                    }
                }
                if (rw != 0)
                {
                    rx.Add(nInput);//COMPILE .\A.TXT
                    return;
                }
                if (nInput[0] == "REPEAT" && dwStatus == 1)
                {
                    if (KDP == 0)
                    {
                        //执行循环
                        KDP = Convert.ToInt32(nInput[1]);
                        rw = 1;
                        return;
                    }
                }
                #endregion
                //IDE普通命令解析器
                try
                {
                    //休眠命令
                    if (nInput[0] == "SLEEP")
                    {
                        if (nInput[1] != "EPH")
                        {
                            Thread.Sleep(Convert.ToInt32(nInput[1]));
                            richTextBox_Output.Text += $"\nConsole@Enviroment>休眠{nInput[1]}毫秒。";
                        }
                        else
                        {
                            Thread.Sleep(EPH);
                            richTextBox_Output.Text += $"\nConsole@Enviroment>休眠{EPH}毫秒。";
                        }
                    }
                    else if (nInput[0] == "SET")
                    {
                        ArrayList arraySet = new ArrayList();
                        arraySet.AddRange(nInput);
                        if (arraySet[1].ToString() == "EPH")
                        {
                            //整数型赋值
                            EPH = Convert.ToInt32(arraySet[2]);
                            richTextBox_Output.Text += $"\nConsole@Memory>将{arraySet[2]}设置到整数存储器。";
                        }
                        else
                        {
                            richTextBox_Output.Text += $"\nMemoryNotExistException：未找到该存储器";
                            try
                            {
                                throw new Exceptions.MethodNotExistException("MemoryNotExistException：未找到该存储器。");
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine("MemoryNotExistException：未找到该存储器。");
                                //发送错误报告
                                var properties = new Dictionary<string, string>
                            {
                                {"Display" , $"SyntaxError：{ex.Message}" }
                            };
                                string code = "";
                                foreach (var item in nInput)
                                {
                                    code += item + " ";
                                }
                                properties.Add("Code", code);
                                Crashes.TrackError(ex, properties);
                            }
                        }
                    }
                    else if (nInput[0].StartsWith("EPH"))
                    {
                        //存储器操作
                        if (nInput[0] == "EPH+")
                        {
                            EPH += Convert.ToInt32(nInput[1]);
                        }
                        else if (nInput[0] == "EPH-")
                        {
                            EPH -= Convert.ToInt32(nInput[1]);
                        }
                        else if (nInput[0] == "EPH*")
                        {
                            EPH *= Convert.ToInt32(nInput[1]);
                        }
                        else if (nInput[0] == "EPH/")
                        {
                            EPH /= Convert.ToInt32(nInput[1]);
                        }
                        else
                        {
                            try
                            {
                                throw new Exceptions.MethodNotExistException("MethodNotExistError：存储器EPH不存在该操作。");
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine("MethodNotExistError：存储器EPH不存在该操作。");

                                //发送错误报告
                                var properties = new Dictionary<string, string>
                            {
                                {"code" , $"{richTextBox_Output.Text}" },
                                {"display" , $"SyntaxError：{ex.Message}" }
                            };
                                Crashes.TrackError(ex, properties);
                                richTextBox_Output.Text += $"\nSyntaxError：{ex.Message}";
                            }
                        }
                        richTextBox_Output.Text += $"\nConsole@Memory>EPH：{EPH.ToString()}";
                    }
                    else if (nInput[0] == "ADD")
                    {
                        //打开信号接口
                        Main.Sign = 4;
                        if (dwStatus != 1)
                        {
                            if (nInput[1].Contains("[EPH]"))
                            {
                                nInput[1] = nInput[1].Replace("[EPH]", EPH.ToString());
                            }
                            if (nInput[2].Contains("[EPH]"))
                            {
                                nInput[2] = nInput[2].Replace("[EPH]", EPH.ToString());
                            }
                        }
                        Main.tName = nInput[1];
                        Main.tTime = Convert.ToInt32(nInput[2]);
                        richTextBox_Output.Text += $"\nConsole@Main>Main：添加任务{nInput[1]}，时长{nInput[2]}。";
                    }
                    else if (nInput[0] == "COMPILE")
                    {
                        nInput[1] = nInput[1].Replace(@".\", Application.StartupPath + @"\");
                        try
                        {
                            using (StreamReader sr = new StreamReader(nInput[1]))
                            {
                                textBox_Pars.Text = sr.ReadToEnd().Trim();
                            }
                            richTextBox_Output.Text += $"\nConsole@Main>Main：开始编译解析{nInput[1]}。";
                            //开新线程解析语法
                            Thread threadCompile = new Thread(new ThreadStart(Compile));
                            threadCompile.Start();
                        }
                        catch { richTextBox_Output.Text += $"\nFileNotExistError：指定文件不存在。"; }

                    }
                    else if (nInput[0] == "SIGN")
                    {
                        if (nInput[1] == "HELP")
                        {
                            richTextBox_Output.Text += "\nConsole@Main>信 号 规 范：";
                            richTextBox_Output.Text += "\n|SIGN = 1 => 删除任务";
                            richTextBox_Output.Text += "\n|SIGN = 2 => 伸长菜单栏";
                            richTextBox_Output.Text += "\n|SIGN = 3 => 缩短菜单栏";
                            richTextBox_Output.Text += "\n|SIGN = 4 => 新建待办";
                            richTextBox_Output.Text += "\n|SIGN = 5 => 传出自身对象集合";
                            richTextBox_Output.Text += "\n请注意：错误的使用信号将导致崩溃";
                        }
                        else
                        {
                            try
                            {
                                int signal = Convert.ToInt32(nInput[1]);
                                Main.Sign = signal;
                                richTextBox_Output.Text += $"\nConsole@Poster>成功：将{signal}信号发送到主窗口。";
                            }
                            catch (Exception ex)
                            {
                                //发送错误报告
                                var properties = new Dictionary<string, string>
                            {
                                {"code" , $"{richTextBox_Output.Text}" },
                                {"display" , $"SyntaxError：{ex.Message}" }
                            };
                                Crashes.TrackError(ex, properties);
                                richTextBox_Output.Text += $"\nSyntaxError：{ex.Message}";
                            }
                        }
                    }
                    else if (nInput[0] == "EXIT")
                    {
                        richTextBox_Output.Text = "";
                        Dispose();
                    }
                }
                catch (Exception ex)
                {
                    //发送错误报告
                    var properties = new Dictionary<string, string>
                {
                    {"code" , $"{richTextBox_Output.Text}" },
                    {"display" , $"SyntaxError：{ex.Message}" }
                };
                    Crashes.TrackError(ex, properties);
                    richTextBox_Output.Text += $"\nSyntaxError：{ex.Message}";
                }

            }
            catch (Exception ex)
            {
                //发送错误报告
                var properties = new Dictionary<string, string>
                {
                    {"code" , $"{richTextBox_Output.Text}" },
                    {"display" , $"SyntaxError：{ex.Message}" }
                };
                Crashes.TrackError(ex, properties);
                string rew = "";
                foreach (var item in nInput)
                {
                    rew += item + " ";
                }
                richTextBox_Output.Text += $"\nSyntaxError：{ex.Message}，In：{rew}.";
            }
        }
        #endregion
        #region 运行时解释器
        protected void Compile()
        {
            string[] command = textBox_Pars.Text.Trim().Split('\n');
            foreach (var item in command)
            {
                richTextBox_Output.Text += $"\nConsole@Compiler>{item}";
                SyntaxParser(item, 1);
                Thread.Sleep(1000);
            }
            richTextBox_Output.Text += $"\nConsole@Compiler>编译解析完成。";
        }
        protected void Compile(string row)
        {
            richTextBox_Output.Text += $"\nConsole@Compiler>指令收到，开始解释运行。";
            string[] command = row.Trim().Split('\n');
            foreach (var item in command)
            {
                richTextBox_Output.Text += $"\nConsole@Compiler>{item}";
                SyntaxParser(item, 1);
                Thread.Sleep(1000);
            }
            richTextBox_Output.Text += $"\nConsole@Compiler>编译解析完成。";
        }
        #endregion
        #region 上侧边栏功能
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            stOpenRem = !stOpenRem;
        }

        private unsafe void 中文插入CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //char szBufferSize;
            //char* chineseText = &szBufferSize;
            //InputBoxStruct IBS = new InputBoxStruct();
            //IBS.lpText = "请输入您想插入的中文";
            //IBS.lpCaption = "中文插入系统";
            //IBS.szValueBack = chineseText;
            //InputBox IB = new InputBox(IBS);
            //IB.Show();
            //TaskFactory nameGetThread = new TaskFactory();
            //nameGetThread.StartNew(() => FileAddition(chineseText, IB));
            Inner.InnerFuncs.CmdExecuter.RunCmd("notepad");
        }
        private unsafe void 新建NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            char[] szFileNameSize = new char[256];
            fixed (char* fileName = szFileNameSize)
            {
                InputBoxStruct IBS = new InputBoxStruct();
                IBS.lpText = "输入文件名";
                IBS.lpCaption = "请输入文件名";
                IBS.szValueBack = fileName;
                InputBox input = new InputBox(IBS);
                input.Show();
                TaskFactory nameGetThread = new TaskFactory();
                char* temp = fileName;
                nameGetThread.StartNew(() => FileAddition(temp, input));
            }
        }
        public unsafe void FileAddition(char* fileName, InputBox input)
        {
            //FIXED FILENAME AS CONST
            while (true)
            {
                if (input.IN)
                {
                    char* temp = fileName;
                    for (int i = 0; i < 256; i++)
                    {
                        fileNameString += *temp++;
                    }
                    richTextBox_Main.Text += $"//Filename：{fileNameString}\n";
                    iFileName = $"{Application.StartupPath}\\coding\\{fileNameString.Trim(new char[] { '\0'})}.mtd";
                    iFileName.Replace('\0', ' ');
                    input.IN = !input.IN;
                }
            }
        }

        //        foreach (var item in fileName)
        //{
        //    fileNameString += *item;
        //}

        private void 文件FToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 打开OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            openFileDialog.Filter = "MethodBox脚本文件|*.mtd|文本文件|*.txt|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fName = openFileDialog.FileName;
                MessageBox.Show(fName);
                using (StreamReader sr = new StreamReader(fName))
                {
                    string result = sr.ReadToEnd().Trim();
                    this.richTextBox_Main.Text = result;
                }
            }
        }

        private void 保存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(iFileName))
                {
                    sw.Write(richTextBox_Main.Text);
                    MessageBox.Show("保存成功。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DialogResult dr =
                MessageBox.Show($"保存失败，原因是{ex.Message}。\n是否向MethodBox报告该错误（自动报告）？", "失败",
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dr == DialogResult.Yes)
                {
                    throw ex;
                }
                else
                { }
            }

        }
        private void 打印PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = $@"{Application.StartupPath}\temp\pFile{new Random().Next(100000, 999999)}.txt";
            File.Create(path).Close();
            StreamWriter sw = new StreamWriter(path);
            sw.Write(richTextBox_Main.Text);
            sw.Close();
            Inner.InnerFuncs.CmdExecuter.RunCmd($"notepad {path}");
        }

        private void 打印预览VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = $@"{Application.StartupPath}\temp\pFile{new Random().Next(100000, 999999)}.txt";
            File.Create(path).Close();
            StreamWriter sw = new StreamWriter(path);
            sw.Write(richTextBox_Main.Text);
            sw.Close();
            Inner.InnerFuncs.CmdExecuter.RunCmd($"notepad {path}");
        }

        private void 退出XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void 撤消UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.Undo();
        }

        private void 重复RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.Redo();
        }

        private void 剪切TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.Cut();
        }

        private void 复制CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.Copy();
        }

        private void 粘贴PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.Paste();
        }

        private void 全选AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.SelectAll();
        }

        private void DateTimeDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Main.Text += DateTime.Now.ToString();
        }

        //可访问性权限引子
        public RichTextBox rtbMain
        {
            get { return this.richTextBox_Main; }
            set { this.richTextBox_Main = value; }
        }

        private void FindFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Finder fd = new Finder(this);
            fd.Show();
        }

        private void 关于AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.Show();
        }
        private void 搜索SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Finder fd = new Finder(this);
            fd.Show();
        }

        private void 内容CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(richTextBox_Main.Text);
        }

        private void 索引IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://afdian.net/@MethodBox");
        }

        private void 停止调试EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                main.Hide();
                main = null;
            }
            catch { }
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                SyntaxParser(richTextBox_Input.Text.Substring(1));
                richTextBox_Input.Text = ">";
                richTextBox_Input.Select(1, 0);
            }
        }

        private void 本线程调试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = new StreamWriter(iFileName))
            {
                sw.Write(richTextBox_Main.Text);
            }
            tabControl_Buttom.SelectedIndex = 1;
            tabControl_Buttom.Focus();
            tabPage_Jh.Focus();
            richTextBox_Input.Focus();
            richTextBox_Input.Text = $">COMPILE {iFileName}";
            richTextBox_Input.Select(richTextBox_Input.TextLength,0);
        }

        private void 开始调试SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox_Output.Text = String.Empty;
            main = new Main();
            main.Show();
            Compile(richTextBox_Main.Text); 
        }

        private void 另存为AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //可能要获取的路径名
            string localFilePath = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //书写规则例如：txt files(*.txt)|*.txt
            saveFileDialog.Filter = "MethodBox脚本文件|*.mtd|文本文件|*.txt|所有文件|*.*";
            //主设置默认文件extension（可以不设置）
            saveFileDialog.DefaultExt = "mtd";
            //获取或设置一个值，该值指示如果用户省略扩展名，文件对话框是否自动在文件名中添加扩展名。（可以不设置）
            saveFileDialog.AddExtension = true;
            //保存对话框是否记忆上次打开的目录
            saveFileDialog.RestoreDirectory = true;
            // Show save file dialog box
            DialogResult result = saveFileDialog.ShowDialog();
            //点了保存按钮进入
            if (result == DialogResult.OK)
            {
                //获得文件路径
                localFilePath = saveFileDialog.FileName.ToString();
                using (StreamWriter sw = new StreamWriter(localFilePath))
                {
                    sw.Write(richTextBox_Main.Text);
                    MessageBox.Show("保存成功。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion
    }
}
