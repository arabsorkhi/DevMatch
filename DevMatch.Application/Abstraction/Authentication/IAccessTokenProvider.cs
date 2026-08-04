using DevMatch.Domain.Entities.Developer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DevMatch.Application.Abstraction.Authentication
{
    //GitHub Token:
    //برای درخواست به GitHub API

    //DevMatch JWT:
    //برای احراز هویت کاربر در DevMatch


    public interface IAccessTokenProvider
    {
        InternalAccessToken Create(Developer developer);
    }

    public sealed record InternalAccessToken(
        string Token,
        DateTimeOffset ExpiresAtUtc);

    //    RefreshToken
    //    ├── Id
    //    ├── DeveloperId
    //    ├── TokenHash
    //    ├── ExpiresAt
    //    ├── CreatedAt
    //    ├── RevokedAt
    //    └── ReplacedByTokenId

    //خود Refresh Token را به‌صورت Plain Text در دیتابیس ذخیره نکن؛ فقط Hash آن ذخیره شود.
}
