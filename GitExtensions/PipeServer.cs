using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
/* 
 * Original code from Martijn Dijksterhuis
 * http://www.dijksterhuis.org/using-named-pipes-in-c-windows/
 * 
 */
namespace GitExtensions
{   
    /// <summary>
    /// 
    /// </summary>
    class PipeServer
    {
        public void StartServer()
            {
                System.Windows.Forms.MessageBox.Show("server started");
                try
                {
                    // Create a name pipe
                    using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("GitExtensionsPipe"))
                    {
                        Console.WriteLine("[Server] Pipe created {0}", pipeStream.GetHashCode());

                        // Wait for a connection
                        pipeStream.WaitForConnection();
                        Console.WriteLine("[Server] Pipe connection established");

                        using (StreamReader sr = new StreamReader(pipeStream))
                        {
                            string temp;
                            // We read a line from the pipe and print it together with the current time
                            while ((temp = sr.ReadLine()) != null)
                            {
                                //Console.WriteLine("[Server] {0}: {1}", DateTime.Now, temp);
                                //if (temp == "exit exit")
                                //{}
                                temp = ". " + temp;
                                string[] cmd = temp.Split(' ');

                                GitExtensions.Program.RunCommand(cmd);
                                //System.Windows.Forms.MessageBox.Show("close");

                                //Environment.Exit(9);

                            }
                        }
                    }
                }
                catch (IOException ie)
                {
                    System.Windows.Forms.MessageBox.Show(ie.Message);
                    Environment.Exit(9);
                }
                Console.WriteLine("Connection lost");
            }
    }
}
