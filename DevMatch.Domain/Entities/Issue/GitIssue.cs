using DevMatch.Domain.Enums;
using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Issue
{
    
    public sealed class GitIssue : AuditableEntity<Guid>
    {
        private GitIssue()
        {
        }

        public Guid GitRepositoryId { get; private set; }

        public long GithubIssueId { get; private set; }

        public int Number { get; private set; }

        public string Title { get; private set; } = null!;

        public string? Body { get; private set; }

        public GitIssueState State { get; private set; }

        public IssueDifficulty Difficulty { get; private set; }

        public bool IsGoodFirstIssue { get; private set; }

        public bool IsHelpWanted { get; private set; }

        public DateTime? ClosedAtUtc { get; private set; }

        public GitRepository.GitRepository GitRepository { get; private set; } = null!;

        //GitHub Sync
        // 
        // ↓
        // 
        // Create()
        // 
        // ↓
        // 
        // Database

        public static GitIssue Create(
            Guid repositoryId,
            long githubIssueId,
            int number,
            string title,
            string? body,
            bool goodFirstIssue,
            bool helpWanted)
        {
            return new GitIssue
            {
                Id = Guid.NewGuid(),

                GitRepositoryId = repositoryId,

                GithubIssueId = githubIssueId,

                Number = number,

                Title = title,

                Body = body,

                IsGoodFirstIssue = goodFirstIssue,

                IsHelpWanted = helpWanted,

                State = GitIssueState.Open,

                Difficulty = IssueDifficulty.Unknown,

                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public void UpdateFromGithub(
            string title,
            string? body,
            bool goodFirstIssue,
            bool helpWanted)
        {
            Title = title;

            Body = body;

            IsGoodFirstIssue = goodFirstIssue;

            IsHelpWanted = helpWanted;

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeDifficulty(
            IssueDifficulty difficulty)
        {
            Difficulty = difficulty;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void Close()
        {
            if (State == GitIssueState.Closed)
                return;

            State = GitIssueState.Closed;

            ClosedAtUtc = DateTime.UtcNow;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void ReOpen()
        {
            if (State == GitIssueState.Open)
                return;

            State = GitIssueState.Open;

            ClosedAtUtc = null;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void Rename(string title)
        {
            Title = title;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void ChangeBody(string? body)
        {
            Body = body;

            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
 