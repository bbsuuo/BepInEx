using BepInEx.Preloader.Socket;
using System;
using System.Text;

namespace BepInEx
{
    public static class TSSocketHandle
    {
        private static bool debugSocket = false;
        private static TSBepInExSocket _socket;
        internal static void TryConnectTSClinet()
        {
            if(!debugSocket) return;
            try
            {
                _socket = new TSBepInExSocket("127.0.0.1", 12555);
                _socket.Connect();
                //_socket.SendPackage(254,"TranslateStudio - 解析程序 - 连接");
                TrySendMessageWithHead("解析程序启动");
                HarmonyLib.Tools.HarmonyFileLog.Enabled = false;
                HarmonyLib.Tools.Logger.ChannelFilter = HarmonyLib.Tools.Logger.LogChannel.All;
                HarmonyLib.Tools.Logger.MessageReceived += HarmonyLibMessageReceived;

            }
            catch (Exception)
            {

            }
        }
        internal static void HarmonyLibMessageReceived(object sendere , HarmonyLib.Tools.Logger.LogEventArgs arg) 
        {
            string message = $"[HarmonyLog][{arg.LogChannel}] {arg.Message}";
            TrySendMessage(message);
        }

        internal static void TryDisconnectTsClient()
        {
            if (!debugSocket) return;
            TrySendMessageWithHead("解析程序关闭");
            HarmonyLib.Tools.Logger.MessageReceived -= HarmonyLibMessageReceived;
            try {
                if(_socket!=null)
                _socket.Disconnect();
            }
            catch (Exception)
            {

            }
        }

        internal static void TrySendMessage(string msg)
        {
            if (_socket != null)
            {
                try
                {
                    _socket.SendPackage(254, msg);
                }
                catch (Exception)
                {

                }

            }
        }

        internal static void TrySendMessageError(string msg)
        {
            if (_socket != null)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(" - 解析程序出现错误 -");
                    sb.AppendLine(msg);
                    _socket.SendPackage(254, sb.ToString());
                }
                catch (Exception)
                {

                }

            }
        }

        internal static void TrySendMessageWithHead(string msg)
        {
            if (_socket != null)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(" - 解析程序日志 -");
                    sb.AppendLine(msg);
                    _socket.SendPackage(254, sb.ToString());
                }
                catch (Exception)
                {

                }

            }
        }
    }
}
