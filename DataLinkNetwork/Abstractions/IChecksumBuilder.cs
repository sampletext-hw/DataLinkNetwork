using System.Collections;

namespace DataLinkNetwork.Abstractions
{
    public interface IChecksumBuilder
    {
        public BitArray Build(BitArray data);
    }
}