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
        Unknown=0,

        Beginner = 1,

        Junior = 2,

        Intermediate = 3,

        Advanced = 4,

        Expert = 5
    }
    public enum DeveloperSkillSource
    {
        Manual = 1,
        GitHubRepository = 2,
        GitHubContribution = 3,
        Imported = 4
    }
    public enum IssueTaskType
    {
        Unknown = 0,
        Bug = 1,
        Documentation = 2,
        Testing = 3,
        Refactor = 4,
        Feature = 5,
        UserInterface = 6,
        DevOps = 7,
        Performance = 8,
        Security = 9
    }

    public enum EstimateConfidence
    {
        Low = 1,
        Medium = 2,
        High = 3
    }


    //    private static readonly int[,] LevelMatrix =
    //    {
    ///* Required ↓   Beginner Junior Mid Senior Expert */

    ///* Beginner */ {100,100,100,100,100},

    ///* Junior */   {40,100,100,100,100},

    ///* Mid */      {20,60,100,100,100},

    ///* Senior */   {10,40,70,100,100},

    ///* Expert */   {0,20,50,80,100}
    //    };


}
