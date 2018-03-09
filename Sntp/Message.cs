using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sntp
{
    class Message
    {
        public readonly Field LI;
        public readonly Field VN;
        public readonly Field Mode;
        public readonly Field Stratum;
        public readonly Field Poll;
        public readonly Field Precision;
        public readonly Field RootDelay;
        public readonly Field RootDispersion;
        public readonly Field ReferenceIdentifier;
        public readonly Field ReferenceTimestamp;
        public readonly Field OriginateTimestamp;
        public readonly Field ReceiveTimestamp;
        public readonly Field TransmitTimestamp;
        public readonly Field KeyIdentifier;
        public readonly Field MessageDigest;

        private BitArray ByteArrayToBitArray(byte[] data)
        {
            var result = new BitArray(data.Length*8);
            var tmp = new bool[8];
            for (var i = 0; i < data.Length; i++)
            {
                for (var j = 0; j < 8; j++)
                    tmp[j] = (data[i] & (1 << j)) != 0;
                Array.Reverse(tmp);
                for (var j = 0; j < tmp.Length; j++)
                    result[i * 8 + j] = tmp[j];
            }
            return result;
        }

        public Message(byte[] data)
        {
            if (data.Length < 48 || data.Length > 68) throw new ArgumentException("Received data is'not SNTP message");
            var newData = ByteArrayToBitArray(data);
            LI = new Field(newData, 2, 0);
            VN = new Field(newData, 3, 2);
            Mode = new Field(newData, 3, 5);
            Stratum = new Field(newData, 8, 8);
            Poll = new Field(newData, 8, 16);
            Precision = new Field(newData, 8, 24);
            RootDelay = new Field(newData, 32, 32);
            RootDispersion = new Field(newData, 32, 64);
            ReferenceIdentifier = new Field(newData, 32, 96);
            ReferenceTimestamp = new Field(newData, 64, 128);
            OriginateTimestamp = new Field(newData, 64, 192);
            ReceiveTimestamp = new Field(newData, 64, 256);
            TransmitTimestamp = new Field(newData, 64, 320);
            if (data.Length > 48)
                KeyIdentifier = new Field(newData, 32, 384);
            if (data.Length > 52)
                MessageDigest = new Field(newData, 128, 416);
        }

        public Message(Field LI, Field VN, Field Mode, Field Stratum, Field Poll, Field Precision,
            Field RootDelay, Field RootDispersion, Field ReferenceIdentifier, Field ReferenceTimestamp,
            Field OriginateTimestamp, Field ReceiveTimestamp, Field TransmitTimestamp,
            Field KeyIdentifier=null, Field MessageDigest=null)
        {
            this.LI = LI;
            this.VN = VN;
            this.Mode = Mode;
            this.Stratum = Stratum;
            this.Poll = Poll;
            this.Precision = Precision;
            this.RootDelay = RootDelay;
            this.RootDispersion = RootDispersion;
            this.ReferenceIdentifier = ReferenceIdentifier;
            this.ReferenceTimestamp = ReferenceTimestamp;
            this.OriginateTimestamp = OriginateTimestamp;
            this.ReceiveTimestamp = ReceiveTimestamp;
            this.TransmitTimestamp = TransmitTimestamp;
            this.KeyIdentifier = KeyIdentifier;
            this.MessageDigest = MessageDigest;
        }


        private BitArray TransformValueOfField(Field field)
        {
            var result = new BitArray(field.Value.Length);
            for (var i = 0; i < field.Value.Length; i++)
                result[i] = field.Value[i];
            return result;
        }

        private List<BitArray> GetBitArraysFromAllField()
        {
            var allFields = GetAllFields();
            return allFields.Select(TransformValueOfField).ToList();
        }

        private List<Field> GetAllFields()
        {
            var allFields = new List<Field>
            {
                LI,
                VN,
                Mode,
                Stratum,
                Poll,
                Precision,
                RootDelay,
                RootDispersion,
                ReferenceIdentifier,
                ReferenceTimestamp,
                OriginateTimestamp,
                ReceiveTimestamp,
                TransmitTimestamp,
            };
            if (KeyIdentifier != null)
                allFields.Add(KeyIdentifier);
            if (MessageDigest != null)
                allFields.Add(MessageDigest);
            return allFields;
        }

        private int BitsToNumber(BitArray bits)
        {
            var result = 0;
            var maxPow = bits.Length - 1;
            for (var i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    result += (int)Math.Pow(2, maxPow - i);
            }
            return result;
        }

        private BitArray Concat(List<BitArray> bitArraysFromFields)
        {
            var result = new List<bool>();
            foreach (var valueFromField in bitArraysFromFields)
            {
                foreach (bool b in valueFromField)
                    result.Add(b);
            }
            return new BitArray(result.ToArray());
        }

        private BitArray GetSubarray(BitArray data, int start, int length = 8)
        {
            var result = new BitArray(8);
            for (var i = 0; i < length; i++)
                result[i] = data[i + start];
            return result;
        }

        public byte[] ToByteArray()
        {
            var data = new List<byte>();
            var bits = Concat(GetBitArraysFromAllField());
            for (var i = 0; i <= bits.Length; i++)
            {
                if (i != 0 && i % 8 == 0)
                    data.Add((byte)BitsToNumber(GetSubarray(bits, i - 8)));
            }
            return data.ToArray();
        }
    }
}
