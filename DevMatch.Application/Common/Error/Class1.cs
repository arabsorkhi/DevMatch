using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Common.Error
{
    public static class DeveloperErrors
    {
        public static readonly SharedKernel.Result.Error AlreadyExists =
            new(
                "Developer.AlreadyExists",
                "Developer already exists.");

        public static readonly SharedKernel.Result.Error NotFound =
            new(
                "Developer.NotFound",
                "Developer was not found.");
    }
}
