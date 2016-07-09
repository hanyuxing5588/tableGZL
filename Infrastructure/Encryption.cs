using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Infrastructure
{
    /// <summary>
    /// 加密累
    /// </summary>
    public class Encryption
    {
        private const long OFFSET_4 = 4294967296;
        private const long MAXINT_4 = 2147483647;

        private const long S11 = 7;
        private const long S12 = 12;
        private const long S13 = 17;
        private const long S14 = 22;
        private const long S21 = 5;
        private const long S22 = 9;
        private const long S23 = 14;
        private const long S24 = 20;
        private const long S31 = 4;
        private const long S32 = 11;
        private const long S33 = 16;
        private const long S34 = 23;
        private const long S41 = 6;
        private const long S42 = 10;
        private const long S43 = 15;
        private const long S44 = 21;

        private long[] State = new long[5];
        private long ByteCounter;
        private byte[] ByteBuffer = new byte[64];

        
        public string RegisterA
        {
            get { return State.GetValue(1).ToString(); }
        }

        
        public string RegisterB
        {
            get { return State.GetValue(2).ToString(); }
        }

        
        public string RegisterC
        {
            get { return State.GetValue(3).ToString(); }
        }

        
        public string RegisterD
        {
            get { return State.GetValue(4).ToString(); }
        }

        //private string DigestFileToHexStr(string Filename)
        //{
        //    string z;
        //    FileStream filest=new FileStream(Filename,FileMode.Open);
        //    BinaryReader br=new BinaryReader(filest );
        //    br.Read(
        //    //Open Filename For Binary Access Read As #1;
        //    MD5Init();
        //    do while (!EOF(1))
        //    {
        //        //ByteBuffer=new UTF8Encoding(true).GetBytes()
        //        //Get #1, , ByteBuffer;
        //        if (Loc(1) < LOF(1))
        //        {
        //            ByteCounter = ByteCounter + 64;
        //            MD5Transform (ByteBuffer);
        //        }
        //    }
        //    ByteCounter = ByteCounter + (LOF(1) % 64);
        //    Close #1;
        //    MD5Final;
        //    z = GetValues();
        //    return z;
        //}

        public string DigestStrToHexStr(string SourceString)
        {
            string z;
            MD5Init();
            MD5Update(SourceString.Length, StringToArray(SourceString));
            MD5Final();
            z = GetValues();
            return z;
        }

        private byte[] StringToArray(string InString)
        {
            byte[] z;
            int i;
            byte[] bytBuffer;
            bytBuffer = new byte[InString.Length];
            for (i = 0; i < InString.Length; i++)
            {
                bytBuffer[i] = (byte)Convert.ToInt32(char.Parse(InString.Substring(i, 1)));
            }
            z = bytBuffer;
            return z;
        }

        public string GetValues()
        {
            string z;
            z = LongToString(State[1]) + LongToString(State[2]) + LongToString(State[3]) + LongToString(State[4]);
            return z;
        }

        private string LongToString(long Num)
        {
            string z;
            Byte a;
            Byte b;
            Byte c;
            Byte d;

            a = (byte)(Num & Convert.ToInt32("FF", 16));
            if (a < 16)
            {
                z = "0" + a.ToString("X");
            }
            else
            {
                z = a.ToString("X");
            }

            b = (byte)((Num & Convert.ToInt32("FF00", 16)) / 256);
            if (b < 16)
            {
                z = z + "0" + b.ToString("X");
            }
            else
            {
                z = z + b.ToString("X");
            }

            c = (byte)((Num & Convert.ToInt32("FF0000", 16)) / 65536);

            if (c < 16)
            {
                z = z + "0" + c.ToString("X");
            }
            else
            {
                z = z + c.ToString("X");
            }

            if (Num < 0)
            {
                d = (byte)(((Num & Convert.ToInt32("7F000000", 16)) / 16777216) | Convert.ToInt32("80", 16));
            }
            else
            {
                d = (byte)((Num & Convert.ToInt32("FF000000", 16)) / 16777216);
            }

            if (d < 16)
            {
                z = z + "0" + d.ToString("X");
            }
            else
            {
                z = z + d.ToString("X");
            }
            return z;

        }

        public void MD5Init()
        {
            //State = new long[5];
            ByteCounter = 0;
            State[1] = UnsignedToLong((double)1732584194);
            State[2] = UnsignedToLong(4023233418);
            State[3] = UnsignedToLong(2562383103);
            State[4] = UnsignedToLong(271733879);
        }

        public void MD5Final()
        {
            Double dblBits;

            Byte[] padding;
            long lngBytesBuffered;
            padding = new byte[72];
            padding[0] = (byte)Convert.ToInt32("80", 16);

            dblBits = ByteCounter * 8;

            // Pad out
            lngBytesBuffered = ByteCounter % 64;
            if (lngBytesBuffered <= 56)
            {
                MD5Update(56 - lngBytesBuffered, padding);
            }
            else
            {
                MD5Update(120 - ByteCounter, padding);
            }


            padding[0] = (byte)(UnsignedToLong(dblBits) & Convert.ToInt32("FF", 16));
            padding[1] = (byte)(UnsignedToLong(dblBits) / 256 & Convert.ToInt32("FF", 16));
            padding[2] = (byte)(UnsignedToLong(dblBits) / 65536 & Convert.ToInt32("FF", 16));
            padding[3] = (byte)(UnsignedToLong(dblBits) / 16777216 & Convert.ToInt32("FF", 16));
            padding[4] = 0;
            padding[5] = 0;
            padding[6] = 0;
            padding[7] = 0;

            MD5Update(8, padding);
        }

        public void MD5Update(long InputLen, byte[] InputBuffer)
        {
            int II;
            int i;
            int j;
            int K;
            long lngBufferedBytes;
            long lngBufferRemaining;
            long lngRem;

            lngBufferedBytes = ByteCounter % 64;
            lngBufferRemaining = 64 - lngBufferedBytes;
            ByteCounter = ByteCounter + InputLen;
            // Use up old buffer results first
            if (InputLen >= lngBufferRemaining)
            {
                for (II = 0; II < lngBufferRemaining; II++)
                {
                    ByteBuffer[lngBufferedBytes + II] = InputBuffer[II];
                }
                MD5Transform(ByteBuffer);

                lngRem = (InputLen) % 64;
                // The transfer is a multiple of 64 lets do some transformations
                for (i = (int)lngBufferRemaining; i <= InputLen - II - lngRem; i = i + 64)
                {
                    for (j = 0; j <= 63; j++)
                    {
                        ByteBuffer[j] = InputBuffer[i + j];
                    }
                    MD5Transform(ByteBuffer);
                }
                lngBufferedBytes = 0;
            }
            else
            {
                i = 0;
            }
            //ByteBuffer = new byte[64];
            // Buffer any remaining input
            for (K = 0; K < InputLen - i; K++)
            {
                ByteBuffer[lngBufferedBytes + K] = InputBuffer[i + K];
            }

        }

        private void MD5Transform(byte[] Buffer)
        {
            long[] x;
            long a;
            long b;
            long c;
            long d;

            a = State[1];
            b = State[2];
            c = State[3];
            d = State[4];
            x = new long[72];
            Decode(64, x, Buffer);

            // Round 1
            a = FF(a, b, c, d, x[0], S11, -680876936);
            d = FF(d, a, b, c, x[1], S12, -389564586);
            c = FF(c, d, a, b, x[2], S13, 606105819);
            b = FF(b, c, d, a, x[3], S14, -1044525330);
            a = FF(a, b, c, d, x[4], S11, -176418897);
            d = FF(d, a, b, c, x[5], S12, 1200080426);
            c = FF(c, d, a, b, x[6], S13, -1473231341);
            b = FF(b, c, d, a, x[7], S14, -45705983);
            a = FF(a, b, c, d, x[8], S11, 1770035416);
            d = FF(d, a, b, c, x[9], S12, -1958414417);
            c = FF(c, d, a, b, x[10], S13, -42063);
            b = FF(b, c, d, a, x[11], S14, -1990404162);
            a = FF(a, b, c, d, x[12], S11, 1804603682);
            d = FF(d, a, b, c, x[13], S12, -40341101);
            c = FF(c, d, a, b, x[14], S13, -1502002290);
            b = FF(b, c, d, a, x[15], S14, 1236535329);

            // Round 2
            a = GG(a, b, c, d, x[1], S21, -165796510);
            d = GG(d, a, b, c, x[6], S22, -1069501632);
            c = GG(c, d, a, b, x[11], S23, 643717713);
            b = GG(b, c, d, a, x[0], S24, -373897302);
            a = GG(a, b, c, d, x[5], S21, -701558691);
            d = GG(d, a, b, c, x[10], S22, 38016083);
            c = GG(c, d, a, b, x[15], S23, -660478335);
            b = GG(b, c, d, a, x[4], S24, -405537848);
            a = GG(a, b, c, d, x[9], S21, 568446438);
            d = GG(d, a, b, c, x[14], S22, -1019803690);
            c = GG(c, d, a, b, x[3], S23, -187363961);
            b = GG(b, c, d, a, x[8], S24, 1163531501);
            a = GG(a, b, c, d, x[13], S21, -1444681467);
            d = GG(d, a, b, c, x[2], S22, -51403784);
            c = GG(c, d, a, b, x[7], S23, 1735328473);
            b = GG(b, c, d, a, x[12], S24, -1926607734);

            // Round 3
            a = HH(a, b, c, d, x[5], S31, -378558);
            d = HH(d, a, b, c, x[8], S32, -2022574463);
            c = HH(c, d, a, b, x[11], S33, 1839030562);
            b = HH(b, c, d, a, x[14], S34, -35309556);
            a = HH(a, b, c, d, x[1], S31, -1530992060);
            d = HH(d, a, b, c, x[4], S32, 1272893353);
            c = HH(c, d, a, b, x[7], S33, -155497632);
            b = HH(b, c, d, a, x[10], S34, -1094730640);
            a = HH(a, b, c, d, x[13], S31, 681279174);
            d = HH(d, a, b, c, x[0], S32, -358537222);
            c = HH(c, d, a, b, x[3], S33, -722521979);
            b = HH(b, c, d, a, x[6], S34, 76029189);
            a = HH(a, b, c, d, x[9], S31, -640364487);
            d = HH(d, a, b, c, x[12], S32, -421815835);
            c = HH(c, d, a, b, x[15], S33, 530742520);
            b = HH(b, c, d, a, x[2], S34, -995338651);

            // Round 4
            a = II(a, b, c, d, x[0], S41, -198630844);
            d = II(d, a, b, c, x[7], S42, 1126891415);
            c = II(c, d, a, b, x[14], S43, -1416354905);
            b = II(b, c, d, a, x[5], S44, -57434055);
            a = II(a, b, c, d, x[12], S41, 1700485571);
            d = II(d, a, b, c, x[3], S42, -1894986606);
            c = II(c, d, a, b, x[10], S43, -1051523);
            b = II(b, c, d, a, x[1], S44, -2054922799);
            a = II(a, b, c, d, x[8], S41, 1873313359);
            d = II(d, a, b, c, x[15], S42, -30611744);
            c = II(c, d, a, b, x[6], S43, -1560198380);
            b = II(b, c, d, a, x[13], S44, 1309151649);
            a = II(a, b, c, d, x[4], S41, -145523070);
            d = II(d, a, b, c, x[11], S42, -1120210379);
            c = II(c, d, a, b, x[2], S43, 718787259);
            b = II(b, c, d, a, x[9], S44, -343485551);


            State[1] = LongOverflowAdd(State[1], a);
            State[2] = LongOverflowAdd(State[2], b);
            State[3] = LongOverflowAdd(State[3], c);
            State[4] = LongOverflowAdd(State[4], d);

        }

        private void Decode(int Length, long[] OutputBuffer, byte[] InputBuffer)
        {
            int intDblIndex;
            int intByteIndex;
            double dblSum;

            intDblIndex = 0;
            for (intByteIndex = 0; intByteIndex < Length; intByteIndex = intByteIndex + 4)
            {
                dblSum = InputBuffer[intByteIndex] + InputBuffer[intByteIndex + 1] * (double)256 + InputBuffer[intByteIndex + 2] * (double)65536 + InputBuffer[intByteIndex + 3] * (double)16777216;
                OutputBuffer[intDblIndex] = UnsignedToLong(dblSum);
                intDblIndex = intDblIndex + 1;
            }
        }

        private long FF(long a, long b, long c, long d, long x, long s, long ac)
        {
            a = LongOverflowAdd4(a, (b & c) | (~(b) & d), x, ac);
            a = LongLeftRotate(a, s);
            a = LongOverflowAdd(a, b);
            return a;
        }

        private long GG(long a, long b, long c, long d, long x, long s, long ac)
        {
            a = LongOverflowAdd4(a, (b & d) | (c & ~(d)), x, ac);
            a = LongLeftRotate(a, s);
            a = LongOverflowAdd(a, b);
            return a;
        }

        private long HH(long a, long b, long c, long d, long x, long s, long ac)
        {
            a = LongOverflowAdd4(a, b ^ c ^ d, x, ac);
            a = LongLeftRotate(a, s);
            a = LongOverflowAdd(a, b);
            return a;
        }

        private long II(long a, long b, long c, long d, long x, long s, long ac)
        {
            a = LongOverflowAdd4(a, c ^ (b | ~(d)), x, ac);
            a = LongLeftRotate(a, s);
            a = LongOverflowAdd(a, b);
            return a;
        }

        // Rotate a long to the right

        private long LongLeftRotate(long Value, long bits)
        {
            long z;
            int ls1;
            long lngSign;
            long lngI;
            bits = bits % 32;
            if (bits == 0)
            {
                z = Value;
                return z;
            }
            for (lngI = 1; lngI <= bits; lngI++)
            {

                lngSign = Value & Convert.ToInt32("C0000000", 16);
                Value = (Value & Convert.ToInt32("3FFFFFFF", 16)) * 2;
                if (lngSign < 0)
                {
                    ls1 = 1;
                }
                else
                {
                    ls1 = 0;
                }
                if ((lngSign & Convert.ToInt32("40000000", 16)) == 0)
                {
                    Value = Value | ls1 | 0;
                }
                else
                {
                    Value = Value | ls1 | Convert.ToInt32("80000000", 16);
                }

            }
            z = Value;
            return z;
        }

        private long LongOverflowAdd(long Val1, long Val2)
        {
            long z;
            long lngHighWord;
            long lngLowWord;
            long lngOverflow;

            lngLowWord = (Val1 & Convert.ToInt32("FFFF", 16)) + (Val2 & Convert.ToInt32("FFFF", 16));
            lngOverflow = lngLowWord / 65536;
            lngHighWord = (((Val1 & Convert.ToInt32("FFFF0000", 16)) / 65536) + ((Val2 & Convert.ToInt32("FFFF0000", 16)) / 65536) + lngOverflow) & Convert.ToInt32("FFFF", 16);
            z = UnsignedToLong((lngHighWord * 65536) + (lngLowWord & Convert.ToInt32("FFFF", 16)));
            return z;
        }

        private long LongOverflowAdd4(long Val1, long Val2, long val3, long val4)
        {
            long z;
            long lngHighWord;
            long lngLowWord;
            long lngOverflow;

            lngLowWord = (Val1 & Convert.ToInt32("FFFF", 16)) + (Val2 & Convert.ToInt32("FFFF", 16)) + (val3 & Convert.ToInt32("FFFF", 16)) + (val4 & Convert.ToInt32("FFFF", 16));
            lngOverflow = lngLowWord / 65536;
            lngHighWord = (((Val1 & Convert.ToInt32("FFFF0000", 16)) / 65536) + ((Val2 & Convert.ToInt32("FFFF0000", 16)) / 65536) + ((val3 & Convert.ToInt32("FFFF0000", 16)) / 65536) + ((val4 & Convert.ToInt32("FFFF0000", 16)) / 65536) + lngOverflow) & Convert.ToInt32("FFFF", 16);
            z = UnsignedToLong((lngHighWord * (double)65536) + (lngLowWord & Convert.ToInt32("FFFF", 16)));
            return z;
        }

        private long UnsignedToLong(double Value)
        {
            long z;
            if (Value < 0 || Value >= OFFSET_4)
            {
                //Error 6; // Overflow
            }
            if (Value <= MAXINT_4)
            {
                z = (long)Value;
            }
            else
            {
                z = (long)(Value - OFFSET_4);
            }
            return z;
        }

        private double LongToUnsigned(long Value)
        {
            double z;
            if (Value < 0)
                z = Value + OFFSET_4;
            else
            {
                z = Value;
            }
            return z;
        }



    }


}
