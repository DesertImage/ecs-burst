using System;
using System.Text;

namespace UniversalStore
{
    public class AndroidValidator : BaseValidator
    {
        public AndroidValidator(string url) : base(url)
        {
        }

        protected override string SetupReceipt(string receipt)
        {
            var bytesToEncode = Encoding.UTF8.GetBytes(receipt);

            return Convert.ToBase64String(bytesToEncode);
        }
    }
}