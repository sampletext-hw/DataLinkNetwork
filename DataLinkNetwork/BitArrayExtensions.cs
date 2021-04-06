using System.Collections;
using System.Text;

namespace DataLinkNetwork
{
    public static class BitArrayExtensions
    {
        public static string ToBinString(this BitArray bitArray)
        {
            StringBuilder builder = new StringBuilder(bitArray.Length);
            for (var i = 0; i < bitArray.Count; i++)
            {
                builder.Append(bitArray[i] ? 1 : 0);
            }

            return builder.ToString();
        }

        public static bool IsSame(this BitArray bitArray, BitArray compare)
        {
            if (bitArray.Length != compare.Length)
            {
                return false;
            }

            for (var i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i] != compare[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSameNoCopy(this BitArray srcArray, BitArray cmpArray, int srcStartIndex, int cmpStartIndex, int length)
        {
            for (var i = 0; i < length; i++)
            {
                if (srcArray[srcStartIndex + i] != cmpArray[cmpStartIndex + i])
                {
                    return false;
                }
            }

            return true;
        }

        public static int FindFlag(this BitArray bitArray, int offset = 0)
        {
            int i = offset;
            while (i <= bitArray.Length - Constants.FlagSize)
            {
                if (bitArray.IsSameNoCopy(HdlcFrame.Flag, i, 0, Constants.FlagSize))
                {
                    // We hit the flag
                    return i;
                }

                i++;
            }

            return -1;
        }
    }
}