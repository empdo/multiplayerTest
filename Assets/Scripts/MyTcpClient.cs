using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Net.NetworkInformation;
using System.Linq;

using UnityEngine;
public class MyTcpClient : MonoBehaviour {

    static object _lock = new object();
    static NetworkStream stream;
    static NetworkStream reciveStream;
    static TcpClient client;
    
    public Rigidbody rb;

    public HandllePlayes handllePlayes;

    void Start() {

       rb = GetComponent<Rigidbody>(); 

    }
    public void Main(){

        Console.CancelKeyPress += new ConsoleCancelEventHandler(InteruptHandler);

        lock (_lock) {client = new TcpClient("127.0.0.1", 25250);};
        lock (_lock) {stream = client.GetStream();};


        Vector3 position = rb.position;
        ushort packageType = 0;

        byte[] buffer = new byte[sizeof(float)*3];

        position.x = BitConverter.ToSingle(buffer,0*sizeof(float));
        position.y = BitConverter.ToSingle(buffer,1*sizeof(float));
        position.z = BitConverter.ToSingle(buffer,2*sizeof(float));

        ushort packetLength = (ushort)buffer.Length;

        List<byte> list = new List<byte>();
        list.AddRange(BitConverter.GetBytes(packageType));
        list.AddRange(BitConverter.GetBytes(packetLength));
        list.AddRange(buffer);

        
        lock (_lock) {stream.Write(list.ToArray(), 0, list.Count);};


        Thread thread = new Thread(Send);
        Thread thread1 =  new Thread(Recive);


        //vänta på en connecta sen starta threads istället
        thread.Start();
        thread1.Start();
    }

    public void Recive() {
        lock (_lock) {                
           reciveStream = client.GetStream(); 
        };

        Byte[] lengthBuffer = new Byte[sizeof(ushort)];
        Byte[] typeBuffer = new Byte[sizeof(ushort)]; //gör om skit namn
        Byte[] idBuffer = new Byte[sizeof(ushort)];
        Byte[] positionBuffer = new Byte[sizeof(float)];
        while(true) {

            Byte[] bytes = new Byte[sizeof(float) *3];
            Dictionary<int, string> data = new Dictionary<int, string>();

            int countRead;
            lock (_lock) {
                countRead = reciveStream.Read(typeBuffer, 0, typeBuffer.Length);
            };

            ushort packageType = BitConverter.ToUInt16(typeBuffer, 0);

            countRead = reciveStream.Read(lengthBuffer, 0, lengthBuffer.Length);

            if (countRead == 0) {
                break;
            }

            if (countRead < lengthBuffer.Length)
            {
                throw new InvalidOperationException("packet to short");
            }

            ushort bytesToRead = BitConverter.ToUInt16(lengthBuffer, 0);

            
            countRead = reciveStream.Read(idBuffer, 0, idBuffer.Length);

            ushort id = BitConverter.ToUInt16(idBuffer, 0);


    //            Console.WriteLine("package from: {0}", player.client.Client.RemoteEndPoint);

            int i;
            while ((i = stream.Read(bytes, 0, Math.Min(bytesToRead, bytes.Length))) != 0)
            {
                bytesToRead -= (ushort)i;
            }

            Vector3 position = new Vector3();

            position.x = Convert.ToSingle(BitConverter.ToDouble(bytes.Skip(0).Take(positionBuffer.Length).ToArray(), 0));
            position.y = Convert.ToSingle(BitConverter.ToDouble(bytes.Skip(positionBuffer.Length).Take(positionBuffer.Length *2).ToArray(), 0));
            position.z = Convert.ToSingle(BitConverter.ToDouble(bytes.Skip(positionBuffer.Length *2).Take(positionBuffer.Length *3).ToArray(), 0));

            if (packageType == 0) {                    
                handllePlayes.initPlayers(id, position);
            } else if (packageType == 1) {
                handllePlayes.updatePosition(id, position);
            } else if (packageType == 2) {
                handllePlayes.removePlayer(id);
            }

        }
    }

    //göra två threads en för att skicka och en för att ta emot
    public void Send() {

        try {
            while(true) {

                lock (_lock) {
                    if (!client.Client.Connected) {
                        break;
                    }
                };

                Vector3 position = rb.position;

                byte[] buffer = new byte[sizeof(float)*3];

                position.x = BitConverter.ToSingle(buffer,0*sizeof(float));
                position.y = BitConverter.ToSingle(buffer,1*sizeof(float));
                position.z = BitConverter.ToSingle(buffer,2*sizeof(float));

                ushort packageType = 0;
                ushort packetLength = (ushort)buffer.Length;

                List<byte> list = new List<byte>();
                list.AddRange(BitConverter.GetBytes(packageType));
                list.AddRange(BitConverter.GetBytes(packetLength));
                list.AddRange(buffer);

                
                lock (_lock) {stream.Write(list.ToArray(), 0, list.Count);};
                

                Console.WriteLine("Sent: {0} \n", string.Join(", ", list));
            }

            lock (_lock) {
                client.Client.Shutdown(SocketShutdown.Send);
            };

            lock (_lock) {stream.Close();};
            lock (_lock) {client.Close();};

        } catch (ArgumentNullException e) {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }

    protected static void InteruptHandler(object sender, ConsoleCancelEventArgs args) {
            Console.WriteLine("Shutting down...");

            args.Cancel = true;

            lock (_lock) {
                client.Client.Shutdown(SocketShutdown.Send);
            };

            lock (_lock) {stream.Close();};
            lock (_lock) {client.Close();};

            System.Environment.Exit(1);  
            
    }
}