using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebSocket4Net;
using System.Threading;
using System.Net;
using System.IO;

namespace GardenNGClient
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Settings settings = Settings.GetInstance();
            API api = API.GetInstance();
            SWRSMonitor swrsMonitor = SWRSMonitor.GetInstance();

            loadSettings();

            api.WebSocket.Error += delegate(Object _sender, SuperSocket.ClientEngine.ErrorEventArgs _e)
            {
                Invoke(new onErrorDelegate(onError), _sender, _e);
            };
            api.WebSocket.Opened += delegate(Object _sender, EventArgs _e)
            {
                Invoke(new onOpenedDelegate(onOpened), _sender, _e);
            };
            api.WebSocket.MessageReceived += delegate(Object _sender, MessageReceivedEventArgs _e)
            {
                Invoke(new onMessageReceivedDelegate(onMessageReceived), _sender, _e);
            };
            api.WebSocket.Closed += delegate(Object _sender, EventArgs _e)
            {
                Invoke(new onClosedDelegate(onClosed), _sender, _e);
            };

            swrsMonitor.OnGameFound += delegate(Object _sender, SWRSMonitor.GameFoundEventArgs _e)
            {
                toolStripStatusLabel2.Text = String.Format("风险投资{0}已启动", _e.Version);
            };

            swrsMonitor.OnGameLost += delegate(Object _sender, EventArgs _e)
            {
                toolStripStatusLabel2.Text = "风险投资未启动";
            };

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    ActiveControl = textBox1;
                    break;
                case 2:
                    updateRecords();
                    break;
            }

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            switch (e.KeyChar)
            {
                case (char) Keys.Enter:
                    sendMessage();
                    break;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendMessage();
        }

        private void button8_Click(object sender, EventArgs e)
        {

            Settings settings = Settings.GetInstance();

            settings.Store.Nickname = textBox6.Text;

            settings.Save();

            API.GetInstance().ChangeNickname(settings.Store.Nickname);

        }

        private void button2_Click(object sender, EventArgs e)
        {

            String gamePath = Settings.GetInstance().Store.GamePath;

            if (File.Exists(gamePath))
            {
                System.Diagnostics.Process.Start(gamePath);
            }
            else
            {
                MessageBox.Show("呜呜呜，没配置游戏路径或者配置错了，快去配置里看看！");
            }

            SWRSMonitor.GetInstance().Start();
            SWRSBattleRecorder.GetInstance().Start();
            toolStripStatusLabel2.Text = "监视开始";

        }

        private void button7_Click(object sender, EventArgs e)
        {

            SWRSBattleRecorder.GetInstance().Stop();
            SWRSMonitor.GetInstance().Stop();
            toolStripStatusLabel2.Text = "监视停止";

        }

        private void button3_Click(object sender, EventArgs e)
        {
            API.GetInstance().Mount("udpSocketProxy");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "th123.exe|th123.exe";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox7.Text = dialog.FileName;
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadSettings();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            saveSettings();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            Console.WriteLine("Closing.");
            SWRSMonitor.GetInstance().Stop();

        }

        delegate void onErrorDelegate(Object sender, SuperSocket.ClientEngine.ErrorEventArgs e);
        delegate void onOpenedDelegate(Object sender, EventArgs e);
        delegate void onMessageReceivedDelegate(Object sender, MessageReceivedEventArgs e);
        delegate void onClosedDelegate(Object sender, EventArgs e);
        delegate void onBattleEndedDelegate(Object sender, SWRSMonitor.BattleEndedEventArgs e);

        void onError(Object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {

            toolStripStatusLabel1.Text = "服务器已断开：" + e.Exception.Message + "";

        }

        void onOpened(Object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "服务器已连接";
        }

        void onMessageReceived(Object sender, MessageReceivedEventArgs e)
        {

            API api = API.GetInstance();

            API.Message<Object> json = SimpleJson.SimpleJson.DeserializeObject<API.Message<Object>>(e.Message);

            switch (json.type)
            {
                case "users":
                    API.Response.Users users = SimpleJson.SimpleJson.DeserializeObject<API.Response.Users>(json.data.ToString());

                    listBox1.BeginUpdate();

                    listBox1.Items.Clear();

                    foreach(API.UserData user in users.users) {
                        listBox1.Items.Add(String.Format("{0}[{1}]", user.nickname, user.identity));
                    }

                    listBox1.EndUpdate();

                    break;
                case "messages":
                    API.Response.Messages messages = SimpleJson.SimpleJson.DeserializeObject<API.Response.Messages>(json.data.ToString());

                    foreach (API.MessageData message in messages.messages)
                    {
                        appendMessage(message);
                    }
                    
                    break;
                case "newUser":
                    API.Response.NewUser newUser = SimpleJson.SimpleJson.DeserializeObject<API.Response.NewUser>(json.data.ToString());

                    listBox1.BeginUpdate();

                    listBox1.Items.Add(String.Format("{0}[{1}]", newUser.nickname, newUser.identity));

                    listBox1.EndUpdate();

                    break;
                case "newMessage":
                    API.Response.NewMessage newMessage = SimpleJson.SimpleJson.DeserializeObject<API.Response.NewMessage>(json.data.ToString());
                    appendMessage(newMessage);

                    break;
                case "forward":
                    API.Response.Forward forward = SimpleJson.SimpleJson.DeserializeObject<API.Response.Forward>(json.data.ToString());
                    textBox4.Text = String.Format("{0}:{1}", forward.listenHost, forward.listenPort);

                    UDPSocketProxy.GetInstance().HeartbeatEP = new IPEndPoint(IPAddress.Parse(forward.heartbeatHost), forward.heartbeatPort);

                    break;
                case "punch":
                    API.Response.Punch punch = SimpleJson.SimpleJson.DeserializeObject<API.Response.Punch>(json.data.ToString());
                    textBox3.Text = String.Format("{0}:{1}", punch.punchHost, punch.punchPort);

                    break;
            }

        }

        void onClosed(Object sender, EventArgs e)
        {
            
        }

        void appendMessage(API.MessageData message)
        {

            DateTime time = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(message.time / 1000).ToLocalTime();
            richTextBox1.AppendText(String.Format("[{0}] {1}[{2}]: {3}\n", time.ToString("HH:mm:ss"), message.nickname, message.identity, message.message));
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();

        }

        void sendMessage()
        {

            API.GetInstance().SendMessage(textBox1.Text);
            textBox1.ResetText();

        }

        void updateRecords()
        {

            dataGridView2.DataSource = SWRSBattleRecorder.GetInstance().GetDataTable();

        }

        void loadSettings()
        {
            Settings settings = Settings.GetInstance();

            textBox2.Text = settings.Store.ServerHost;
            textBox5.Text = settings.Store.ServerPort.ToString();
            textBox6.Text = settings.Store.Nickname;
            textBox7.Text = settings.Store.GamePath;

        }

        void saveSettings()
        {

            Settings settings = Settings.GetInstance();

            settings.Store.ServerHost = textBox2.Text;
            settings.Store.ServerPort = Int32.Parse(textBox5.Text);
            settings.Store.Nickname = textBox6.Text;
            settings.Store.GamePath = textBox7.Text;

            settings.Save();

        }

    }
}
