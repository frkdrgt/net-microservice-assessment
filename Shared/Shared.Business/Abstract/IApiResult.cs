using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Abstract
{
    interface IApiResult
    {
        bool IsSucceed { get; set; }
        string Message { get; }
    }
    interface IApiResult<T> : IApiResult
    {
        T ResultObject { get; set; }
    }
}
