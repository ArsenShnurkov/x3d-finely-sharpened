using System;
using System.IO;
using System.IO.Compression;
using LiquidTechnologies.FastInfoset;

namespace X3D.Parser.X3DB
{
    // TODO: implement the following X3DB Encoders
    // http://www.web3d.org/documents/specifications/19776-3/V3.3/Part03/tables.html#t-EncodingAlgorithmURI


    /// <summary>
    ///     This encoder takes an array of float values and encodes them as a quantized form
    ///     of the single precision floating point numbers defined in IEC 60559.
    /// </summary>
    public class QuantizedzlibFloatArrayEncoder : FIEncodingAlgorithm
    {
        /*
A custom value is selected for the exponent, 
mantissa and sign bits typically used to encode floats. 
The bias of 127 is still used. 
These values are written to the stream first before the data. 
The number of exponent bits is encoded using three bits, 
giving a range of 1-8, zero exponent bits cannot be used. 
The number of mantissa bits is encoded using five bits. 
One bit is used for the sign bit. 

When a number uses zero sign bits, it is assumed to be positive. 
The seven remaining bits are padded with '0000000' (padding) are appended to the bit stream.

The number of float values in the field is appended to the stream using four bytes. 
The float values themselves are then encoded using the zlib library (see 2.[RFC1950]) 
and appended to the stream.

This technique is a lossy encoder. 
 * */

        public QuantizedzlibFloatArrayEncoder() : base(
            new Uri("encoder://web3d.org/QuantizedzlibFloatArrayEncoder"), 34)
        {
        }

        public override string Decode(byte[] data)
        {
            string result;

            using (var stream = new MemoryStream(data, 2, data.Length - 2))
            using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(inflater))
            {
                result = streamReader.ReadToEnd();
            }

            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class QuantizedFloatArrayEncoder : FIEncodingAlgorithm
    {
        public QuantizedFloatArrayEncoder() : base(new Uri("encoder://web3d.org/QuantizedFloatArrayEncoder"), 32)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class DeltazlibIntArrayEncoder : FIEncodingAlgorithm
    {
        public DeltazlibIntArrayEncoder() : base(new Uri("encoder://web3d.org/DeltazlibIntArrayEncoder"), 33)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class zlibFloatArrayEncoder : FIEncodingAlgorithm
    {
        public zlibFloatArrayEncoder() : base(new Uri("encoder://web3d.org/zlibFloatArrayEncoder"), 35)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class QuantizedDoubleArrayEncoder : FIEncodingAlgorithm
    {
        public QuantizedDoubleArrayEncoder() : base(new Uri("encoder://web3d.org/QuantizedDoubleArrayEncoder"), 36)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class zlibDoubleArrayEncoder : FIEncodingAlgorithm
    {
        public zlibDoubleArrayEncoder() : base(new Uri("encoder://web3d.org/zlibDoubleArrayEncoder"), 37)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class QuantizedzlibDoubleArrayEncoder : FIEncodingAlgorithm
    {
        public QuantizedzlibDoubleArrayEncoder() : base(new Uri("encoder://web3d.org/QuantizedzlibDoubleArrayEncoder"),
            38)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }

    public class RangeIntArrayEncoder : FIEncodingAlgorithm
    {
        public RangeIntArrayEncoder() : base(new Uri("encoder://web3d.org/RangeIntArrayEncoder"), 39)
        {
        }

        public override string Decode(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encode(object data)
        {
            throw new NotImplementedException();
        }
    }
}