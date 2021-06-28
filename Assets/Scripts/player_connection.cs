using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServer {
    public class PlayerConnection {

        public Queue<string> packetQueue = new Queue<string>();
        public TcpClient client;

        public readonly object packetQueueLock = new object();
        public PlayerConnection(TcpClient client){
            this.client = client; 
        }

        public void writeQueue(TcpClient client){
            NetworkStream stream = client.GetStream();
            while(packetQueue.Count > 0) {
                foreach(string dict in packetQueue) {
                    foreach(var message in dict){

                            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message.ToString());
                            List<byte> list = new List<byte>();
                            list.AddRange(buffer);
                            stream.Write(list.ToArray(), 0, list.Count);    
                    }
                }
                packetQueue.Clear(); 
            }
        }

    }

}