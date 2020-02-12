using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProxmoxSpiceLauncher
{
    class Program
    {        
        // spice proxmox viewer
        // based on other peoples code snippets in this thread.        
        // https://forum.proxmox.com/threads/remote-spice-access-without-using-web-manager.16561/
        //
        // example command line args
        //
        // host=192.168.0.1 port=8006 username=root@pam password=xxx node=pve vm=101 viewer="C:\Program Files\VirtViewer v8.0-256\bin\remote-viewer.exe" debug=on

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();

        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;
        private static bool showConsole = false; //Or false if you don't want to see the console

        static void Main(string[] args)
        {
            string[] requiredArgs = new string[] { "username", "password", "host", "port", "node", "vm", "viewer" };
            var parsedArgs = args.Select(s => s.Split('=')).ToDictionary(s => s[0], s => s[1]);

            if (parsedArgs.ContainsKey("debug"))
            {
                if (parsedArgs["debug"] == "on")
                 showConsole = true;
            }

            if (showConsole)
            {
                AllocConsole();
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                Microsoft.Win32.SafeHandles.SafeFileHandle safeFileHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
                FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
                StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }


            foreach(var requried in requiredArgs)
            {
                if (!parsedArgs.ContainsKey(requried))
                {
                    Console.WriteLine("missing argument : " + requried);
                    Console.ReadLine();
                    return;
                }
            }
            
            string Username = parsedArgs["username"];
            string Password = parsedArgs["password"];
            string Host = parsedArgs["host"];
            int Port = Convert.ToInt32(parsedArgs["port"]);
            string Node = parsedArgs["node"];
            string VmId = parsedArgs["vm"];
            string RemoteViewer = parsedArgs["viewer"]; 

            ProxmoxAPI aAPI = new ProxmoxAPI();
            aAPI.Username = Username; 
            aAPI.Password = Password;
            aAPI.Host = Host;
            aAPI.Port = Port;

            try
            {
                string Ticket = aAPI.RefreshTicket();
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to get ticket : " + ex.Message);
                Console.ReadLine();
                return;
            }

            string SpiceCommand = "";
            try
            {
                SpiceCommand = aAPI.GetSpiceCommand(Node, VmId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to get spice command : " + ex.Message);
                Console.ReadLine();
                return;
            }

            string FileName = "";

            try
            {
                FileName = Path.GetTempFileName();
                System.IO.File.WriteAllText(FileName, SpiceCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to get write temp file : " + ex.Message);
                Console.ReadLine();
                return;
            }

            try
            {
                Process.Start(RemoteViewer, FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to start remote viewer : " + ex.Message);
                Console.ReadLine();
                return;
            }
}
    }
}
