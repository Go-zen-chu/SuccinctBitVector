using System.Collections.Generic;

namespace Succinct
{
    public class SuccinctBitVector<T>
    {
        int rangeOfIndex = 40000;
        int columnLength = 40000 / 32 + 1;
        public int RangeOfIndex
        {
            get { return rangeOfIndex; }
            set
            {
                rangeOfIndex = value;
                columnLength = rangeOfIndex / 32 + 1; // 32 = 4byte which is type long's size
                bitFlgArray = new ulong[columnLength];
                flgCountArray = new ushort[columnLength];
            }
        }

        // bit-flg array which describe whether the value exists in specified index
        ulong[] bitFlgArray;
        // Count the number of bit-flgs. Number of values has to be fewer than 1-65535 because the type is ushort.
        // If the number of bit-flgs is lower than 256 use byte rather than ushort.
        ushort[] flgCountArray;
        // Squeeze all data into this list
        List<T> denseDataList = new List<T>();


        public SuccinctBitVector()
        {
            bitFlgArray = new ulong[columnLength];
            flgCountArray = new ushort[columnLength];
        }

        public SuccinctBitVector(int rangeOfIndex)
        {
            RangeOfIndex = rangeOfIndex;
        }

        public void SetValue(int idx, T value)
        {
            var bitFlg = 0x1UL << (idx % 64);
            // Operate "or" to bit flg array
            bitFlgArray[idx / 64] |= bitFlg;

            denseDataList.Add(value);
        }

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

        public T GetValue(int idx)
        {
            var type = typeof(T);
            if (type == typeof(int) || type == typeof(double) || type == typeof(short) || type == typeof(float)
                || type == typeof(uint) || type == typeof(ushort) || type == typeof(long) || type == typeof(ulong))
            {
                return isDatumExists(idx) ? denseDataList[rank(idx) - 1] : (T)(dynamic)0;
            }
            else
            {
                return isDatumExists(idx) ? denseDataList[rank(idx) - 1] : (T)(dynamic)null;
            }
        }

        bool isDatumExists(int idx)
        {
            if (idx < 0) return false;
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
            return (ushort)(flgCountArray[idx / 64] + popCount64(bitFlgArray[idx / 64] & bit_mask));
        }

        public void Build()
        {
            // count flags that have true values
            flgCountArray[0] = 0; // No flg is true before idx 0
            for (int array_idx = 0; array_idx < columnLength - 1; array_idx++)
            {
                flgCountArray[array_idx + 1] = (ushort)(flgCountArray[array_idx] + popCount64(bitFlgArray[array_idx]));
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
