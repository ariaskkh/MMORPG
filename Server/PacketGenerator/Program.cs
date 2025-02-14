using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace PacketGenerator;

class Program
{
    static string genPackets;
    static ushort packetId;
    static string packetEnums;

    static void Main(string[] args)
    {
        XmlReaderSettings settings = new XmlReaderSettings()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };
        using (XmlReader r = XmlReader.Create("PDL.xml", settings))
        {
            r.MoveToContent(); // header 건너 뛰고 contents 부분으로 바로 감 (<packet ~ >)

            while (r.Read()) // 이게 왜 필요한진 모르겠음
            {
                if (r.Depth == 1 && r.NodeType == XmlNodeType.Element) // 패킷 시작 element
                    ParsePacket(r); // 처음에 packet을 parse해야함
            }
            
            string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
            File.WriteAllText("GenPackets.cs", fileText);
        }
    }

    public static void ParsePacket(XmlReader r)
    {
        if (r.NodeType == XmlNodeType.EndElement)
            return;

        if (r.Name.ToLower() != "packet")
        {
            Console.WriteLine("Invalid packet node");
            return;
        }

        string packetName = r["name"];
        if (string.IsNullOrEmpty(packetName))
        {
            Console.WriteLine("Packet without name");
            return;
        }

        Tuple<string, string, string> t = ParseMembers(r);
        genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);

        packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";
    }

    // {1} 멤버 변수들
    // {2} 멤버 변수 Read
    // {3} 멤버 변수 Write
    static public Tuple<string, string, string> ParseMembers(XmlReader r)
    {
        string packetName = r["name"];

        string memberCode = "";
        string readCode = "";
        string writeCode = "";

        int depth = r.Depth + 1;
        while (r.Read())
        {
            if (r.Depth != depth)
                break;

            string memberName = r["name"];
            if (string.IsNullOrEmpty(memberName))
            {
                Console.WriteLine("Member without name");
                return null;
            }

            if (!string.IsNullOrEmpty(memberCode))
                memberCode += Environment.NewLine;
            if (!string.IsNullOrEmpty(readCode))
                readCode += Environment.NewLine;
            if (!string.IsNullOrEmpty(writeCode))
                writeCode += Environment.NewLine;

            string memberType = r.Name.ToLower();
            switch (memberType)
            {
                case "byte":
                case "sbyte":
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                    writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                    break;
                case "bool":
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "float":
                case "double":
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                    writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                    break;
                case "string":
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    readCode += string.Format(PacketFormat.readStringFormat, memberName);
                    writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                    break;
                case "list":
                    Tuple<string, string, string> t = ParseList(r);
                    memberCode += t.Item1;
                    readCode += t.Item2;
                    writeCode += t.Item3;
                    break;
                default:
                    break;
            }
        }

        memberCode = memberCode.Replace("\n", "\n\t");
        readCode = readCode.Replace("\n", "\n\t\t");
        writeCode = writeCode.Replace("\n", "\n\t\t");

        return new Tuple<string, string, string>(memberCode, readCode, writeCode);
    }

    private static Tuple<string, string, string> ParseList(XmlReader r)
    {
        string listName = r["name"];
        if (string.IsNullOrEmpty(listName))
        {
            Console.WriteLine("List without name");
            return null;
        }

        string upperListName = FirstCharToUpper(listName);
        string lowerListName = FirstCharToLower(listName);

        Tuple<string, string, string> t = ParseMembers(r);
        string memberCode = string.Format(PacketFormat.memberListFormat,
            upperListName,
            lowerListName,
            t.Item1,
            t.Item2,
            t.Item3);
        string readCode = string.Format(PacketFormat.readListFormat, upperListName, lowerListName);
        string writeCode = string.Format(PacketFormat.writeListFormat, upperListName, lowerListName);

        return new Tuple<string, string, string>(memberCode, readCode, writeCode);
    }

    private static string ToMemberType(string memberType)
    {
        switch (memberType)
        {
            case "bool":
                return "ToBoolean";
            case "short":
                return "ToInt16";
            case "ushort":
                return "ToUInt16";
            case "int":
                return "ToInt32";
            case "uint":
                return "ToUInt32";
            case "long":
                return "ToInt64";
            case "float":
                return "ToSingle";
            case "double":
                return "ToDouble";
            default:
                return "";
        }
    }

    private static string FirstCharToUpper(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        return input[0].ToString().ToUpper() + input.Substring(1);
    }

    private static string FirstCharToLower(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        return input[0].ToString().ToLower() + input.Substring(1);
    }
}
