using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Enums
{
    public enum IssueCandidateState
    {
        Open = 0,
        Closed = 1,
        Removed = 2
    }

    public enum IssueSyncStatus
    {
        Pending = 0,
        Running = 1,
        Succeeded = 2,
        PartiallySucceeded = 3,
        Failed = 4
    }

}
