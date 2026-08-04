Domain
│
├── Entities
│   ├── Developer
│   ├── GitRepository
│   ├── GitIssue
│   ├── Skill
│   ├── DeveloperSkill
│   ├── IssueSkill
│   ├── Bookmark
│   ├── IssueApplication
│   └── Notification
│
├── ValueObjects
│   ├── MatchScore
│   ├── GithubUrl
│   └── SkillLevel
│
├── Enums
│
├── Services
│   ├── MatchingService
│   ├── SkillExtractionService
│   └── DifficultyEstimator
│
└── Events




MatchResult
│
├── Score
├── MissingSkills
├── MatchedSkills
├── Recommendation
└── IsQualified


DevMatch.Domain
│
├── Services
│      IMatchingService.cs
│      MatchingService.cs
│
├── ValueObjects
│      ConfidenceScore.cs
│      MatchResult.cs
│      MatchScore.cs
│
├── Enums
│
├── Entities


IssueSkill

↓

Skill Exists؟

↓

Level Match

↓

Confidence

↓

Verified

↓

Weight

↓

Final Score



======better 

Developer

↓

Skill Score

↓

Repository Score

↓

Contribution Score

↓

Activity Score

↓

Preference Score

↓

Final Recommendation



=======

Recommendation Score

=

Skill Match

+

Experience

+

Activity

+

Interest

+

History


این Issue ۹۴٪ برای تو مناسب است.s

چون

✔ Redis

✔ Docker

✔ قبلاً Bug Fix کردی

✔ اخیراً Active بودی

✔ علاقه‌مند به Backend هستی


===========\
Application
    ↓
IGitHubClient
    ↓
Infrastructure
    ↓
GitHub API


=========

Developer

↓

Skills

↓

Repositories

↓

History

↓

Preferences

↓

Activity

↓

Recommendation


===========

Recommendation

=

Skill

×

0.45

+

Repository

×

0.15

+

Contribution

×

0.15

+

Activity

×

0.10

+

Preference

×

0.10

+

History

×

0.05

=========
اولین Use Case واقعی محصول

Login with GitHub
        ↓
SyncRepositories
        ↓
SyncIssues
        ↓
ExtractSkills
        ↓
RecommendIssues


==============
Sprint 3.1 — Matching Engine (کامل)
SkillScore
RepositoryScore
ContributionScore
ActivityScore
PreferenceScore
HistoryScore
RecommendationScore
RecommendationResult
IMatchingEngine
BasicMatchingEngine
Unit Testهای موتور
Sprint 3.2 — GitHub Integration (کامل)
IGitHubClient
GitHubClient
GitHubOptions
Authentication
DTOها
Mapping
Error Handling
DI Registration
Sprint 3.3 — Repository Synchronization
SyncRepositoriesCommand
Handler
Validator
Endpoint
Upsert Logic
تست
Sprint 3.4 — Issue Synchronization
SyncIssuesCommand
Handler
Validator
Endpoint
Mapping
ذخیره Skillها
Sprint 3.5 — Recommendation
RecommendIssuesQuery
Handler
Endpoint
اتصال به Matching Engine


===========
Feature 1 (بعدی)

✅ SkillAlias

Entity
Configuration
Migration
CRUD
Validator
Endpoint
Feature 2

✅ GitHub Configuration

GitHubOptions
IGitHubClient
GitHubClient
HttpClient Registration
Polly
Authentication
Feature 3

✅ SyncRepositories

کامل

Feature 4

✅ SyncIssues

کامل





GenerateDailyRecommendations.Handler
            ↓
IMatchingProfileReader
            ↓
IMatchingService.RankIssues
            ↓
IMatchingEngine.Match
            ↓
BasicMatchingEngine
            ↓
MatchResult
            ↓
DailyRecommendation
            ↓
IUnitOfWork.SaveChangesAsync



DevMatch.Domain/
├── Entities/
│   └── Matching/
│       ├── DeveloperMatchProfile.cs
│       ├── IssueMatchProfile.cs
│       ├── MatchingSnapshots.cs
│       ├── MatchResult.cs
│       ├── MatchComponentScores.cs
│       ├── MatchReason.cs
│       └── MatchingWeights.cs
│
├── Services/
│   ├── IMatchingEngine.cs
│   └── BasicMatchingEngine.cs
│
└── ValueObjects/
    └── Matching/
        ├── MatchScore.cs
        ├── RepositoryScore.cs
        ├── ContributionScore.cs
        ├── ActivityScore.cs
        ├── PreferenceScore.cs
        └── HistoryScore.cs



        DevMatch.Application/
├── Abstraction/
│   └── Matching/
│       ├── IMatchingProfileReader.cs
│       └── IAiMatchingEngine.cs
│
├── Matching/
│   ├── IMatchingService.cs
│   └── MatchingService.cs
│
└── Features/
    └── Recommendations/
        └── GenerateDailyRecommendations/
            ├── Command.cs
            ├── Validator.cs
            ├── Response.cs
            ├── Errors.cs
            └── Handler.cs


            DevMatch.Infrastructure/
├── Matching/
│   └── MatchingProfileReader.cs
│
└── Ai/
    └── AiMatchingEngine.cs




    GitHub Callback
    ↓
Validate state
    ↓
Exchange authorization code
    ↓
Receive GitHub access token
    ↓
Fetch authenticated GitHub user
    ↓
Find Developer by GitHubUserId
    ↓
Create or synchronize Developer
    ↓
Issue application access token
    ↓
Return/redirect authenticated user




04082026

Begin Login
    ↓
Generate state
    ↓
Store state in cache/cookie
    ↓
Redirect to GitHub
    ↓
GitHub callback
    ↓
Validate and consume state
    ↓
Exchange code for access token