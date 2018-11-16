﻿using Logs;
using Newtonsoft.Json;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FilePrintService
{
    public class WebSocketHelper
    {
        protected WebSocketServer WebSocketServer { get; private set; }
        //protected WebSocketSession session;
        private int port;

        protected AutoResetEvent MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent DataReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent CloseEvent = new AutoResetEvent(false);
        protected string CurrentMessage { get; private set; }
        protected byte[] CurrentData { get; private set; }

        public WebSocketHelper(int port)
        {
            this.port = port;
        }

        protected void Setup(WebSocketServer websocketServer, Action<ServerConfig> configurator)
        {
            var rootConfig = new RootConfig { DisablePerformanceDataCollector = true };

            websocketServer.NewDataReceived += new SessionHandler<WebSocketSession, byte[]>(WebSocketServer_NewDataReceived);

            var config = new ServerConfig();
            configurator(config);

            var ret = websocketServer.Setup(rootConfig, config, null, null, new ConsoleLogFactory(), null, null);

            WebSocketServer = websocketServer;
        }


        public virtual void Setup()
        {
            Setup(new WebSocketServer(), c =>
            {
                c.Port = port;
                c.Ip = "Any";
                c.MaxConnectionNumber = 100;
                c.MaxRequestLength = 100000;
                c.Name = "SuperWebSocket Server";
            });

            WebSocketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(WebSocketServer_NewMessageReceived);
        }

        protected void WebSocketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<FilePrintService.Models.FilePrintRequest>(e);

                if (!Manager.Checklisence())
                {
                    session.Close();
                    return;
                }

                switch (data.action)
                {
                    case "HandShake":
                        session.Send(JsonConvert.SerializeObject(data));
                        break;
                    case "Print":
                        Manager.Print(data.printer, data.htmlData);
                        session.Close();
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("WebSocketServer_NewMessageReceived", ex);
            }

        }

        protected void WebSocketServer_NewDataReceived(WebSocketSession session, byte[] e)
        {
            session.Send(e, 0, e.Length);
        }

        public void StartServer()
        {
            WebSocketServer.Start();
        }


        public void StopServer()
        {
            WebSocketServer.Stop();
        }

    }
}
