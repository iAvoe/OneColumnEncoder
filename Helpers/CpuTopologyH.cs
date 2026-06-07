using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers
{
    public partial class CpuTopologyH
    {
        // ----- Win32 P/Invoke 声明 -----
        private enum LOGICAL_PROCESSOR_RELATIONSHIP : int
        {
            RelationProcessorCore = 0,
            RelationNumaNode = 1,
            RelationCache = 2,
            RelationProcessorPackage = 3,
            RelationGroup = 4
        }

        private enum PROCESSOR_CACHE_TYPE : int
        {
            CacheUnified,
            CacheInstruction,
            CacheData,
            CacheTrace
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CACHE_RELATIONSHIP
        {
            [FieldOffset(0)] public byte Level;
            [FieldOffset(1)] public byte Associativity;
            [FieldOffset(2)] public ushort LineSize;
            [FieldOffset(4)] public uint CacheSize;
            [FieldOffset(8)] public PROCESSOR_CACHE_TYPE Type;
            [FieldOffset(32)] public GROUP_AFFINITY GroupMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GROUP_AFFINITY
        {
            public ulong Mask;
            public ushort Group;
            public ushort Reserved1;
            public ushort Reserved2;
            public ushort Reserved3;
        }

        // Use Ex variant
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX
        {
            public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
            public uint Size;
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetLogicalProcessorInformationEx(
            LOGICAL_PROCESSOR_RELATIONSHIP relationshipType,
            IntPtr buffer,
            ref uint returnedLength);

        public class CacheGroupInfo
        {
            public int TotalCores { get; set; }
            public int TotalThreads { get; set; }
            public int GroupCount { get; set; }
            public int CoresPerGroup { get; set; }
            public int ThreadsPerGroup { get; set; }
            public int CacheMbPerGroup { get; set; } // TotalL3Mb/GroupCount
            public int TotalL3Mb { get; set; }
        }

        public static CacheGroupInfo? GetCacheTopology()
        {
            uint returnLength = 0;
            // Pass in RelationAll (0xFFFF)
            GetLogicalProcessorInformationEx((LOGICAL_PROCESSOR_RELATIONSHIP)0xFFFF, IntPtr.Zero, ref returnLength);

            int err = Marshal.GetLastWin32Error();
            if (err != 122) return null;// ERROR_INSUFFICIENT_BUFFER

            IntPtr buffer = Marshal.AllocHGlobal((int)returnLength);
            try
            {
                if (!GetLogicalProcessorInformationEx((LOGICAL_PROCESSOR_RELATIONSHIP)0xFFFF, buffer, ref returnLength))
                    return null;

                int totalCores = 0;
                int l3GroupCount = 0;
                int l3ThreadsPerGroup = 0;
                long l3SizePerGroup = 0;

                int offset = 0;
                while (offset < returnLength)
                {
                    IntPtr currentPtr = buffer + offset;
                    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX baseInfo =
                        Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>(currentPtr);

                    if (baseInfo.Size == 0) break;

                    switch (baseInfo.Relationship)
                    {
                        case LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore:
                            totalCores++;
                            break;

                        case LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache:

                            IntPtr cachePtr = currentPtr + 8;
                            CACHE_RELATIONSHIP cacheDesc =
                                Marshal.PtrToStructure<CACHE_RELATIONSHIP>(cachePtr);

                            if (cacheDesc.Level == 3)
                            {
                                l3GroupCount++;
                                if (l3SizePerGroup == 0)
                                {
                                    l3SizePerGroup = cacheDesc.CacheSize;
                                    l3ThreadsPerGroup = CountBits(cacheDesc.GroupMask.Mask);
                                }
                            }
                            break;
                    }

                    offset += (int)baseInfo.Size;
                }

                if (l3GroupCount == 0) return null;

                int totalThreads = Environment.ProcessorCount;
                int cacheMb = (int)(l3SizePerGroup / (1024 * 1024));

                return new CacheGroupInfo
                {
                    TotalCores = totalCores,
                    TotalThreads = totalThreads,
                    GroupCount = l3GroupCount,
                    CoresPerGroup = totalCores / l3GroupCount,
                    ThreadsPerGroup = l3ThreadsPerGroup,
                    CacheMbPerGroup = cacheMb,
                    TotalL3Mb = cacheMb * l3GroupCount
                };
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        public static int GetProcessorPackageCount()
        {
            uint returnLength = 0;
            GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage, IntPtr.Zero, ref returnLength);

            int err = Marshal.GetLastWin32Error();
            if (err != 122) return 0;// ERROR_INSUFFICIENT_BUFFER

            IntPtr buffer = Marshal.AllocHGlobal((int)returnLength);
            try
            {
                if (!GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage, buffer, ref returnLength))
                    return 0;

                int packageCount = 0;
                int offset = 0;
                while (offset < returnLength)
                {
                    IntPtr currentPtr = buffer + offset;
                    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX baseInfo =
                        Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>(currentPtr);

                    if (baseInfo.Size == 0) break;
                    if (baseInfo.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage)
                        packageCount++;

                    offset += (int)baseInfo.Size;
                }

                return packageCount;
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        private static int CountBits(ulong value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }
    }
}
