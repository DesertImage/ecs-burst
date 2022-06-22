using System;

namespace UniStore
{
    [Serializable]
    public class ValidationResponse
    {
        public string Status;
        public string Error;
        public string Data;
    }
}