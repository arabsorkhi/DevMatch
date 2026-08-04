using DevMatch.Application.Abstraction.Auth;
using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Common.Error;

namespace DevMatch.Application.Features.Auth.Github.BeginLogin
{
    public sealed class BeginGitHubLoginHandler
    {
        private readonly IGitHubOAuthClient _gitHubOAuthClient;

        public BeginGitHubLoginHandler(
            IGitHubOAuthClient gitHubOAuthClient)
        {
            _gitHubOAuthClient = gitHubOAuthClient;
        }

        public Task<Result<BeginGitHubLoginResponse>> Handle(
            BeginGitHubLoginQuery query,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);

            cancellationToken.ThrowIfCancellationRequested();

            string state = GenerateState();

            string authorizationUrl =
                _gitHubOAuthClient.BuildAuthorizationUrl(state);

            if (string.IsNullOrWhiteSpace(authorizationUrl))
            {
                return Task.FromResult(
                    Result<BeginGitHubLoginResponse>.Failure(
                        GitHubAuthenticationErrors.InvalidAuthorizationUrl));
            }

            var response = new BeginGitHubLoginResponse(
                authorizationUrl);

            return Task.FromResult(
                Result<BeginGitHubLoginResponse>.Success(response));
        }

        private static string GenerateState()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);

            return Convert
                .ToHexString(randomBytes)
                .ToLowerInvariant();
        }
    }
}