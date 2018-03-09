using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sntp
{
    class Field
    {
        public readonly BitArray Value;

        public Field(BitArray data, int length, int startIndex)
        {
            Value = new BitArray(length);
            for (var i = 0; i < length; i++)
                Value[i] = data[startIndex + i];
        }

        public Field(BitArray value)
        {
            Value = value;
        }
    }
}
