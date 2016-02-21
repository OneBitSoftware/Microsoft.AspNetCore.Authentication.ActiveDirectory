using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory.Events
{
    public class BaseActiveDirectoryContext : BaseControlContext
    {
        public BaseActiveDirectoryContext(HttpContext context, ActiveDirectoryOptions options)
            : base(context)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Options = options;
        }

        public ActiveDirectoryOptions Options { get; }
    }
}
