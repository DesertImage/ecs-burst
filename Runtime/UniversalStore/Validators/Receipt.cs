using System;

namespace UniStore
{
    [Serializable]
    public class Receipt
    {
        public string Payload;
        public string Store;
        public string TransactionID;
    }
}