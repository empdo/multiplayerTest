using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace GameServer {
    public class Player {

        public Queue<byte[]> packetQueue = new Queue<byte[]>();
        public List<byte> deltaPacket = new List<byte>();
        public TcpClient client;

        public float x, y, z;

        public ushort id;
        public Player(TcpClient client){
            this.client = client; 
        }

        public byte[] GetTick() {
            return new byte[1]; 
        }

        public void UpdatePositionFromDelta(float[] position) {
            UpdatePositionFromDelta(position[0], position[1], position[2]);
        }

        public void UpdatePositionFromDelta(float dx, float dy, float dz) {
            x += dx;
            y += dy;
            z += dz;
        }

        public void QueuePacket(ushort packageType, List<byte> data) {
            QueuePacket(packageType, data.ToArray());
        }

        public void QueuePacket(ushort packageType, byte[] data) {
            List<byte> packet = new List<byte>();

            ushort packetLength = (ushort)data.Length;

            packet.AddRange(BitConverter.GetBytes(packageType));
            packet.AddRange(BitConverter.GetBytes(packetLength));
            packet.AddRange(data);
            
            packetQueue.Enqueue(packet.ToArray());

            Console.WriteLine("Sent package of type " +  packageType + ", with size " + packet.Count + "b");
        }


        void AddDelta(ushort deltaType, byte[] buffer) {
            deltaPacket = deltaPacket.Concat(BitConverter.GetBytes(deltaType).Concat(buffer)).ToList();
        }

        public void SendPositionDelta(byte[] positionDelta) {
            AddDelta(1, positionDelta);
        // Debug.Log("Sednig position delta" + positionDelta);
        }


    }

}