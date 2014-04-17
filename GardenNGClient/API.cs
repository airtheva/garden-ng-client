using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net;
using SuperSocket.ClientEngine;

namespace GardenNGClient
{

    public class API
    {

        public static class Status
        {

            public static int SUCCESS = 0;

        }

        public static class Request
        {

            public class Register
            {

                public String nickname;

            }

            public class SendMessage
            {

                public String message;

            }

            public class Mount
            {

                public String type;
                public String gardenHost;

            }

        }

        public static class Response
        {

            public class Users
            {

                public int status;
                public UserData[] users;

            }

            public class Messages
            {

                public int status;
                public MessageData[] messages;

            }

            public class NewUser : UserData
            {
                public int status;
            }

            public class NewMessage : MessageData
            {
                public int status;
            }

            public class Forward
            {
                public int status;
                public String heartbeatHost;
                public int heartbeatPort;
                public String listenHost;
                public int listenPort;
            }

            public class Punch
            {
                public int status;
                public String punchHost;
                public int punchPort;
            }

        }

        public class Message<T>
        {
            public String type;
            public T data;
        }

        public class UserData
        {

            public String nickname;

        }

        public class MessageData
        {

            public long time;
            public String source;
            public String message;

        }

        static API mInstance;

        public static API GetInstance()
        {

            if (mInstance == null)
            {
                mInstance = new API();
            }

            return mInstance;

        }

        public WebSocket WebSocket;

        private API() {

            Settings settings = Settings.GetInstance();

            WebSocket = new WebSocket(String.Format("ws://{0}:{1}/", settings.Store.ServerHost, settings.Store.ServerPort));

            WebSocket.Error += onError;
            WebSocket.Opened += onOpened;
            WebSocket.MessageReceived += onMessageReceived;
            WebSocket.Closed += onClosed;

            WebSocket.Open();

        }

        void onError(Object sender, ErrorEventArgs e)
        {

            Console.WriteLine(e.Exception.Message);

            WebSocket.Open();

        }

        void onOpened(Object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket opened.");

            Register(Settings.GetInstance().Store.Nickname);

        }

        void onMessageReceived(Object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        void onClosed(Object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket closed.");
        }

        public void Register(String nickname)
        {

            Message<Request.Register> json = new Message<Request.Register>();

            json.type = "register";
            json.data = new Request.Register();
            json.data.nickname = nickname;

            String j = SimpleJson.SimpleJson.SerializeObject(json);

            WebSocket.Send(j);

        }

        public void SendMessage(String message)
        {

            Message<Request.SendMessage> json = new Message<Request.SendMessage>();

            json.type = "sendMessage";
            json.data = new Request.SendMessage();
            json.data.message = message;

            String j = SimpleJson.SimpleJson.SerializeObject(json);

            WebSocket.Send(j);

        }

        public void Mount(String type)
        {

            Message<Request.Mount> json = new Message<Request.Mount>();

            json.type = "mount";

            json.data = new Request.Mount();
            json.data.type = type;
            json.data.gardenHost = Settings.GetInstance().Store.ServerHost;

            String j = SimpleJson.SimpleJson.SerializeObject(json);

            WebSocket.Send(j);

        }

    }


}
