namespace PacketGenerator;

public class PacketFormat
{
    // {0} 패킷 이름
    // {1} 멤버 변수들
    // {2} 멤버 변수 Read
    // {3} 멤버 변수 Write
    public static string packetFormat =
@"
class {0}
{{
    {1}

    public void Read(ArraySegment<byte> seg)
    {{
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        ushort count = 0;
        
        count += sizeof(ushort); // 패킷 길이
        count += sizeof(ushort); // 패킷 id
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        count += sizeof(ushort); // size
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);
        {3}        

        success &= BitConverter.TryWriteBytes(s, count); // size를 마지막에 넣어줘야 함
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }}
}}
";
    // {0} 변수 형식
    // {1} 변수 이름
    public static string memberFormat =
@"public {0} {1};";

    // {0} 리스트 이름 [대문자]
    // {1} 리스트 이름 [소문자]
    // {2} 멤버 변수들
    // {3} 멤버 변수 Read
    // {4} 멤버 변수 Write
    public static string memberListFormat =
@"public struct {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}
public List<{0}> {1}s = new List<{0}>();
";

    // {0} 변수 이름
    // {1} To~ 변수 형식
    // {2} 변수 형식
    public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

    // {0} 변수 이름
    public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

    // {0} 리스트 이름 [대문자]
    // {1} 리스트 이름 [소문자]
    public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}";

    // {0} 변수 이름
    // {1} 변수 형식
    public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0});
count += sizeof({1});";

    // {0} 변수 이름
    public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort)); // 실제 데이터 카피를 먼저 함
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

    // {0} 리스트 이름 [대문자]
    // {1} 리스트 이름 [소문자]
    public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in {1}s)
    success &= {1}.Write(s, ref count);";
}
