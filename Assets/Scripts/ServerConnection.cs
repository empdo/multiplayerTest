using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.NetworkInformation;
using System.Linq;

using UnityEngine;
public class ServerConnection : MonoBehaviour {

    static NetworkStream stream;
    static TcpClient client;    
    public HandlePlayers handlePlayers;

    Byte[] idBuffer = new Byte[sizeof(ushort)];
    Byte[] positionBuffer = new Byte[sizeof(float)];

    ushort ownId;
    public void Start(){
        client = new TcpClient("127.0.0.1", 25250);
        stream = client.GetStream(); 

        Debug.Log("Connected!");
        SendPacket(0, new List<byte>());
    }

    void SendPacket(ushort packageType, List<byte> data) {
        List<byte> packet = new List<byte>();

        ushort packetLength = (ushort)data.Count;

        packet.AddRange(BitConverter.GetBytes(packageType));
        packet.AddRange(BitConverter.GetBytes(packetLength));
        packet.AddRange(data);
        
        stream.Write(packet.ToArray(), 0, packet.Count);
        Debug.Log("Sent package of type " +  packageType + ", with size " + packet.Count + "b");
    }

    /*
    client skickar till server med typ 0 att den finns, får tillbaka ett id med packet typ 0,
    clienten skickar packet av typ 1 för att updatera sin position
    */

    void SendVector3(ushort packageType, Vector3 data) {
        List<byte> list = new List<byte>();
        list.AddRange(BitConverter.GetBytes(data.x));
        list.AddRange(BitConverter.GetBytes(data.y));
        list.AddRange(BitConverter.GetBytes(data.z));
        SendPacket(packageType, list);
    }
    
    void ReadClientId(byte[] buffer) {
        // Read 4 bytes, save as client id
        stream.Read(buffer, 0, buffer.Length);
        ownId = BitConverter.ToUInt16(buffer, 0);

        Debug.Log("Saved client id as:" + ownId);
    }

    void ReadServerEvent(byte[] buffer) {
        // Implement server event
    }

    void ReadTick(byte[] buffer) {
        // 2; 5, 4, 1, 2, 3; 5, 3, 1, 3, 2
        
        int offset = 0;
        while(offset < buffer.Length) {
            ushort deltaType = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);
            byte[] newBuffer = buffer.Skip(offset).ToArray();
            switch (deltaType) {
                case 1:
                    offset += ReadPositionDelta(newBuffer);
                    break;
            }
        }
    }

    int ReadPositionDelta(byte[] buffer) {
        int offset = 0;
        ushort id = BitConverter.ToUInt16(buffer, offset);
        offset += sizeof(ushort);

        Vector3 deltaPosition = new Vector3();
        deltaPosition.x = BitConverter.ToSingle(buffer, offset);
        offset += sizeof(float);

        deltaPosition.y = BitConverter.ToSingle(buffer, offset);
        offset += sizeof(float);

        deltaPosition.z = BitConverter.ToSingle(buffer, offset);
        offset += sizeof(float);

        handlePlayers.UpdatePlayerPosition(id, deltaPosition);

        return offset;
    }

    void FixedUpdate() {
        while (stream.CanRead && stream.DataAvailable) {
            ProcessPackage();
        }
    }

    void ProcessPackage() {
        if (stream.CanRead & stream.DataAvailable) {
            Debug.Log("Listening for packages...");

            Byte[] bytes = new Byte[sizeof(float) *3];

            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, buffer.Length);
            ushort packageType = BitConverter.ToUInt16(buffer, 0);

            stream.Read(buffer, 0, buffer.Length);
            ushort packageLength = BitConverter.ToUInt16(buffer, 0);

            Debug.Log("Type is " + packageType + " and length is " + packageLength);

            byte[] packageContent = new byte[packageLength];
            stream.Read(packageContent, 0, packageContent.Length);

            switch(packageType) {
                case 0:
                    // Read id
                    ReadClientId(packageContent);
                    break;
                case 1:
                    // Server event
                    ReadServerEvent(packageContent); 
                    break;
                case 2:
                    ReadTick(packageContent);
                    // Tick packet (delta from last tick)
                    break;
            }
            
        }
    }


}