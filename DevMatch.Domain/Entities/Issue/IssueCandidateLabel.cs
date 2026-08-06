using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Issue
{
    public sealed class IssueCandidateLabel
    {
        public Guid IssueCandidateId { get; set; }
        public Guid IssueLabelId { get; set; }

        public IssueCandidate IssueCandidate { get; set; } = null!;
        public IssueLabel IssueLabel { get; set; } = null!;
    }

}
