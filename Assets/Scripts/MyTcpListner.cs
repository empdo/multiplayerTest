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

public class MyTcpListner
{

    static object _lock = new object();
    static Dictionary<int, PlayerConnection> client_list = new Dictionary<int, PlayerConnection>();
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

            PlayerConnection player = new PlayerConnection(connection);
            lock (_lock) { client_list.Add(count, player); }

            Thread thread = new Thread(clientThread);
            thread.Start(count);
            count++;
        }

    }

    public static void clientThread(object threadIndex)
    {


        PlayerConnection player;
        lock (_lock) { player = client_list[(int)threadIndex]; }
        Console.WriteLine("Connection established from: {0}", player.client.Client.RemoteEndPoint);


		//client appendar en short i början av packetet och den här grejen läser av 2 bytes vilket är shorten, t.ex 3 0.
        // Layout: (short) package type, (short) package length, (byte[]) package content.
        Byte[] lengthBuffer = new Byte[sizeof(ushort)];
        Byte[] typeBuffer = new Byte[sizeof(ushort)]; //gör om skit namn
        Byte[] idBuffer = new Byte[sizeof(ushort)];
        while(true) {
            

            Byte[] bytes = new Byte[1024];
            string data = string.Empty;

            NetworkStream stream = player.client.GetStream();

            int countRead = stream.Read(typeBuffer, 0, typeBuffer.Length);
            ushort packageType = BitConverter.ToUInt16(typeBuffer, 0);
            Console.WriteLine("Package type {0}", packageType);

            if (packageType == 0) {

                ushort SendPackageType = 0;
                ushort id = Convert.ToUInt16((int)threadIndex);
                ushort packetLength = (ushort)idBuffer.Length;

                List<byte> list = new List<byte>();
                list.AddRange(BitConverter.GetBytes(SendPackageType));
                list.AddRange(BitConverter.GetBytes(packetLength));
                list.AddRange(BitConverter.GetBytes(id));

                stream.Write(list.ToArray(), 0, list.Count);    
            } else {

                countRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                //if (!player.client.Client.Connected) {
                //    break;
                //}

                //if (countRead == 0) {
                //    break;
                //}

                ushort bytesToRead = BitConverter.ToUInt16(lengthBuffer, 0);

                Console.WriteLine("package from: {0}", player.client.Client.RemoteEndPoint);

                int i;
                while ((i = stream.Read(bytes, 0, Math.Min(bytesToRead, bytes.Length))) != 0)
                {
                    bytesToRead -= (ushort)i;
                    data = packageType.ToString() + System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    player.packetQueue.Enqueue(data);

                    foreach(KeyValuePair<int, PlayerConnection> pair in client_list) {
                        if (pair.Key != (int)threadIndex){
                            pair.Value.packetQueue.Enqueue(data);
                            pair.Value.writeQueue(pair.Value.client);
                        }
                    }

                }

            }

        }


        Console.WriteLine("asd");
        //Console.WriteLine("Shutting down connection to {0}", player.client.Client.RemoteEndPoint);
        //lock (_lock) client_list.Remove((int)threadIndex);
        //player.client.Client.Shutdown(SocketShutdown.Both);
        //player.client.Close();
    }
}
