using System;

namespace UniversalStore
{
    [Serializable]
    public class Receipt
    {
        public string Payload;
        public string Store;
        public string TransactionID;
    }
}