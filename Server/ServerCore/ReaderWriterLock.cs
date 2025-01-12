using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{

    interface IReaderWriterLock
    {
        public void WriteLock();
        public void WriteUnlock();
        public void ReadLock();
        public void ReadUnlock();
    }

    class ReaderWriterLock : IReaderWriterLock
    {
        int _flag;
        const int EMPTY_FLAG = 0;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_LOCK_COUNT = 5000;
        int _writeLockCount = 0;

        // 재귀락 불가 가정
        // [부호(0)] [WriteLock ThreadId(15)] [ReadLock Count(16)]
        // Lock 정책 - 5000번 이후 yield

        public void WriteLock()
        {
            var rawCurThreadId = Thread.CurrentThread.ManagedThreadId;
            var writeLockThreadId = (_flag & WRITE_MASK) >> 16;
            if (rawCurThreadId == writeLockThreadId)
            {
                _writeLockCount++;
                return;
            }

            var desiredThreadId = (rawCurThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i <= MAX_LOCK_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desiredThreadId, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeLockCount = 1;
                        return;
                    }
                        
                }
                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            _writeLockCount--;
            if (_writeLockCount == 0)
            {
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
            }
        }

        public void ReadLock()
        {
            // 재귀 lock 허용 -> writeLock 잡힌 상태처리
            if (_writeLockCount > 0)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            while (true)
            {
                for (var i = 0; i < MAX_LOCK_COUNT; i++)
                {
                    // writeLock이 안잡힌 상태
                    var expected = _flag & READ_MASK;
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
