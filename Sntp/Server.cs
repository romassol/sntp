using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sntp
{
    class Server
    {
        private readonly UdpClient udpClient;
        private IPEndPoint client;
        private DateTime timeOfLastReceive;

        public Server()
        {
            try
            {
                udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123));
                client = new IPEndPoint(IPAddress.Any, 0);
            }
            catch (SocketException e)
            {
                Console.WriteLine("123 port is already busy");
                Environment.Exit(e.ErrorCode);
            }

        }

        public void Start()
        {
            while (true)
            {
                var data = udpClient.Receive(ref client);
                timeOfLastReceive = DateTime.UtcNow;
                Console.WriteLine("Got data");
                var receivedMessage = new Message(data);
                var reply = GetReplyMessage(receivedMessage).ToByteArray();
                udpClient.Send(reply, reply.Length, client);
                Console.WriteLine("Sent reply to user");
            }
        }

        private BitArray NumberToBitArray(ulong number, int length)
        {
            var result = GetBoolFromNumber(number);
            AddInsignificantRightZeros(length, result);
            return new BitArray(result.ToArray().Reverse().ToArray());
        }

        private static List<bool> GetBoolFromNumber(ulong number)
        {
            var result = new List<bool>();
            if (number == 0)
                result.Add(false);
            while (number != 0)
            {
                result.Add(number % 2 != 0);
                number /= 2;
            }
            return result;
        }

        private static void AddInsignificantRightZeros(int length, List<bool> result)
        {
            while (result.Count < length)
                result.Add(false);
        }

        private TimeSpan GetTimeDelay(string fileName)
        {
            var seconds = int.Parse(File.ReadAllLines(fileName)[0]);
            return new TimeSpan(0, 0, 0, seconds);
        }

        private BitArray GetTimeStamp()
        {
            var time = timeOfLastReceive - new DateTime(1900, 1, 1) + GetTimeDelay("config.txt");

            var seconds = (uint)time.TotalSeconds;
            var milliseconds = (uint)time.Milliseconds;

            var result = new BitArray(64);
            var secondsToBitArray = NumberToBitArray(seconds, 32);

            var tmp = GetBoolFromNumber(milliseconds);
            tmp.Reverse();
            AddInsignificantRightZeros(32, tmp);
            var millisecondsToBitArray = new BitArray(tmp.ToArray());

            WriteBitsToDataWithStart(secondsToBitArray, result, 0);
            WriteBitsToDataWithStart(millisecondsToBitArray, result, 32);
            return result;
        }

        private static void WriteBitsToDataWithStart(BitArray arrayForCopy, BitArray data, int start)
        {
            for (var i = 0; i < arrayForCopy.Length; i++)
                data[i + start] = arrayForCopy[i];
        }

        private Message GetReplyMessage(Message receivedMessage)
        {
            var LI = new Field(NumberToBitArray(0, 2));
            var mode = new Field(NumberToBitArray(4, 3));
            var stratum = new Field(NumberToBitArray(1, 8));
            var precision = new Field(NumberToBitArray(0xe9, 8));
            var rootDelay = new Field(NumberToBitArray(0, 32));
            var rootDispersion = new Field(NumberToBitArray(0, 32));
            var id = Encoding.UTF8.GetBytes("LOCL");
            var referenceIdentifier = new Field(new BitArray(id));
            var timestamp = GetTimeStamp();
            var referenceTimestamp = new Field(timestamp);
            var receiveTimestamp = new Field(timestamp);
            var transmitTimestamp = new Field(timestamp);
            var replyMessage = new Message(
                LI, receivedMessage.VN, mode, stratum,
                receivedMessage.Poll, precision, rootDelay,
                rootDispersion, referenceIdentifier, referenceTimestamp,
                receivedMessage.TransmitTimestamp, receiveTimestamp, transmitTimestamp
            );
            return replyMessage;
        }
    }
}