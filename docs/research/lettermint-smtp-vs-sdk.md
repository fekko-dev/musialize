# Lettermint: SMTP with MailKit or the .NET SDK

Research date: 2026-11-06. Sources are Lettermint's docs and OpenAPI spec, the `lettermint-dotnet` source on GitHub, MailKit's source, NuGet, and Microsoft Learn. Points that a source does not state, or that I inferred, are marked ⚠️.

Context: the backend sends only transactional email: sign-in codes for an Account now, possibly invites for Members of a Group later. Volume is a few hundred a month. The Lettermint account is approved, and test mail through both the HTTP API and SMTP on 587 from the Hetzner server worked. The question is how the ASP.NET Core backend should talk to Lettermint, not which provider to use (see [provider research](transactional-email-providers.md) and [ADR 0006](../adr/0006-lettermint-smtp-for-transactional-email.md)).

## 1. Summary and recommendation

**Keep MailKit over SMTP (587, STARTTLS).** This agrees with ADR 0006, but for more specific reasons than the ADR gives. The SDK is well built, and on paper it is the nicer API. It loses on the points that matter for this project:

1. **One code path everywhere.** MailKit sends the same message to Mailpit locally and in CI and to Lettermint in production, so the sign-in flow can be tested end to end, including reading the code from the mail. The SDK only talks to Lettermint's HTTP API. Mailpit has its own HTTP send API, not Lettermint's ([Mailpit API](https://mailpit.axllent.org/docs/api-v1/)). Lettermint's Sandbox mode is not on the Free plan ([sandbox](https://lettermint.co/docs/platform/projects-and-routes/sandbox-mode)). So with the SDK, local and CI runs need an app-level `IEmailSender` fake, and the production sender is never exercised outside production.
2. **SMTP has parity for everything we need.** That includes the Lettermint message ID. Lettermint's `250` reply after `DATA` contains it (`250 Message accepted for delivery id=<uuid>`) ([response codes](https://lettermint.co/docs/guides/send-email-with-smtp/response-codes)), and MailKit 4.18's `SendAsync` returns that reply text ([MailTransport.cs @4.18.1](https://github.com/jstedfast/MailKit/blob/4.18.1/MailKit/MailTransport.cs)). Tags, metadata, idempotency, reply-to, custom headers and text + HTML all work over SMTP (§3.1).
3. **Dependency risk.** The SDK's 1.0.0 was released on 2026-09-07. It has had 8 releases in 2 months, including a breaking 2.0 after which "1.x no longer receives updates" ([UPGRADE.md](https://github.com/lettermint/lettermint-dotnet/blob/main/UPGRADE.md)). One human contributor has made 22 commits, and NuGet shows ~3.6K downloads. MailKit has been around since 2013 and has ~289M downloads (§3.4). For a sign-in path we can't do without, the boring option wins.
4. **Provider-neutral.** Moving to another provider changes host, port and credentials, not code. The ADR already relies on this.

What the SDK would give us is a typed `MessageId`, typed exceptions, a `RetryAfter` value, and no transitive dependencies. None of these outweighs the points above at our volume. The one SMTP chore is to parse `id=` from the reply string and log it.

## 2. Comparison

Legend: ✅ yes, documented · 🟡 partial / caveat · ❌ no

|                                       | MailKit → SMTP relay                                                                        | `Lettermint` SDK → HTTP API                               |
| ------------------------------------- | ------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| Lettermint message ID on send         | 🟡 in the `250` text (`id=<uuid>`); parse it from the returned string                       | ✅ `SendMailResponse.MessageId`                           |
| Tags / metadata                       | ✅ `X-LM-Tags`, `X-LM-Metadata-<key>` headers                                               | ✅ `.Tags(...)`, `.Metadata(...)`                         |
| Idempotency                           | ✅ `Idempotency-Key` message header                                                         | ✅ `IdempotentRequestOptions`                             |
| Reply-To, custom headers, text + HTML | ✅                                                                                          | ✅                                                        |
| Errors                                | `SmtpCommandException` with `StatusCode`; 4xx temporary, 5xx permanent                      | typed exceptions per HTTP status; `RetryAfter` on 429/5xx |
| Automatic retry                       | ❌ none                                                                                     | ❌ "never retries on its own"                             |
| Local/CI with Mailpit                 | ✅ same code path                                                                           | ❌ needs an app-level fake                                |
| Lettermint Sandbox                    | 🟡 header-driven; paid plans only                                                           | 🟡 `.SandboxResult(...)`; paid plans only                 |
| Port                                  | 587 (Hetzner: open, tested)                                                                 | 443                                                       |
| Default timeout                       | 2 min (socket)                                                                              | 30 s (whole request)                                      |
| Package age / downloads               | since 2013 / ~289M                                                                          | since 2026-09-07 / ~3.6K                                  |
| Transitive deps (net8/net10)          | MimeKit → BouncyCastle.Cryptography, System.Security.Cryptography.Pkcs; System.Formats.Asn1 | none                                                      |
| Licence                               | MIT                                                                                         | MIT                                                       |

## 3. Findings

### 3.1 Feature parity and message IDs

- **SMTP control headers** ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)): `X-LM-Tag` (single), `X-LM-Tags: name=value, …` (up to 20, can be repeated, max 900 bytes per header), `X-LM-Metadata-<key>` (sent in webhooks, not shown to recipients), `X-Lettermint-Route`, `X-LM-Scheduled-At` (Growth/Pro only), `X-LM-Override-TLS` and `X-LM-Preserve-Message-ID`. Lettermint strips these headers before delivery. Malformed tags are rejected, not skipped.
- **Idempotency over SMTP:** "The `Idempotency-Key` header works the same way when sending via SMTP relay." The key lasts 24 h per project. Reusing it with a different body is rejected ([idempotency](https://lettermint.co/docs/platform/emails/idempotency); over SMTP this gives `550 5.7.0`, or `451 4.7.0` while the first send is still in flight: [response codes](https://lettermint.co/docs/guides/send-email-with-smtp/response-codes)).
- **Reply-To, custom headers, text + HTML:** with MailKit these are plain MIME headers and a `BodyBuilder` with `TextBody`/`HtmlBody`. An invalid `Reply-To` gives `553 5.1.7` ([response codes](https://lettermint.co/docs/guides/send-email-with-smtp/response-codes)). The SDK has `ReplyTo`, `Headers`, `Text`, `Html` and `Metadata` ([Emails.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/Emails.cs)).
- **Message ID, SMTP:** `250 Message accepted for delivery id=0199a5f2-…` ([response codes](https://lettermint.co/docs/guides/send-email-with-smtp/response-codes)). Lettermint's API responses and webhooks use a UUID `message_id` that is separate from the RFC `Message-ID`, which Lettermint replaces by default ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)). In MailKit 4.18.x, `SendAsync` returns "The final free-form text response from the server" ([MailTransport.cs @4.18.1](https://github.com/jstedfast/MailKit/blob/4.18.1/MailKit/MailTransport.cs)). MailKit's `master` branch adds a structured `SmtpSendResult` with the status code and response text, but it is not in a release yet ([SmtpClient.cs @master](https://github.com/jstedfast/MailKit/blob/master/MailKit/Net/Smtp/SmtpClient.cs)). ⚠️ The docs don't say outright that the `id=` value is the same UUID as `message_id` in webhooks and the dashboard. Its format suggests so; check it with one real send.
- **Message ID, API/SDK:** `POST /v1/send` returns `202` ([OpenAPI](https://lettermint.co/docs/api-reference/sending/0.0.1/lettermint-sending-openapi.json)). The SDK maps this to `SendMailResponse { MessageId, Status, Sandbox?, SandboxResult?, … }` ([Types.g.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/Generated/Types.g.cs)).
- **For "I didn't get my code":** both paths give the ID at send time. Logging it next to the Account ID is enough to find the message in the 28-day dashboard history.

### 3.2 Errors, rate limits, retries

- **SMTP:** "`4xx` means a temporary failure, so retry later. `5xx` means a permanent failure" ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)). The ones that matter for us:
  - `451 4.3.0`: temporary Lettermint error.
  - `454 4.7.0`: token check temporarily unavailable.
  - `535 5.7.8`: wrong token, SMTP not enabled, or IP allowlist.
  - `550 5.7.0`: monthly limit reached, which means a plan upgrade.
  - `550 5.7.1`: unverified domain or recipient.

  All codes are in the [response codes](https://lettermint.co/docs/guides/send-email-with-smtp/response-codes) guide. MailKit reports a rejected command as `SmtpCommandException` with `StatusCode` and `ErrorCode` (`MessageNotAccepted`, `SenderNotAccepted`, `RecipientNotAccepted`, …) ([SmtpCommandException.cs](https://github.com/jstedfast/MailKit/blob/4.18.1/MailKit/Net/Smtp/SmtpCommandException.cs)). MailKit's `SmtpClient` contains no retry logic (no "retry" in [SmtpClient.cs @4.18.1](https://github.com/jstedfast/MailKit/blob/4.18.1/MailKit/Net/Smtp/SmtpClient.cs)).

- **API/SDK:** the OpenAPI spec lists `202`, `401`, `403` (`FEATURE_NOT_AVAILABLE`), `409` (idempotency), `422` (`VALIDATION_ERROR`), `429` (`RATE_LIMITED`, with `Retry-After`) and `500` ([OpenAPI](https://lettermint.co/docs/api-reference/sending/0.0.1/lettermint-sending-openapi.json)). The SDK maps each to an exception: `AuthenticationException`, `ValidationException`, `RateLimitException.RetryAfter`, `ServerException.RetryAfter`, `LettermintTimeoutException`, `ConnectionException`, and so on. It also states: "The SDK never retries on its own" ([README](https://github.com/lettermint/lettermint-dotnet/blob/main/README.md), [Exceptions.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/Exceptions.cs)).
- **Rate limits:** SMTP "uses the same sending pipeline as the Sending API" and has "no fixed per-minute rate limit". Plan quota and abuse controls still apply ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)). ⚠️ No numeric API rate limit is published for the Sending API ([limitations](https://lettermint.co/docs/platform/emails/limitations)). It doesn't matter at our volume.
- **What this means for us:** neither path retries. A sign-in code is sent while the person waits on the page, so the simple and correct behaviour is the same on both paths: on a temporary failure, show "couldn't send, try again". The new code gets a new idempotency key, so there is no need for a background retry queue (YAGNI).

### 3.3 Testing

- **MailKit:** point `Email:Smtp` at Mailpit and assert through Mailpit's API, as in [provider research §4](transactional-email-providers.md#4-local-and-ci-testing-without-real-mail). Lettermint's headers are ordinary headers to Mailpit and are ignored.
- **SDK, stubbing:** `LettermintOptions` takes `BaseUrl` and a caller-owned `HttpClient`. The handler must not follow redirects, and the client must not set default auth headers ([LettermintOptions.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/LettermintOptions.cs), [Transport.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/Internal/Transport.cs)). The SDK's own tests stub HTTP with a `FakeHandler : HttpMessageHandler` ([Helpers.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/tests/Lettermint.Tests/Helpers.cs)). So HTTP can be stubbed, but then a test checks the JSON we send, not that a Member receives a readable code. In practice you would put an `IEmailSender` interface in front with a fake for dev and CI: two implementations, and the real one only runs in production.
- **Lettermint's own test options:**
  - Sandbox projects work over both API and SMTP, don't count against the quota, and show up in the dashboard and webhooks. They are on Starter, Growth and Pro only; on Free, sandbox sends are rejected with `403 FEATURE_NOT_AVAILABLE` or `550 5.7.1` ([sandbox](https://lettermint.co/docs/platform/projects-and-routes/sandbox-mode)).
  - Test addresses on Live projects: `ok@`, `softbounce@`, `hardbounce@`, `spamcomplaint@` and `dsn@lettermint.dev` are "Excluded from billing" ([test emails](https://lettermint.co/docs/platform/emails/sending-test-emails)). That page only shows API examples. The SMTP guide mentions `@lettermint.dev` addresses in the sandbox context ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)). ⚠️ That they work over SMTP on a Live project is likely but not stated. They are useful for a manual smoke test after deploy, not for CI, because the code can't be read back.

### 3.4 Dependency risk

|                           | `Lettermint`                                                                           | `MailKit`                                                                                                  |
| ------------------------- | -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Latest                    | 2.3.0, 2026-10-30                                                                      | 4.18.2, 2026-10-23                                                                                         |
| First release             | 1.0.0, 2026-09-07                                                                      | repo created 2013-12-07                                                                                    |
| Releases                  | 8 in 2 months; breaking 2.0 on 2026-10-04                                              | regular releases (latest tag 4.18.1 on GitHub; 4.18.2 on NuGet)                                            |
| Downloads                 | ~3.6K                                                                                  | ~289M (MimeKit ~345M)                                                                                      |
| Maintainers               | NuGet owner `lettermint`; contributors `bjarn` (22 commits), dependabot, a release bot | NuGet owner `jstedfast`; 6.9K stars                                                                        |
| Issues                    | 0 ever filed                                                                           | 8 open                                                                                                     |
| Licence                   | MIT                                                                                    | MIT                                                                                                        |
| Targets                   | net8.0, net10.0                                                                        | netstandard2.0/2.1, net4.6.2–4.8, net8.0, net10.0                                                          |
| Transitive deps (net8/10) | none                                                                                   | MimeKit 4.18.2 (→ BouncyCastle.Cryptography 2.7.0, System.Security.Cryptography.Pkcs), System.Formats.Asn1 |

Sources: [NuGet search API](https://azuresearch-usnc.nuget.org/query?q=packageid:lettermint), [Lettermint CHANGELOG](https://github.com/lettermint/lettermint-dotnet/blob/main/CHANGELOG.md), [releases](https://github.com/lettermint/lettermint-dotnet/releases), [Lettermint.csproj](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/Lettermint.csproj), `gh api repos/lettermint/lettermint-dotnet{,/contributors,/issues}`, [MailKit on NuGet](https://www.nuget.org/packages/MailKit), [MailKit 4.18.2 catalog entry](https://api.nuget.org/v3/catalog0/data/2026.10.23.12.33.27/mailkit.4.18.2.json), [MimeKit on NuGet](https://www.nuget.org/packages/MimeKit), `gh api repos/jstedfast/MailKit`.

The SDK's code quality is good. It never follows redirects, redacts tokens, uses open enums for forward compatibility, has `TreatWarningsAsErrors` and NuGet audit on, and runs CI on three OSes ([README](https://github.com/lettermint/lettermint-dotnet/blob/main/README.md), [Directory.Build.props](https://github.com/lettermint/lettermint-dotnet/blob/main/Directory.Build.props)). The risk is its age and bus factor, not its quality. MailKit's extra dependencies (BouncyCastle for S/MIME/PGP, which we don't use) are a larger surface, but a well-known one.

`System.Net.Mail.SmtpClient` is not an option. Microsoft: "We don't recommend that you use the `SmtpClient` class for new development … Use MailKit or other libraries instead" ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient)).

### 3.5 Code size (illustrative)

**MailKit.** Options bound from `Email:Smtp`, password from a Dokploy secret (`Email__Smtp__Password`):

```csharp
var message = new MimeMessage();
message.From.Add(MailboxAddress.Parse(smtp.From));
message.To.Add(MailboxAddress.Parse(account.Email));
message.Subject = "Your sign-in code";
message.Headers.Add("X-LM-Tags", "type=sign-in-code");
message.Headers.Add("Idempotency-Key", $"sign-in-{codeId}");
message.Body = new BodyBuilder { TextBody = text, HtmlBody = html }.ToMessageBody();

using var client = new SmtpClient();
await client.ConnectAsync(smtp.Host, smtp.Port, smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);
if (smtp.Username is not null) await client.AuthenticateAsync(smtp.Username, smtp.Password, ct);
var reply = await client.SendAsync(message, ct);   // "Message accepted for delivery id=0199…"
await client.DisconnectAsync(true, ct);
logger.LogInformation("Sign-in code sent: {Reply}", reply);
```

**SDK.** Project token from a Dokploy secret (`Email__Lettermint__ProjectToken`):

```csharp
builder.Services.AddSingleton(_ => new LettermintClient(new LettermintOptions
{
    SendingToken = builder.Configuration["Email:Lettermint:ProjectToken"],
}));

var result = await lettermint.Emails.Compose()
    .From(from).To(account.Email).Subject("Your sign-in code")
    .Text(text).Html(html)
    .Tags(new MessageTagInput { Name = "type", Value = "sign-in-code" })
    .SendAsync(new IdempotentRequestOptions { IdempotencyKey = $"sign-in-{codeId}" }, ct);
logger.LogInformation("Sign-in code sent: {MessageId}", result.MessageId);
```

API from [README](https://github.com/lettermint/lettermint-dotnet/blob/main/README.md) and [Emails.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/Emails.cs). The SDK version is a few lines shorter. Add the `IEmailSender` fake it needs for dev and CI (§3.3), and the total is about the same.

### 3.6 Operations

- **Ports:** 587 STARTTLS is open on Hetzner Cloud and tested ([Hetzner FAQ](https://docs.hetzner.com/cloud/servers/faq/)). The API uses 443, which nobody blocks. Lettermint also offers 2587 (STARTTLS) and 2465 (implicit TLS) as SMTP fallbacks ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)).
- **Connection overhead:** connect, STARTTLS, AUTH and send for each message costs a few extra round trips compared with one HTTPS request. ⚠️ Not measured; at a few hundred sends a month it is negligible. Don't pool SMTP connections (YAGNI).
- **Timeouts:** MailKit's default is 2 minutes per network operation (`timeout = 2 * 60 * 1000`, [SmtpClient.cs @4.18.1](https://github.com/jstedfast/MailKit/blob/4.18.1/MailKit/Net/Smtp/SmtpClient.cs)). Set `Timeout` to ~15–30 s and pass the request's `CancellationToken` so a hung relay doesn't hold a sign-in request for minutes. The SDK defaults to 30 s for the whole request ([LettermintOptions.cs](https://github.com/lettermint/lettermint-dotnet/blob/main/src/Lettermint/LettermintOptions.cs)).
- **Limits on both paths:** 50 recipients per message, 25 MB ([limitations](https://lettermint.co/docs/platform/emails/limitations)). Irrelevant for sign-in codes.

### 3.7 Third option: raw `HttpClient` to `POST /v1/send`

This is one JSON POST with an `x-lettermint-token` header ([OpenAPI](https://lettermint.co/docs/api-reference/sending/0.0.1/lettermint-sending-openapi.json)), with no package and no SDK churn. It is only worth it if we leave SMTP for another reason, such as 587 being blocked. Even then it has the same testing gap as the SDK. Not recommended now.

## 4. What would change the recommendation

- **We need Lettermint features beyond "send one message" from code**, such as resend, cancel or reschedule, suppression management, or analytics. The SDK covers these through the Team API ([README](https://github.com/lettermint/lettermint-dotnet/blob/main/README.md)). Not planned.
- **Webhooks.** If we add a webhook endpoint (for example, to show "bounced" to an Organizer), the SDK's `Webhook` verifier can be used **for verification only**, while sending stays on SMTP. This does not by itself argue for sending through the SDK.
- **Outbound 587 becomes unreliable** from Hetzner or Dokploy. Try 2587 first, then switch to the API.
- **We move to a paid plan and want Sandbox** as the CI target instead of Mailpit. Sandbox works over SMTP too, so this alone doesn't flip the choice.
- **The SDK matures:** a year of non-breaking releases, more maintainers, real usage. That would remove reason 3, but not reasons 1 and 4.

## 5. Flagged uncertainties

- ⚠️ That the `id=` in the SMTP `250` reply is the same UUID as `message_id` in the dashboard and webhooks is implied, not stated. Check with one send and log the reply.
- ⚠️ Whether the `@lettermint.dev` test addresses work over SMTP on a Live (Free) project. The test-email page only shows API examples.
- ⚠️ No numeric rate limit is published for the Sending API or SMTP. Irrelevant at our volume.
- ⚠️ MailKit's structured `SmtpSendResult` is on `master` only. Until it ships, the message ID comes from parsing the reply string, and Lettermint could change the wording.
- ⚠️ The handshake overhead of SMTP versus HTTPS was not measured.
- ⚠️ MailKit's GitHub tags stop at 4.18.1 while NuGet has 4.18.2. Source references here use the 4.18.1 tag.

## 6. Sources

All checked 2026-11-06. GitHub data via `gh api`.

- Lettermint docs: https://lettermint.co/docs/guides/send-email-with-smtp · https://lettermint.co/docs/guides/send-email-with-smtp/response-codes · https://lettermint.co/docs/guides/send-email-with-dotnet · https://lettermint.co/docs/platform/emails/idempotency · https://lettermint.co/docs/platform/projects-and-routes/sandbox-mode · https://lettermint.co/docs/platform/emails/sending-test-emails · https://lettermint.co/docs/platform/emails/limitations · https://lettermint.co/docs/api-reference/introduction · https://lettermint.co/docs/api-reference/sending · https://lettermint.co/docs/api-reference/sending/0.0.1/lettermint-sending-openapi.json · https://lettermint.co/docs/llms.txt
- Lettermint SDK: https://github.com/lettermint/lettermint-dotnet (`README.md`, `CHANGELOG.md`, `UPGRADE.md`, `Directory.Build.props`, `src/Lettermint/{Lettermint.csproj,LettermintClient.cs,LettermintOptions.cs,Emails.cs,Exceptions.cs,Internal/Transport.cs,Generated/Types.g.cs}`, `tests/Lettermint.Tests/{Helpers.cs,TransportTests.cs}`, releases, contributors, issues) · https://www.nuget.org/packages/Lettermint
- MailKit / MimeKit: https://github.com/jstedfast/MailKit (tag 4.18.1: `MailKit/MailTransport.cs`, `MailKit/MailService.cs`, `MailKit/Net/Smtp/{SmtpClient,SmtpCommandException,SmtpErrorCode}.cs`; `master`: `MailKit/Net/Smtp/SmtpClient.cs`) · https://www.nuget.org/packages/MailKit · https://www.nuget.org/packages/MimeKit · https://github.com/jstedfast/MimeKit (tag 4.18.1, `MimeKit/MimeKit.csproj`)
- Microsoft: https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient
- Mailpit: https://mailpit.axllent.org/docs/api-v1/
- Hetzner: https://docs.hetzner.com/cloud/servers/faq/
