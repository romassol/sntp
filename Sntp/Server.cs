using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Sntp
{
    class Server
    {
        private readonly UdpClient udpClient;
        private IPEndPoint client;
        private DateTime timeOfLastReceive;

        public Server()
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123));
            client = new IPEndPoint(IPAddress.Any, 0);
        }

        public void Start()
        {
            var data = udpClient.Receive(ref client);
            timeOfLastReceive = DateTime.Now;
            Console.WriteLine("Got data");
            var receivedMessage = new Message(data);
            var reply = GetReplyMessage(receivedMessage).ToByteArray();
            udpClient.Send(reply, reply.Length, client);
            Console.WriteLine("Sent reply to user");
        }

        private BitArray NumberToBitArray(ulong number, int length)
        {
            var result = new List<bool>();
            if(number == 0)
                result.Add(false);
            while (number != 0)
            {
                result.Add(number%2 != 0);
                number /= 2;
            }
            while (result.Count<length)
                result.Add(false);
            return new BitArray(result.ToArray().Reverse().ToArray());
        }

        private BitArray GetTimeStamp()
        {
            Console.WriteLine(timeOfLastReceive);
            var time = timeOfLastReceive - new DateTime(1900, 1, 1);
            //Console.WriteLine(time.);
            var seconds = time.TotalSeconds;
            Console.WriteLine(seconds);
            Console.WriteLine((ulong)seconds);
            var milliseconds = time.Milliseconds;
            Console.WriteLine((ulong)milliseconds);
            var result = new BitArray(64);
            var secondsToBitArray = NumberToBitArray((ulong)seconds, 32);
            foreach (bool b in secondsToBitArray)
            {
                Console.Write(b ? 1 : 0);
            }
            Console.WriteLine();
            var millisecondsToBitArray = NumberToBitArray((ulong)milliseconds, 32);
            WriteBitsToDataWithStart(secondsToBitArray, result, 0);
            WriteBitsToDataWithStart(millisecondsToBitArray, result, 32);
            foreach (bool b in result)
            {
                Console.Write(b ? 1 : 0);
            }
            Console.WriteLine();
            return result;
        }

        private static void WriteBitsToDataWithStart(BitArray arrayForCopy, BitArray data, int start)
        {
            for (var i = 0; i < arrayForCopy.Length; i++)
                data[i+start] = arrayForCopy[i];
        }

        private Message GetReplyMessage(Message receivedMessage)
        {
            var LI = new Field(NumberToBitArray(0, 2));
            var mode = new Field(NumberToBitArray(4, 3));
            var stratum = new Field(NumberToBitArray(1, 8));
            var precision = new Field(NumberToBitArray(0, 8));
            var rootDelay = new Field(NumberToBitArray(0, 32));
            var rootDispersion = new Field(NumberToBitArray(0, 32));
            var referenceIdentifier = new Field(NumberToBitArray(0, 32));
            var timestamp = GetTimeStamp();
            var referenceTimestamp = new Field(timestamp);
            var receiveTimestamp = new Field(timestamp);
            var transmitTimestamp = new Field(timestamp);
            foreach (bool b in referenceTimestamp.Value)
            {
                Console.Write(b ? 1 : 0);
            }
            //var keyIdentifier = new Field(NumberToBitArray(0,32));
            //var messageDigest = new Field(NumberToBitArray(0, 128));
            var replyMessage = new Message(
                LI, receivedMessage.VN, mode, stratum, receivedMessage.Poll, precision,
                rootDelay, rootDispersion, referenceIdentifier, referenceTimestamp,
                receivedMessage.TransmitTimestamp, receiveTimestamp, transmitTimestamp//,
                //keyIdentifier, messageDigest
            );
            return replyMessage;
        }
    }
}