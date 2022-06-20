using System;
using System.Threading.Tasks;

namespace Monetization
{
    public interface IValidator
    {
        Task Validate(string receipt, Action<bool> callback);
    }
}