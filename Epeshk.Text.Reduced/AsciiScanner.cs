// ReSharper disable All

namespace Epeshk.Text
{
  using System;using System.IO;using System.Text;using M=System.Runtime.CompilerServices.MethodImplAttribute;
  using O=System.Runtime.CompilerServices.MethodImplOptions;using S=System.ReadOnlySpan<byte>;
  using U=System.Buffers.Text.Utf8Parser;using n=System.Int32;using b=System.Boolean;using c=System.Char;

  class AsciiScanner{
    const O I=O.AggressiveInlining;const O N=O.NoInlining;byte[] b;n l,o;readonly Stream s;
    public AsciiScanner(Stream stream=null,n size=8192){s=stream??Console.OpenStandardInput();b=new byte[size];}
    b X<T,TP>(out T v,c f='\0',TP p=default)where TP:struct,G<T> =>A(out v,f,p) ?? W(out v,f,p);
    b? A<T,TP>(out T v,c f='\0',TP p=default)where TP:struct,G<T>{v=default;if (!SkipDelimiters())return null;
      if(!p.P(S,out v,out n b,f))return K<T>()?(b?)null:false;if(o+b>=l)return null;o += b;return true;}
    [M(I)] b W<T,TP>(out T v,c f='\0',TP p=default)where TP:struct,G<T>{while(F()){var q=A(out v,f,p);
      if(q.HasValue)return q.GetValueOrDefault();}var r=p.P(S,out v,out var bc,f);o += bc;return r;}
    Span<byte> S=>b.AsSpan(o,l - o);
    b K<T>()=>typeof(T)==typeof(double)||typeof(T)==typeof(float)?K():S.Length<32;
    [M(N)]b K(){var s=S;if (s.Length<10)return false;var c=s[^1];return c-'+'<='/'-'+'||(c&~32)is'E';}
    b SkipDelimiters(){while(o<l){if(!D(b[o]))return true;o++;}return false;}
    b F(){R();if(l==b.Length)H();n c=s.Read(b,l,b.Length-l);if(c>0)l+=c;return c>0;}
    void R(){n r=l-o;b.AsSpan(o,r).CopyTo(b);o=0;l=r;}
    [M(I)]static b D(byte c)=>c<=32;
    void H(){var c=new byte[b.Length*2];b.CopyTo(c,0);b=c;}
    static void Q()=>throw new FormatException();
    interface G<T>{b P(S s,out T v,out n c,c f='\0');}
    struct CharP:G<c>{public b P(S s,out c v,out n c,c f=default){if(s.IsEmpty){v=default; c=0;return false;}v=(c)s[0];c=1;return true;}}
    struct StringP:G<string>{public b P(S s,out string v,out n c,c f='\0'){if(s.IsEmpty){v=default;c=0;return false;}
      var sb=new StringBuilder();foreach(var b in s){if(D(b))break;sb.Append((c)b);}v=sb.ToString();c=v.Length;return true;}}
    [M(I)] T Read<T,TP>(c f='\0') where TP:struct,G<T>{if(!X<T,TP>(out T v, f))Q();return v;}
    [M(I)] public b TryRead(out string value)=>X<string,StringP>(out value);
    [M(I)] public string ReadString()=>Read<string,StringP>();
    [M(I)] public b TryRead(out c value)=>X<c,CharP>(out value);
    [M(I)] public c ReadChar()=>Read<c,CharP>();
    struct BoolP:G<b> {[M(I)] public b P(S s,out b v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct SByteP:G<sbyte> {[M(I)] public b P(S s,out sbyte v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct ByteP:G<byte> {[M(I)] public b P(S s,out byte v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct Int16P:G<short> {[M(I)] public b P(S s,out short v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct UInt16P:G<ushort> {[M(I)] public b P(S s,out ushort v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct Int32P:G<n> {[M(I)] public b P(S s,out n v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct UInt32P:G<uint> {[M(I)] public b P(S s,out uint v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct Int64P:G<long> {[M(I)] public b P(S s,out long v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct UInt64P:G<ulong> {[M(I)] public b P(S s,out ulong v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct SingleP:G<float> {[M(I)] public b P(S s,out float v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct DoubleP:G<double> {[M(I)] public b P(S s,out double v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    struct DecimalP:G<decimal> {[M(I)] public b P(S s,out decimal v,out n c,c f='\0')=>U.TryParse(s,out v,out c,f);}
    [M(I)] public b TryRead(out b value,c format='\0')=>X<b,BoolP>(out value,format);
    [M(I)] public b ReadBool(c format='\0')=>Read<b,BoolP>(format);
    [M(I)] public b TryRead(out sbyte value,c format='\0')=>X<sbyte,SByteP>(out value,format);
    [M(I)] public sbyte ReadSByte(c format='\0')=>Read<sbyte,SByteP>(format);
    [M(I)] public b TryRead(out byte value,c format='\0')=>X<byte,ByteP>(out value,format);
    [M(I)] public byte ReadByte(c format='\0')=>Read<byte,ByteP>(format);
    [M(I)] public b TryRead(out short value,c format='\0')=>X<short,Int16P>(out value,format);
    [M(I)] public short ReadInt16(c format='\0')=>Read<short,Int16P>(format);
    [M(I)] public b TryRead(out ushort value,c format='\0')=>X<ushort,UInt16P>(out value,format);
    [M(I)] public ushort ReadUInt16(c format='\0')=>Read<ushort,UInt16P>(format);
    [M(I)] public b TryRead(out n value,c format='\0')=>X<n,Int32P>(out value,format);
    [M(I)] public n ReadInt32(c format='\0')=>Read<n,Int32P>(format);
    [M(I)] public b TryRead(out uint value,c format='\0')=>X<uint,UInt32P>(out value,format);
    [M(I)] public uint ReadUInt32(c format='\0')=>Read<uint,UInt32P>(format);
    [M(I)] public b TryRead(out long value,c format='\0')=>X<long,Int64P>(out value,format);
    [M(I)] public long ReadInt64(c format='\0')=>Read<long,Int64P>(format);
    [M(I)] public b TryRead(out ulong value,c format='\0')=>X<ulong,UInt64P>(out value,format);
    [M(I)] public ulong ReadUInt64(c format='\0')=>Read<ulong,UInt64P>(format);
    [M(I)] public b TryRead(out float value,c format='\0')=>X<float,SingleP>(out value,format);
    [M(I)] public float ReadSingle(c format='\0')=>Read<float,SingleP>(format);
    [M(I)] public b TryRead(out double value,c format='\0')=>X<double,DoubleP>(out value,format);
    [M(I)] public double ReadDouble(c format='\0')=>Read<double,DoubleP>(format);
    [M(I)] public b TryRead(out decimal value,c format='\0')=>X<decimal,DecimalP>(out value,format);
    [M(I)] public decimal ReadDecimal(c format='\0')=>Read<decimal,DecimalP>(format);
  }
}