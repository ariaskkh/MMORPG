using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 재귀적 락 허용 NO
    // 재귀적 락 서용 YES - WriteLock->WriteLock OK, WriteLock->ReadLock OK, ReadLock->WriteLock NO
    // 스핀락 정책 (5000번 -> Yield)
    class Lock
    {

        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        int flag = EMPTY_FLAG;
        int writeCount = 0;


        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int writeLockThreadId = (flag & WRITE_MASK) >> 16;
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (writeLockThreadId == currentThreadId)
            {
                writeCount++;
                return;
            }

            int desired = ((currentThreadId << 16) & WRITE_MASK);
            
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        writeCount = 1;
                        return;
                    }
                }
                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --writeCount;
            if (lockCount == 0)
            {
                Interlocked.Exchange(ref flag, EMPTY_FLAG); // 깨끗한 값으로 밀어주기
            }
        }

        public void ReadLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int writeLockThreadId = (flag & WRITE_MASK) >> 16;
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (writeLockThreadId == currentThreadId)
            {
                Interlocked.Increment(ref flag);
                return;
            }

            int writeArea = flag & WRITE_MASK;
            // 아무도 writeLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // readLock과 writeLock이 동시에 잡히면 안됨
                    int expected = (flag & READ_MASK);
                    // 2가지 포인트 1) writeLock 있으면 flag와 expected이 다름 2) readlock 동시에 잡으려 하면 expected로 거르기
                    if (Interlocked.CompareExchange(ref flag, expected + 1, expected) == expected) // 어려운 부분
                    {
                        return;
                    }
                }
                Thread.Yield();
            }
            
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref flag);
        }
    }
}
