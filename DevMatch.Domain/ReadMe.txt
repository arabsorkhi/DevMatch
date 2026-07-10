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