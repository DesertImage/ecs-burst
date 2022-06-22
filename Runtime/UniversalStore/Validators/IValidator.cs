using System;
using System.Threading.Tasks;

namespace UniStore
{
    public interface IValidator
    {
        Task Validate(string receipt, Action<bool> callback);
    }
}