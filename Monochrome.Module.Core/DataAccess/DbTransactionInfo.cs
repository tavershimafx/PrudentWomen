using System.Collections.Generic;

namespace Monochrome.Module.Core.DataAccess
{
    public struct DbTransactionInfo
    {
        public bool Succeeded { get; set; }
        public IList<string> Errors { get; set; }
    }
}
