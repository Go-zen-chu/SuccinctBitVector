using System.Collections.Generic;

namespace Succinct
{
    public class SuccinctBitVector<T>
    {
        const int BIT_FLG_LENGTH = 25000; // range of values(should be less than max range of int which is 2,147,483,647)
        const int BIT_FLG_ARRAY_LENGTH = BIT_FLG_LENGTH / 32; // 32 = 4byte which is type long's size
        // bit-flg array which describe whether the value exists in specified index
        ulong[] bitFlgArray = new ulong[BIT_FLG_ARRAY_LENGTH];
        // Indices which count the number of bit-flgs. Number of flgs has to be fewer than 1-65535
        // If the number of bit-flgs is lower than 256 use byte rather than ushort.
        ushort[] bitCountArray = new ushort[BIT_FLG_ARRAY_LENGTH];
        // Squeeze all data into this list
        List<T> denseDataList = new List<T>();

        public SuccinctBitVector() { }

        public void SetValue(int idx, T value)
        {
            var bitFlg = 0x1UL << (idx % 64);
            // Operate "or" to bit flg array
            bitFlgArray[idx / 64] |= bitFlg;

            denseDataList.Add(value);
        }

        // Actually this is an ugly solution because you can define GetValue method
        // and switch conditions depends on Types like "if(typeof(T) == typeof(int))"
        // but I didn't want to use "if" inside these methods so I changed the name of the method.

        // This method throws exception if type T is not numeric
        public T GetValue_Numeric(int idx)
        {
            // casting 0 to object and then cast to T
            return isDatumExists(idx) ? denseDataList[rank(idx) - 1] : (T)(dynamic)0;
        }
        // This method throws exception if type T is not nullable(e.g. numeric Types)
        public T GetValue_Nullable(int idx)
        {
            return isDatumExists(idx) ? denseDataList[rank(idx) - 1] : (T)(dynamic)null;
        }

        bool isDatumExists(int idx)
        {
            var bitFlg = 0x1UL << (idx % 64);
            // check whether the flg at the idx is 1
            var flg = bitFlgArray[idx / 64] & bitFlg;
            return flg != 0x0UL;
        }

        
        // Rank tells which index you should get from dense data array
        ushort rank(int idx)
        {
            ulong bit_mask = ulong.MaxValue; // all 1
            if (idx % 64 < 63) bit_mask = (0x1UL << (idx % 64 + 1)) - 0x1UL;
            return (ushort)(bitCountArray[idx / 64] + popCount64(bitFlgArray[idx / 64] & bit_mask));
        }

        public void Build()
        {
            // count flags that have true values
            bitCountArray[0] = 0; // No flg is true before idx 0
            for (int array_idx = 0; array_idx < BIT_FLG_ARRAY_LENGTH - 1; array_idx++)
            {
                bitCountArray[array_idx + 1] = (ushort)(bitCountArray[array_idx] + popCount64(bitFlgArray[array_idx]));
            }
        }
        // This method simply counts the number of true flags in 64bit(= x)
        ushort popCount64(ulong x)
        {
            x = ((x & 0xaaaaaaaaaaaaaaaaUL) >> 1) + (x & 0x5555555555555555UL);
            x = ((x & 0xccccccccccccccccUL) >> 2) + (x & 0x3333333333333333UL);
            x = ((x & 0xf0f0f0f0f0f0f0f0UL) >> 4) + (x & 0x0f0f0f0f0f0f0f0fUL);
            x = ((x & 0xff00ff00ff00ff00UL) >> 8) + (x & 0x00ff00ff00ff00ffUL);
            x = ((x & 0xffff0000ffff0000UL) >> 16) + (x & 0x0000ffff0000ffffUL);
            x = ((x & 0xffffffff00000000UL) >> 32) + (x & 0x00000000ffffffffUL);
            return (ushort)x;
        }
    }
}
