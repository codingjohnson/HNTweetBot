using System;
using System.Runtime.Serialization;

namespace OpenBitly.Serialization
{
    [Serializable]
    public class BitlyResult : BitlyEntity
    {
        [DataMember]
        public virtual BitlyData Data { get; set; }
    }
}