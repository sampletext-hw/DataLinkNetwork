using System.Collections;

namespace DataLinkNetwork
{
    public interface IChecksumBuilder
    {
        public BitArray Build(BitArray data);
    }
}