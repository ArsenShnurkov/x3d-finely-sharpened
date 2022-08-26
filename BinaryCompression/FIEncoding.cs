/*
 *  Liquid Fast Infoset - XML Compression Library
 *  Copyright © 2001-2011 Liquid Technologies Limited. All rights reserved.
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *  
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  For product and commercial licensing details please contact us:
 *  http://www.liquid-technologies.com
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LiquidTechnologies.FastInfoset
{
    /// <summary>
    ///     Abstract base class of Fast Infoset encodings <see cref="FIRestrictedAlphabet" /> and
    ///     <see cref="FIEncodingAlgorithm" />.
    /// </summary>
    public abstract class FIEncoding
    {
        /// <summary>
        ///     Creates an instance of FIEncoding.
        /// </summary>
        public FIEncoding()
        {
        }

        internal int TableIndex { get; set; }

        /// <summary>
        ///     Method used to encode the data in the derived concrete class.
        /// </summary>
        /// <param name="data">Data to encode</param>
        /// <returns>Encoded data.</returns>
        /// <remarks>The data to encode should be of a type expected by the specific derived concrete class.</remarks>
        public abstract byte[] Encode(object data);

        /// <summary>
        ///     Method used to decode the data in the derived concrete class.
        /// </summary>
        /// <param name="data">Data to decode</param>
        /// <returns>Decoded data</returns>
        /// <remarks>
        ///     The decoded data must always return as a string value as this will be read by the FIReader as if it were over
        ///     a standard XML text document. The string returned may be string representation of binary data, e.g. Hex or Base64
        ///     encoded. The client would be expected to decode to binary data as required in the same way they would if they were
        ///     reading an plain XML text document.
        /// </remarks>
        public abstract string Decode(byte[] data);
    }

    /// <summary>
    ///     Implementation of Fast Infoset Restricted Alphabet [X.891 Section 8.2].
    /// </summary>
    internal class FIRestrictedAlphabet : FIEncoding
    {
        /// <summary>
        ///     Creates an instance of FIRestrictedAlphabet using the specified characters.
        /// </summary>
        /// <param name="alphabetChars">Characters that form the Restricted Alphabet.</param>
        /// <remarks>
        ///     <para>
        ///         Each entry in the restricted alphabet table shall be an ordered set of distinct ISO/IEC 10646 characters of
        ///         any size between 2 and 220 characters.
        ///     </para>
        ///     <para>
        ///         NOTE – A restricted alphabet permits a compact encoding of any character string consisting entirely of
        ///         characters from that set, through the assignment of progressive integers to the characters in the set and the
        ///         use of those integers to encode the characters of the string [X.891 Section 7.17.6].
        ///     </para>
        /// </remarks>
        internal FIRestrictedAlphabet(string alphabetChars)
        {
            AlphabetChars = alphabetChars;
        }

        internal string AlphabetChars { get; }

        #region Overrides

        public override byte[] Encode(object data)
        {
            var strVal = data as string;

            if (string.IsNullOrEmpty(strVal))
                return null;

            var alphabetCount = AlphabetChars.Length;

            // 8.2.2
            if (alphabetCount < 2 || alphabetCount > FIConsts.TWO_POWER_TWENTY)
                throw new LtFastInfosetException(
                    "Failed to write FastInfoset. Invalid Restricted Alphabet. Alphabet must contain between 2 and 2^20 characters.");

            // see how many bits we need per char
            var bits = 2;
            while (1 << bits <= alphabetCount) bits++;

            // populate memory buffer withe encoded bytes
            var buffer = new MemoryStream();
            var len = strVal.Length;

            if (bits == 8)
            {
                // easy 1 to 1 mapping

                var n = 0;
                while (n < len)
                {
                    var c = strVal[n++];
                    var pos = AlphabetChars.IndexOf(c);
                    if (pos == -1)
                        throw new LtFastInfosetException(
                            "Failed to write FastInfoset. Character not found in Restricted Alphabet [" + c + "].");

                    buffer.WriteByte((byte)AlphabetChars[pos]);
                }
            }
            else if (bits == 4)
            {
                // semi-easy 2 to 1 mapping

                var n = 0;
                while (n < len)
                {
                    var c = strVal[n++];
                    var pos = AlphabetChars.IndexOf(c);
                    if (pos == -1)
                        throw new LtFastInfosetException(
                            "Failed to write FastInfoset. Character not found in Restricted Alphabet [" + c + "].");

                    if (n == len)
                    {
                        // fill last 4 bits with terminator value 1111
                        buffer.WriteByte((byte)((AlphabetChars[pos] << 4) | 0xF));
                    }
                    else
                    {
                        var c2 = strVal[n++];
                        var pos2 = AlphabetChars.IndexOf(c2);
                        if (pos2 == -1)
                            throw new LtFastInfosetException(
                                "Failed to write FastInfoset. Character not found in Restricted Alphabet [" + c2 +
                                "].");

                        buffer.WriteByte((byte)((AlphabetChars[pos] << 4) | AlphabetChars[pos2]));
                    }
                }
            }
            else
            {
                // tricky arbitry mapping
                throw new LtFastInfosetException(
                    "Failed to write FastInfoset. Unsupported Feature in FIRestrictedAlphabet Encode.");
            }

            return buffer.ToArray();
        }

        public override string Decode(byte[] data)
        {
            var alphabetCount = AlphabetChars.Length;

            // 8.2.2
            if (alphabetCount < 2 || alphabetCount > FIConsts.TWO_POWER_TWENTY)
                throw new LtFastInfosetException(
                    "Failed to parse FastInfoset. Invalid Encoding of Restricted Alphabet. Alphabet must contain between 2 and 2^20 characters.");

            // see how many bits we need per char
            var bits = 2;
            while (1 << bits <= alphabetCount) bits++;

            // populate string buffer withe decoded chars
            StringBuilder buffer = null;
            var len = data.Length;

            if (bits == 8)
            {
                // easy 1 to 1 mapping

                buffer = new StringBuilder(len);

                for (var n = 0; n < len; n++) buffer[n] = AlphabetChars[data[n]];
            }
            else if (bits == 4)
            {
                // semi-easy 2 to 1 mapping

                buffer = new StringBuilder(len * 2);

                var nChars = 0;
                for (var n = 0; n < len; n++)
                {
                    buffer[nChars++] = AlphabetChars[data[n] >> 4];

                    // check for terminator xxxx1111
                    if ((data[n] & 0xF) == 0xF)
                        break;

                    buffer[nChars++] = AlphabetChars[data[n] & 0xF];
                }
            }
            else
            {
                // tricky arbitry mapping

                // if we can fit an additional char in the last octet then
                // the char bits are all set to 1
                var terminator = (1 << bits) - 1;

                // see how many chars there are
                var charCount = len * 8 / bits;

                // populate buffer from stream
                buffer = new StringBuilder(charCount);

                var pos = 0;
                var bitsLen = bits;
                var bitsRemainder = 8;
                var bitsLastVal = data[pos++];

                for (var bufferOffset = 0; bufferOffset < charCount; bufferOffset++)
                {
                    int offset = bitsLastVal;
                    while (true)
                        if (bitsRemainder == bitsLen)
                        {
                            bitsRemainder = 0;
                            bitsLastVal = 0;
                            break;
                        }
                        else if (bitsRemainder > bitsLen)
                        {
                            bitsRemainder -= bitsLen;
                            bitsLastVal &= FIConsts.BIT_MASKS[bitsRemainder];
                            offset >>= bitsRemainder;
                            break;
                        }
                        else
                        {
                            offset <<= 8;
                            offset |= data[pos++];
                            bitsRemainder = 8;
                        }

                    if (offset == terminator)
                        break;

                    buffer[bufferOffset] = AlphabetChars[offset];
                }
            }

            return buffer.ToString();
        }

        #endregion
    }

    internal class InternalEncodingAlgorithm : FIEncoding
    {
        private static readonly char[] _hexLookup =
            { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        protected EncodingType _type;

        #region Constructors

        internal InternalEncodingAlgorithm()
        {
            _type = EncodingType.None;
        }

        #endregion

        internal EncodingType Encoding
        {
            get => _type;
            set
            {
                _type = value;
                TableIndex = (int)value;
            }
        }

        #region Enums

        internal enum EncodingType
        {
            None,
            HexadecimalEncoding,
            Base64Encoding,
            ShortEncoding,
            IntEncoding,
            LongEncoding,
            BooleanEncoding,
            FloatEncoding,
            DoubleEncoding,
            UUIDEncoding,
            CDataEncoding
        }

        #endregion

        #region Encode Methods

        internal static byte[] ShortEncoding(short s)
        {
            var data = new byte[2];
            data[0] = (byte)(s >> 8);
            data[1] = (byte)(s & 0xFF);
            return data;
        }

        internal static byte[] IntEncoding(int i)
        {
            var data = new byte[4];
            data[0] = (byte)(i >> 24);
            data[1] = (byte)(i >> 16);
            data[2] = (byte)(i >> 8);
            data[3] = (byte)(i & 0xFF);
            return data;
        }

        internal static byte[] LongEncoding(long l)
        {
            var data = new byte[8];
            data[0] = (byte)(l >> 56);
            data[1] = (byte)(l >> 48);
            data[2] = (byte)(l >> 40);
            data[3] = (byte)(l >> 32);
            data[4] = (byte)(l >> 24);
            data[5] = (byte)(l >> 16);
            data[6] = (byte)(l >> 8);
            data[7] = (byte)(l & 0xFF);
            return data;
        }

        internal static byte[] BooleanEncoding(bool b)
        {
            var data = new byte[1];
            // first 4 bits specify number of unused bits
            if (b)
                // 00111000
                data[0] = 0x38;
            else
                // 00110000
                data[0] = 0x30;
            return data;
        }

        internal static byte[] FloatEncoding(float f)
        {
            var data = new byte[4];
            // convert bits into an int
            var i = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
            data[0] = (byte)(i >> 24);
            data[1] = (byte)(i >> 16);
            data[2] = (byte)(i >> 8);
            data[3] = (byte)(i & 0xFF);
            return data;
        }

        internal static byte[] DoubleEncoding(double d)
        {
            var data = new byte[8];

            // convert bits into an int
            var l = BitConverter.ToInt64(BitConverter.GetBytes(d), 0);
            data[0] = (byte)(l >> 56);
            data[1] = (byte)(l >> 48);
            data[2] = (byte)(l >> 40);
            data[3] = (byte)(l >> 32);
            data[4] = (byte)(l >> 24);
            data[5] = (byte)(l >> 16);
            data[6] = (byte)(l >> 8);
            data[7] = (byte)(l & 0xFF);
            return data;
        }

        #endregion

        #region Decoder Methods

        internal static string HexDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            var a = new char[length * 2];
            for (uint i = 0; i < length; i++)
            {
                a[i * 2] = _hexLookup[(0xF0 & data[i]) >> 4];
                a[i * 2 + 1] = _hexLookup[0x0F & data[i]];
            }

            return new string(a);
        }

        internal static string ShortDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            if (length % 2 != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for SHORT byte encoding.");

            var sb = new StringBuilder();

            var dw = 0;
            while (true)
            {
                sb.Append(Convert.ToInt16((short)((data[dw++] << 8)
                                                  | data[dw++])).ToString());

                if (dw == length)
                    break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string IntDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            if (length % 4 != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for INT byte encoding.");

            var sb = new StringBuilder();

            var dw = 0;
            while (true)
            {
                sb.Append(Convert.ToInt32((data[dw++] << 24)
                                          | (data[dw++] << 16)
                                          | (data[dw++] << 8)
                                          | data[dw++]).ToString());

                if (dw == length)
                    break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string LongDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            if (length % 8 != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for LONG byte encoding.");

            var sb = new StringBuilder();

            var dw = 0;
            while (true)
            {
                var ll = (long)data[dw++] << 56;
                ll |= (long)data[dw++] << 48;
                ll |= (long)data[dw++] << 40;
                ll |= (long)data[dw++] << 32;
                ll |= (long)data[dw++] << 24;
                ll |= (long)data[dw++] << 16;
                ll |= (long)data[dw++] << 8;
                ll |= data[dw++];

                sb.Append(Convert.ToInt64(ll).ToString());

                if (dw == length)
                    break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string BooleanDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            var sb = new StringBuilder();

            var dw = 0;

            var by = data[dw++];

            // first 4 bits specify number of unused bits in last byte
            var nLastUnusedBits = (by & 0xF0) >> 4;
            var nCurrentBit = 3;
            var nUnusedBits = 0;

            while (true)
            {
                if (dw == length)
                    nUnusedBits = nLastUnusedBits;

                for (; nCurrentBit >= nUnusedBits; nCurrentBit--)
                    sb.Append(Convert.ToBoolean((by >> nCurrentBit) & 0x1) ? "true" : "false");

                if (dw == length)
                    break;

                by = data[dw++];

                sb.Append(" ");

                nCurrentBit = 7;
            }

            return sb.ToString();
        }

        internal static string FloatDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            if (length % 4 != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for FLOAT byte encoding.");

            var sb = new StringBuilder();

            var dw = 0;
            while (true)
            {
                // get float value into an int
                var n = (data[dw++] << 24)
                        | (data[dw++] << 16)
                        | (data[dw++] << 8)
                        | data[dw++];

                // convert bits to float
                sb.Append(BitConverter.ToSingle(BitConverter.GetBytes(n), 0).ToString(NumberFormatInfo.InvariantInfo));

                if (dw == length)
                    break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string DoubleDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            if (length % 8 != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for DOUBLE byte encoding.");

            var sb = new StringBuilder();

            var dw = 0;
            while (true)
            {
                var ll = (long)data[dw++] << 56;
                ll |= (long)data[dw++] << 48;
                ll |= (long)data[dw++] << 40;
                ll |= (long)data[dw++] << 32;
                ll |= (long)data[dw++] << 24;
                ll |= (long)data[dw++] << 16;
                ll |= (long)data[dw++] << 8;
                ll |= data[dw++];

                // cast bits to double
                sb.Append(BitConverter.ToDouble(BitConverter.GetBytes(ll), 0).ToString(NumberFormatInfo.InvariantInfo));

                if (dw == length)
                    break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string UUIDDecoding(byte[] data)
        {
            if (data == null)
                return string.Empty;

            var length = data.Length;

            if (length % 16 != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for UUID byte encoding.");

            var sb = new StringBuilder();

            var tempBuffer = new byte[16];
            var dw = 0;
            while (true)
            {
                Buffer.BlockCopy(data, dw, tempBuffer, 0, 16);
                sb.Append(HexDecoding(tempBuffer));
                dw += 16;

                if (dw == length)
                    break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        #endregion

        #region Overrides

        public override byte[] Encode(object val)
        {
            byte[] data = null;

            if (val != null)
                switch (_type)
                {
                    case EncodingType.HexadecimalEncoding:
                        data = (byte[])val;
                        break;
                    case EncodingType.Base64Encoding:
                        data = (byte[])val;
                        break;
                    case EncodingType.ShortEncoding:
                        data = ShortEncoding((short)val);
                        break;
                    case EncodingType.IntEncoding:
                        data = IntEncoding((int)val);
                        break;
                    case EncodingType.LongEncoding:
                        data = LongEncoding((long)val);
                        break;
                    case EncodingType.BooleanEncoding:
                        data = BooleanEncoding((bool)val);
                        break;
                    case EncodingType.FloatEncoding:
                        data = FloatEncoding((float)val);
                        break;
                    case EncodingType.DoubleEncoding:
                        data = DoubleEncoding((double)val);
                        break;
                    case EncodingType.UUIDEncoding:
                        data = (byte[])val;
                        break;
                    case EncodingType.CDataEncoding:
                        data = System.Text.Encoding.UTF8.GetBytes((string)val);
                        break;
                    default:
                        throw new LtFastInfosetException("Unknown Encoding");
                }

            return data;
        }

        public override string Decode(byte[] data)
        {
            string val = null;

            switch (_type)
            {
                case EncodingType.HexadecimalEncoding:
                    val = HexDecoding(data);
                    break;
                case EncodingType.Base64Encoding:
                    val = Convert.ToBase64String(data);
                    break;
                case EncodingType.ShortEncoding:
                    val = ShortDecoding(data);
                    break;
                case EncodingType.IntEncoding:
                    val = IntDecoding(data);
                    break;
                case EncodingType.LongEncoding:
                    val = LongDecoding(data);
                    break;
                case EncodingType.BooleanEncoding:
                    val = BooleanDecoding(data);
                    break;
                case EncodingType.FloatEncoding:
                    val = FloatDecoding(data);
                    break;
                case EncodingType.DoubleEncoding:
                    val = DoubleDecoding(data);
                    break;
                case EncodingType.UUIDEncoding:
                    val = UUIDDecoding(data);
                    break;
                case EncodingType.CDataEncoding:
                    // GetString(data) doesn't exist in WindowsCE
                    val = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    break;
                default:
                    throw new LtFastInfosetException("Unknown Encoding");
            }

            return val;
        }

        #endregion
    }

    /// <summary>
    ///     Base class for implementing custom Encoding Algorithms [X.891 Section 8.3].
    /// </summary>
    public abstract class FIEncodingAlgorithm : FIEncoding
    {
        /// <summary>
        ///     Creats an instance of FIEncodingAlgorithm using the specified URI.
        /// </summary>
        /// <param name="uri">URI used as the unique identifier for this encoding.</param>
        /// <remarks>
        ///     <para>
        ///         Each entry in this table specifies the encoding of a character string with some defined characteristics into
        ///         an octet string [X.891 Section 7.17.7].
        ///     </para>
        ///     <para>
        ///         NOTE – The defined characteristics may refer to the length of the string, to the characters appearing in it,
        ///         or to an arbitrarily complex pattern of characters.  In general, a given encoding algorithm applies only to a
        ///         special and defined subset of the ISO/IEC 10646 character strings.
        ///     </para>
        /// </remarks>
        public FIEncodingAlgorithm(Uri uri, int index = -1)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            URI = uri;
            Index = index;
        }

        /// <summary>
        ///     Uniquely identifies this Encoding Algorithm
        /// </summary>
        public Uri URI { get; }

        public int Index { get; set; }
    }
}