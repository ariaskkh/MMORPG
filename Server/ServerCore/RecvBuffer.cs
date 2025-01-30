using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public class RecvBuffer
{
    // [ ][ ][ ][r][ ][w][ ][ ][ ][ ] 10 byte
    ArraySegment<byte> _buffer;
    int _readPos; // 포지션
    int _writePos;

    public RecvBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }

    public int DataSize { get { return _writePos - _readPos; } }
    public int FreeSize { get { return _buffer.Count - _writePos; } }

    public ArraySegment<byte> DataSegment // ReadSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
    }

    public ArraySegment<byte> RecvSegment // WriteSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
    }

    public void Clean()
    {
        int dataSize = DataSize;
        if (dataSize == 0)
        {
            // 남은 데이터가 없으면 복사하지 않고, 커서 위치만 리셋
            _readPos = 0;
            _writePos = 0;
        }
        else
        {
            // 남은 찌끄레기가 있으면 시작 위치로 복사
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize); // 포인터 뿐만 아니라 실제 데이터도 옮겨야 함
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    // 컨텐츠 코드에서 데이터를 가공하여 처리할텐데, 성공적으로 처리한 경우 OnRead 호출
    public bool OnRead(int numOfBytes)
    {
        if (numOfBytes > DataSize)
            return false;
        _readPos += numOfBytes;
        return true;
    }

    public bool OnWrite(int numOfBytes)
    {
        if (numOfBytes > FreeSize)
            return false;
        _writePos += numOfBytes;
        return true;
    }
}
