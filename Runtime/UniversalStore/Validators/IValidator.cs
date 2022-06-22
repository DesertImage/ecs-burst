using System;
using System.Threading.Tasks;

namespace UniversalStore
{
    public interface IValidator
    {
        Task Validate(string receipt, Action<bool> callback);
    }
}