using CommonZ.Utils;
using System.Runtime.CompilerServices;
using IRCode = CommonZ.Utils.Collection<ZSharp.IR.VM.Instruction>;
using VM = ZSharp.IR.VM;

namespace ZSharp.Runtime.NET.IL2IR
{
    public static class LiteralLoader
    {
        public static IRCode Load(object? o)
        {
            return o switch
            {
                sbyte s8 => Load(s8),
                byte u8 => Load(u8),
                short s16 => Load(s16),
                ushort u16 => Load(u16),
                int s32 => Load(s32),
                uint u32 => Load(u32),
                long s64 => Load(s64),
                ulong u64 => Load(u64),
                float f32 => Load(f32),
                double f64 => Load(f64),
                bool b => Load(b),
                char c => Load(c),
                string s => Load(s),
                IntPtr i => Load(i),
                UIntPtr u => Load(u),
                null => [ new VM.PutNull() ],
                _ => throw new NotSupportedException(),
            };
        }

        public static IRCode Load(sbyte v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(byte v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(short v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(ushort v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(int v)
            => [
                new VM.PutInt32(v)
            ];

        public static IRCode Load(uint v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(long v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(ulong v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(IntPtr v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(UIntPtr v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(float v)
            => [
                new VM.PutFloat32(v)
            ];

        public static IRCode Load(double v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(bool v)
            => [
                new VM.PutBoolean(v)
            ];

        public static IRCode Load(char v)
        {
            throw new NotSupportedException();
        }

        public static IRCode Load(string v)
            => [
                new VM.PutString(v)
            ];
    }
}
