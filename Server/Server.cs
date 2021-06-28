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

	public static void SendPlayerID(Player player, int id)
	{
		lock (player)
		{
			player.id = Convert.ToUInt16(id);
			player.QueuePacket(0, BitConverter.GetBytes(id));
		}
	}

	public void SendPositionDelta()
	{
		//TODO: gör dehär    
	}



	static void HandlePlayerDeltaPacket(Player player, byte[] deltaPacket)
	{

		/*
		
		[[1, 1,2,3], [2, 1,2,3], [3, 1,2,3]]
		  ^    ^
		 type   vec3 

		type = 1 : position packet (det enda som behöver hanteras)

        TODO: hanterar bara en grej
        1. gör om till lista med packet, anta storlek från typ av värden i listan
        2. switch kolla första shorten
        3. köra rätt funktion beroende på första shorten
		*/
		
		if (deltaPacket.Length < sizeof(ushort) + (sizeof(float) * 3)) { 
			return;
		}

		int offset = 0;
        while (offset < deltaPacket.Length) {

            ushort type = BitConverter.ToUInt16(deltaPacket, offset);
            offset += sizeof(ushort);
            float dx = BitConverter.ToSingle(deltaPacket, offset);
            offset += sizeof(float);
            float dy = BitConverter.ToSingle(deltaPacket, offset);
            offset += sizeof(float);
            float dz = BitConverter.ToSingle(deltaPacket, offset);
            offset += sizeof(float);

            switch (type) {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    lock (player)
                    {
                        player.UpdatePositionFromDelta(dx, dy, dz);
                    }
                    break;
            }


        }
	}
	public static void clientThread(object threadIndex)
	{


		GameServer.Player player;
		lock (_lock) { player = client_list[(int)threadIndex]; }
		Console.WriteLine("Connection established from: {0}", player.client.Client.RemoteEndPoint);
		NetworkStream stream = player.client.GetStream();

		while (player.client.Client.Connected)
		{

			// 3, 4, 0, 0, 0, 0

			byte[] buffer = new byte[2];
			stream.Read(buffer, 0, buffer.Length);
			ushort packageType = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, buffer.Length);
			ushort packageLength = BitConverter.ToUInt16(buffer, 0);

			Console.WriteLine("Type is " + packageType + " and length is " + packageLength);

			byte[] packageContent = new byte[packageLength];
			stream.Read(packageContent, 0, packageContent.Length);

			switch (packageType)
			{
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


			}

            foreach (byte[] packet in player.packetQueue)
            {
                stream.Write(packet);
            }

            player.packetQueue.Clear();

			//Console.WriteLine("Shutting down connection to {0}", player.client.Client.RemoteEndPoint);
			//lock (_lock) client_list.Remove((int)threadIndex);
			//player.client.Client.Shutdown(SocketShutdown.Both);
			//player.client.Close();
		}
	}
}
