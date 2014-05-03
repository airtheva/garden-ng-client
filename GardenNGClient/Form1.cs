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

            // FIXME: Dirty.
            button2_Click(null, null);

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    ActiveControl = textBox1;
                    break;
                case 1:
                    requestSlaves();
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

            if (gamePath.Equals(""))
            {
                MessageBox.Show("没配置游戏路径，不能帮你启动游戏了哟！");
            }
            else if (File.Exists(gamePath))
            {
                System.Diagnostics.Process.Start(gamePath);
            }
            else
            {
                MessageBox.Show("玛德，游戏路径搞错了吧！");
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

        private void button11_Click(object sender, EventArgs e)
        {

            requestSlaves();

        }

        private void button3_Click(object sender, EventArgs e)
        {

            API.GetInstance().Mount("udpSocketProxy", (String) dataGridView3.CurrentRow.Cells[1].Value);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            updateRecords();
        }

        private void button10_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "*.db|*.db";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SWRSBattleRecorder.GetInstance().MergeToTSK(dialog.FileName);
            }

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

                    foreach(KeyValuePair<String, API.UserData> item in users.users) {
                        listBox1.Items.Add(String.Format("{0}[{1}]", item.Value.nickname, item.Value.identity));
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
                case "slaves":
                    API.Response.Slaves slaves = SimpleJson.SimpleJson.DeserializeObject<API.Response.Slaves>(json.data.ToString());

                    dataGridView3.Rows.Clear();

                    foreach(KeyValuePair<String, API.SlaveData> item in slaves.slaves) {

                        int index = dataGridView3.Rows.Add();

                        DataGridViewRow row = dataGridView3.Rows[index];

                        row.Cells[0].Value = item.Value.name;
                        row.Cells[1].Value = item.Value.address;

                    }

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

        void requestSlaves()
        {

            API.GetInstance().GetSlaves();

        }

        void updateRecords()
        {

            dataGridView2.DataSource = SWRSBattleRecorder.GetInstance().GetDataTable();

        }

        void loadSettings()
        {
            Settings settings = Settings.GetInstance();

            textBox6.Text = settings.Store.Nickname;
            textBox7.Text = settings.Store.GamePath;

        }

        void saveSettings()
        {

            Settings settings = Settings.GetInstance();

            settings.Store.Nickname = textBox6.Text;
            settings.Store.GamePath = textBox7.Text;

            settings.Save();

        }

    }
}
