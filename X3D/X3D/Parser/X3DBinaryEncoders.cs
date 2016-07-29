using LiquidTechnologies.FastInfoset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Parser.X3DB
{
    // TODO: implement the following X3DB Encoders
    // http://www.web3d.org/documents/specifications/19776-3/V3.3/Part03/tables.html#t-EncodingAlgorithmURI
    public class DefaultEncoder : FIEncodingAlgorithm
    {
        public DefaultEncoder() : base(new Uri("encoder://web3d.org/encoder-not-implemented"))
        {

        }

        public override string Decode(byte[] data)
        {
            Encoding enc;

            enc = UTF8Encoding.UTF8;

            return enc.GetString(data);
        }

        public override byte[] Encode(object data)
        {
            Encoding enc;

            enc = UTF8Encoding.UTF8;

            return enc.GetBytes(data.ToString());
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

    public class QuantizedzlibFloatArrayEncoder : FIEncodingAlgorithm
    {
        public QuantizedzlibFloatArrayEncoder() : base(new Uri("encoder://web3d.org/QuantizedzlibFloatArrayEncoder"), 34)
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
        public QuantizedzlibDoubleArrayEncoder() : base(new Uri("encoder://web3d.org/QuantizedzlibDoubleArrayEncoder"), 38)
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
