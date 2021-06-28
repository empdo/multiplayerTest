using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Numerics;

using GameServer;

public class Server
{

    static object _lock = new object();
    static Dictionary<int, GameServer.Player> client_list = new Dictionary<int, GameServer.Player>();
    public static void Main()
    {
        TcpListener server;

        Int32 port = 25250;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        server = new TcpListener(localAddr, port);
        server.Start();

        int count = 1;
        Console.WriteLine("Waiting for a connection... ");
        while (true)
        {

            TcpClient connection = server.AcceptTcpClient();

			GameServer.Player player = new GameServer.Player(connection);
            lock (_lock) { client_list.Add(count, player); }

            Thread thread = new Thread(clientThread);
            thread.Start(count);
            count++;
        }

    }

    public void SendPlayerID(Player player, int id) {
        lock (player) {
            player.id = Convert.ToUInt16(id); 
            player.QueuePacket(0, BitConverter.GetBytes(id));
        }
    }

    public void SendPositionDelta() {
        //TODO: gör dehär    
    }



    void HandlePlayerDeltaPacket(Player player, byte[] deltaPacket) {

        float dx = BitConverter.ToSingle(deltaPacket, 0);
        float dy = BitConverter.ToSingle(deltaPacket, sizeof(float));
        float dz = BitConverter.ToSingle(deltaPacket, sizeof(float) *2);

        lock (player) {
           player.UpdatePositionFromDelta(dx, dy, dz); 
        }
    }
    public static void clientThread(object threadIndex)
    {


		GameServer.Player player;
        lock (_lock) { player = client_list[(int)threadIndex]; }
        Console.WriteLine("Connection established from: {0}", player.client.Client.RemoteEndPoint);
        NetworkStream stream = player.client.GetStream();

        while(player.client.Client.Connected) {

            // 3, 4, 0, 0, 0, 0
            
            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, buffer.Length);
            ushort packageType = BitConverter.ToUInt16(buffer, 0);

            stream.Read(buffer, 0, buffer.Length);
            ushort packageLength = BitConverter.ToUInt16(buffer, 0);

            Console.WriteLine("Type is " + packageType + " and length is " + packageLength);

            byte[] packageContent = new byte[packageLength];
            stream.Read(packageContent, 0, packageContent.Length);

            switch(packageType) {
                case 0:
                    SendPlayerID(player, (int)threadIndex);
                    break;
                case 1:
                    // Server event
                    break;
                case 2:
                    HandlePlayerDeltaPacket(player, packageContent);
                    // Tick packet (delta from last tick)
                    break;

            foreach (byte[] packet in player.packetQueue)
            {
               stream.Write(packet);
            }

            player.packetQueue.Clear();

        }


        //Console.WriteLine("Shutting down connection to {0}", player.client.Client.RemoteEndPoint);
        //lock (_lock) client_list.Remove((int)threadIndex);
        //player.client.Client.Shutdown(SocketShutdown.Both);
        //player.client.Close();
        }
    }
}
