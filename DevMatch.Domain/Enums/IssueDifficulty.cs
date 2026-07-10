using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Enums
{
    //belong to human
    public enum IssueDifficulty
    {
        Unknown = 0,

        Beginner = 1,

        Junior = 2,

        MidLevel = 3,

        Senior = 4,

        Expert = 5
    }
    public enum GitIssueState
    {
        Open = 1,

        Closed = 2
    }

    //for developer or issue
    public enum SkillLevel
    {
        Beginner = 1,

        Junior = 2,

        Intermediate = 3,

        Advanced = 4,

        Expert = 5
    }



}
