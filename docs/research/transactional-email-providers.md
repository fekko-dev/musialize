# Transactional email providers for sign-in codes

Research date: 2026-11-06. Sources are the providers' own pricing pages, docs, legal pages and SDK repos/NuGet listings, plus Hetzner's docs. Where a page could not be loaded, contradicted another official page, or looks recently changed, it is marked ⚠️. Anything not confirmed in a primary source is marked **unverified**.

Context: Musialize sends only transactional email: one-time sign-in codes for an Account now, possibly invite links later. The pilot is one Group of ~20–40 Members on 2026-11-12; later there may be several Groups on one deployment. The backend is ASP.NET Core in Docker on Hetzner Cloud (EU) via Dokploy. The sending domain is a subdomain of `fekko.dev`, later possibly a domain per Group. Recipients are mostly in NL/EU.

## 1. Summary and recommendation

**Pick: Lettermint, sent over SMTP with MailKit on port 587.** It is the only candidate that meets every must-have and every nice-to-have except multi-domain on the free tier:

- **EU-native.** Dutch company (Lettermint B.V.), "Built in the Netherlands. Hosted in Europe." ([home](https://lettermint.co/)). All subprocessors that touch email are in NL/DE ([subprocessors](https://lettermint.co/subprocessors)). The DPA applies without a separate signature ([DPA](https://lettermint.co/dpa)).
- **Free tier:** 300 emails/month, no daily limit, 1 sending domain, 28 days of message history, webhooks and SMTP relay included, "No credit card required" ([pricing](https://lettermint.co/pricing)).
- **.NET:** SMTP relay (`smtp.lettermint.co`, ports 587/465/2587/2465) ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)) and an official, dependency-free SDK, `Lettermint` 2.3.0 (2026-10-30, net8.0/net10.0) ([NuGet](https://www.nuget.org/packages/Lettermint), [.NET guide](https://lettermint.co/docs/guides/send-email-with-dotnet)).
- **Setup:** four DNS records (2 DKIM CNAMEs, a Return-Path CNAME that also covers SPF, and DMARC) ([domains](https://lettermint.co/docs/platform/domains/introduction)), then a compliance verification form. Until it is approved you can only send to team members and test addresses ([compliance verification](https://lettermint.co/docs/platform/onboarding/compliance-verification)).

**Use SMTP via MailKit, not the SDK.** This is justified here, not just a preference: the app only needs "send one message", SMTP is the same code path for Mailpit locally and in CI (§4), and switching to the runner-up is a config change. [MailKit](https://www.nuget.org/packages/MailKit) is current (4.18.2, 2026-10-23). The SDK's extras (typed errors, a `RetryAfter` value, webhook signature verification) don't outweigh that; see the [SMTP vs SDK comparison](lettermint-smtp-vs-sdk.md).

**Hetzner gotcha:** Hetzner blocks outbound **465** (and 25) on Cloud servers; **587 is open** ([Hetzner FAQ](https://docs.hetzner.com/cloud/servers/faq/)). Lettermint recommends 465, so configure **587 with STARTTLS** (or 2465/2587).

**Trade-offs to accept:**

- 300/month is enough for the pilot but not "comfortable". With ~40 Members and long-lived sessions, sign-ins plus resends should stay well under 300; frequent re-sign-ins would not. Beyond that, Starter is from €10/month for 10,000 emails ([pricing](https://lettermint.co/pricing)).
- Free is 1 domain. A domain per Group later means Growth (up to 30 domains) or Pro (unlimited) ([pricing](https://lettermint.co/pricing)).
- The compliance review must be done **before 2026-11-12**. Its duration is not stated in the docs (see §6). _Update 2026-11-06:_ the form itself says most reviews finish within a few hours; our account was approved the same day.

**Decision:** [ADR 0006](../adr/0006-lettermint-smtp-for-transactional-email.md).

**Runner-up: Mailjet.** 6,000 emails/month with a 200/day cap and no card required ([pricing](https://www.mailjet.com/pricing/)). Data is stored on GCP in Frankfurt and Saint-Ghislain, Belgium ([security & privacy](https://www.mailjet.com/security-privacy/)), and there is an official .NET SDK, `Mailjet.Api` ([repo](https://github.com/mailjet/mailjet-apiv3-dotnet)). It is the better choice if volume outgrows 300/month before you want to pay. Downsides: owned by Sinch (non-EU group; its DPA now lives on sinch.com), the free plan shows a Mailjet logo (whether on transactional API/SMTP mail is unclear ⚠️), and the free-plan domain limit and log retention are not stated.

## 2. Comparison table

Legend: ✅ yes, documented · 🟡 partial / caveat · ❌ no · ❓ unverified

|                                    | Lettermint                                   | Mailjet                                   | Scaleway TEM                                                     | Resend                                                        | Amazon SES (eu-central-1)                                                                 | Brevo                             |
| ---------------------------------- | -------------------------------------------- | ----------------------------------------- | ---------------------------------------------------------------- | ------------------------------------------------------------- | ----------------------------------------------------------------------------------------- | --------------------------------- |
| **Free tier**                      | 300/mo, no daily cap                         | 6,000/mo, 200/day                         | 300/mo, then pay-as-you-go                                       | 3,000/mo, 100/day ⚠️                                          | ❌ no permanent free tier; new AWS accounts get up to $200 credits (6-month free plan) ⚠️ | 300/day ⚠️                        |
| **Log / history retention (free)** | 28 days                                      | ❓ ("Basic Statistics")                   | while domain is active; webhook data 15 days                     | 30 days                                                       | your own (event destinations)                                                             | ❓ "limited"                      |
| **Domains on free tier**           | 1                                            | ❓                                        | ❓ ⚠️                                                            | 3 ⚠️                                                          | unlimited identities                                                                      | ❓ "limited"                      |
| **Branding on free**               | ❓ none mentioned                            | 🟡 Mailjet logo (scope ❓)                | ✅ none (headers sent as-is)                                     | ❓ none mentioned                                             | ✅ none                                                                                   | 🟡 Brevo branding                 |
| **EU data residency**              | ✅ NL/DE, EU company                         | ✅ storage in DE/BE; 🟡 US parent (Sinch) | ✅ Paris only, EU company                                        | ❌ sends from eu-west-1, but account data/logs stored in US   | ✅ region-scoped                                                                          | ❓ ⚠️ couldn't load               |
| **GDPR / DPA**                     | ✅ DPA in T&C                                | ✅ standard DPA (Sinch)                   | ✅ DPA; no non-EU transfers for TEM                              | ✅ DPA with SCCs, US processing                               | ✅ AWS DPA (not reviewed here)                                                            | ❓                                |
| **.NET**                           | ✅ official SDK + SMTP                       | ✅ official SDK + SMTP                    | 🟡 SMTP/REST only                                                | 🟡 SDK under `resend` org, NuGet owner is an individual; SMTP | ✅ AWS SDK + SMTP                                                                         | 🟡 REST (RestSharp example), SMTP |
| **Multi-domain later**             | 🟡 Growth: 30, Pro: unlimited                | ❓                                        | ✅ per Project                                                   | 🟡 Pro: 10                                                    | ✅                                                                                        | ❓                                |
| **Logs / webhooks**                | ✅ full delivery/bounce events               | ✅ webhooks on free                       | 🟡 1 webhook per domain (Essential)                              | ✅                                                            | 🟡 via SNS/EventBridge (more setup)                                                       | ❓                                |
| **Card to start**                  | ✅ not required                              | ✅ not required                           | ❓ ⚠️                                                            | ❓                                                            | ❌ required (AWS account)                                                                 | ❌ required per pricing page ⚠️   |
| **Setup effort**                   | DNS (4 records) + compliance form (hours ❓) | DNS (SPF/DKIM) ❓                         | DNS (SPF, DKIM, MX recommended); new-account limits case-by-case | DNS; low                                                      | DNS (3 DKIM CNAMEs, custom MAIL FROM) + **sandbox exit, initial response within 96 h**    | DNS + account ❓                  |

Sources per cell are in §3.

## 3. Per-provider findings

### Lettermint (recommended)

- **Pricing:** Free €0: 300 emails/month, 1 domain, 1 project, 1 user, "No limit" daily, 28-day message history, webhooks, SMTP relay, "No credit card required". Starter from €10/month for 10,000; Growth from €13 (up to 30 sending domains); Pro from €15 (unlimited domains) ([pricing](https://lettermint.co/pricing)).
- **DNS:** `lm1._domainkey` and `lm2._domainkey` CNAMEs (DKIM, rotation), a Return-Path CNAME that "provides SPF authentication" (no separate SPF record), and DMARC. Lettermint blocks first-time verification if DMARC has `aspf=s`. It recommends a subdomain such as `mail.acme.com` ([domains](https://lettermint.co/docs/platform/domains/introduction)). ⚠️ Some provider-specific knowledge-base guides list 3 records instead of 4; follow the dashboard.
- **Onboarding:** create team and project → verify a sending domain → API token → compliance form → wait for verification. "Unverified teams can only send to verified team members and test addresses" ([compliance verification](https://lettermint.co/docs/platform/onboarding/compliance-verification)).
- **SMTP:** host `smtp.lettermint.co`, username `lettermint`, password = Project API token, 465 recommended, 587/2587 STARTTLS, 2465 implicit TLS. SMTP must be enabled per project. Bare LF line endings are rejected with `451` (MailKit writes CRLF) ([SMTP guide](https://lettermint.co/docs/guides/send-email-with-smtp)).
- **Webhooks:** `message.delivered`, `message.hard_bounced`, `message.soft_bounced`, `message.spam_complaint`, `message.failed`, `message.suppressed`, `message.policy_rejected` and more ([events](https://lettermint.co/docs/platform/webhooks/events)). For "I didn't get my code", the 28-day dashboard history is enough for the pilot.
- **SDK:** `Lettermint` 2.3.0, owner `lettermint`, MIT, repo [lettermint/lettermint-dotnet](https://github.com/lettermint/lettermint-dotnet) (pushed 2026-10-30). It is young (1.0.0 on 2026-09-07, ~3.7K downloads) ([NuGet](https://www.nuget.org/packages/Lettermint)). Another reason to stay on SMTP.
- **Privacy:** subprocessors UpCloud (NL, hosting), OVH (DE, backups), Postmastery (NL, deliverability) handle email. Aikido and Mistral are listed as "Europe" with no email access ([subprocessors](https://lettermint.co/subprocessors)). The DPA is part of the Terms; a countersigned copy is available on request ([DPA](https://lettermint.co/dpa)).

### Mailjet (runner-up)

- **Pricing:** Free: 6,000/month, 200/day, 1,000 contacts, APIs, SMTP relay, webhooks, basic statistics. "No Mailjet Logo" starts at Essential. "No credit card required for the free plan" ([pricing](https://www.mailjet.com/pricing/)). Mailjet's help article on the 200/day limit says excess mail is queued to the next day (found via search; ⚠️ the page returned 403 when loaded directly: [help article](https://documentation.mailjet.com/hc/en-us/articles/360043048393-What-is-this-200-emails-per-day-limit-on-free-accounts)).
- **Privacy:** data stored on Google Cloud in Frankfurt (DE) and Saint-Ghislain (BE); DPO via Sinch; standard DPA ([security & privacy](https://www.mailjet.com/security-privacy/)). `mailjet.com/legal/dpa` redirects to Sinch's DPA.
- **.NET:** "the Mailjet official .NET API wrapper", NuGet `Mailjet.Api` 4.1.0 ([repo](https://github.com/mailjet/mailjet-apiv3-dotnet)). Release date and last-commit date not shown ❓.

### Scaleway Transactional Email

- **Free tier:** the FAQ uses "the free tier of 300 emails per month" as its billing example; you pay only for the excess ([FAQ](https://github.com/scaleway/docs-content/blob/main/pages/transactional-email/faq.mdx)). Default quota is 10,000/month, and "new users may face stricter initial limits" ([capabilities and limits](https://github.com/scaleway/docs-content/blob/main/pages/transactional-email/reference-content/tem-capabilities-and-limits.mdx)). Plans: Essential (pay-as-you-go, one webhook per domain) and Scale (100K included, dedicated IP, €0.20 per extra 1,000) ([plans](https://github.com/scaleway/docs-content/blob/main/pages/transactional-email/how-to/manage-tem-plans.mdx), [concepts](https://github.com/scaleway/docs-content/blob/main/pages/transactional-email/concepts.mdx)). ⚠️ scaleway.com returned 403 to every fetch, so these are from Scaleway's official docs source on GitHub. The pricing page (Essential price per email, domain limits, card requirement) was not readable.
- **Privacy:** only in `fr-par`; "Email content, metadata, logs, webhook data, and backups are stored and processed in France"; no tracking pixels or link rewriting; logs kept while the domain is active, webhook data 15 days ([data privacy](https://github.com/scaleway/docs-content/blob/main/pages/transactional-email/reference-content/data-privacy.mdx)).
- **SMTP:** `smtp.tem.scaleway.com`, 587/2587 STARTTLS or 465/2465; username = Project ID, password = IAM API secret key ([SMTP configuration](https://github.com/scaleway/docs-content/blob/main/pages/transactional-email/reference-content/smtp-configuration.mdx)). No .NET SDK.
- **Why not the pick:** the strongest privacy story, but its free tier is the same 300/month, IAM/Project setup is heavier, and price and card requirement could not be verified.

### Resend

- **Pricing:** Free: 3,000/month, 100/day, 3 domains, 30-day retention. Pro $20/month (50K, 10 domains) ([pricing](https://resend.com/pricing)). ⚠️ "3 domains" on Free looks recently changed (it was 1); card requirement not stated.
- **Residency:** sending regions include Ireland (eu-west-1), but "account data, including email metadata, logs, and API records, is stored in the United States" ([regions](https://resend.com/docs/dashboard/domains/regions)). The DPA states "primary processing operations take place in the United States" and relies on SCCs/DPF ([DPA](https://resend.com/legal/dpa)). This is GDPR-workable but not EU residency.
- **.NET / SMTP:** `dotnet add package Resend` ([docs](https://resend.com/docs/send-with-dotnet)). The repo is under the `resend` org, but the NuGet owner is an individual (`filipe.toscano`), version 0.24.0 ([NuGet](https://www.nuget.org/packages/Resend)). SMTP: `smtp.resend.com`, user `resend`, password = API key, ports 25/465/587/2465/2587 ([SMTP](https://resend.com/docs/send-with-smtp)).
- **Why not the pick:** the most generous free tier with the simplest setup, but Member email addresses and logs would live in the US. If you value volume over EU residency, Resend replaces Mailjet as the runner-up.

### Amazon SES

- **Pricing:** new AWS customers get "up to $200 in AWS Free Tier credits"; the free plan lasts 6 months. After that it costs $0.10 per 1,000 (à la carte). New accounts start on the Essentials plan ($0.16/1,000) from 2026-07-21 unless switched ([pricing](https://aws.amazon.com/ses/pricing/)). ⚠️ This replaces the older SES-specific monthly free allowance; pricing changed in 2025–2026.
- **Sandbox:** new accounts can only send to verified addresses, max 200/24 h. Production access is requested per region, and "AWS Support team provides an initial response to your request within 96 hours" ([production access](https://docs.aws.amazon.com/ses/latest/dg/request-production-access.html)). That is a real risk for a pilot six days away.
- **DNS:** Easy DKIM (2048-bit) plus optional custom MAIL FROM ([Easy DKIM](https://docs.aws.amazon.com/ses/latest/dg/send-email-authentication-dkim-easy.html)). SMTP on 587/2587 STARTTLS or 465/2465 ([SMTP](https://docs.aws.amazon.com/ses/latest/dg/smtp-connect.html)).
- **Why not the pick:** cheapest at scale and EU-region, but it needs a card, IAM setup, bounce handling via SNS, and an approval that may not land before 2026-11-12.

### Brevo

- Pricing page: the Free plan includes transactional email, with Brevo branding, limited senders/domains and log retention, and "card required" ([pricing](https://www.brevo.com/pricing/)). A search snippet of Brevo's help center states 300 sends/day ([help article](https://help.brevo.com/hc/en-us/articles/208580669)). ⚠️ The help center, GDPR and privacy pages returned 403 or an empty body, so limits, residency and DPA are unverified. The developer docs show no .NET SDK, only a RestSharp call ([send a transactional email](https://developers.brevo.com/docs/send-a-transactional-email)).
- **Why dropped:** branding, a card required, and none of the details could be verified.

### Dropped early

- **Postmark:** free is 100 emails/month ([pricing](https://postmarkapp.com/pricing)). That fails "a few hundred per month". It also has no EU region on the pricing page.
- **Mailgun (EU region):** free is 100/day, 1 custom domain, **1-day** log retention ([pricing](https://www.mailgun.com/pricing/)). One-day logs don't help with "I didn't get my code". Whether Free can use the EU region is not stated. The DPA lists GCP Germany/Belgium for EU customers ([DPA](https://www.mailgun.com/legal/dpa/)).
- **SendGrid:** only a "free trial" is described, with no ongoing free plan ([product page](https://www.twilio.com/en-us/sendgrid)). ⚠️ The pricing URL redirect-loops between twilio.com and sendgrid.com.
- **MailerSend:** 500/month, but a card is required, "account approval is required", 1-day activity retention, and MailerSend branding ([pricing](https://www.mailersend.com/pricing)).
- **Sweego:** French and EU-hosted, 100/day free, unlimited domains, 7-day logs, signed webhooks, SMTP ([pricing](https://www.sweego.io/pricing), [home](https://www.sweego.io/)). It is a credible EU alternative, but it has no .NET SDK and the card requirement is unstated. Lettermint covers the same ground with clearer docs.

## 4. Local and CI testing without real mail

**Use Mailpit** ([repo](https://github.com/axllent/mailpit), v1.33.0, 2026-10-31). It is a single container with SMTP on 1025 and the UI/API on 8025 ([Docker docs](https://mailpit.axllent.org/docs/install/docker/)):

```yaml
# docker-compose.dev.yml
services:
  mailpit:
    image: axllent/mailpit
    ports: ["8025:8025"] # UI + API; SMTP 1025 only needs to be reachable on the compose network
    environment:
      MP_SMTP_AUTH_ACCEPT_ANY: 1
      MP_SMTP_AUTH_ALLOW_INSECURE: 1
```

**App config:** one `Email:Smtp` options section (`Host`, `Port`, `Username`, `Password`, `UseStartTls`, `From`), bound with the options pattern and overridden per environment through env vars (`Email__Smtp__Host=mailpit`, `Email__Smtp__Port=1025`, StartTLS off). In production the same keys point at `smtp.lettermint.co:587` with StartTLS on, and the token comes from a Dokploy secret. The provider is never named in code.

**Asserting in tests:** the Mailpit REST API ([API v1](https://mailpit.axllent.org/docs/api-v1/)):

- `DELETE /api/v1/messages` clears the inbox between tests.
- `GET /api/v1/search?query=to:"member@example.test"` finds the sign-in mail ([search filters](https://mailpit.axllent.org/docs/usage/search-filters/)).
- `GET /view/{ID}.txt` (or `latest`) returns the plain-text body, from which the test extracts the code.
- Chaos triggers (`PUT /api/v1/chaos`) can simulate SMTP failures to test the "couldn't send" path ([endpoint list, swagger](https://github.com/axllent/mailpit)).

In CI, run Mailpit as a service container (or via Testcontainers) with the same env vars.

**Alternative: smtp4dev** ([repo](https://github.com/rnwood/smtp4dev), 3.16.1, 2026-11-06). It is .NET-based; `docker run -p 5000:80 -p 2525:25 rnwood/smtp4dev`; messages are at `/api/messages`, with a testing guide covering C# ([installation](https://github.com/rnwood/smtp4dev/blob/master/docs/Installation.md), [testing](https://github.com/rnwood/smtp4dev/blob/master/docs/Testing.md), [API](https://github.com/rnwood/smtp4dev/blob/master/docs/API.md)). Either works; Mailpit's search API is better documented, which makes test assertions simpler.

## 5. Self-hosting SMTP on Hetzner

**Verdict: not realistic, and not worth it even later.**

- Hetzner blocks outbound **25 and 465** on all Cloud servers by default. Unblocking requires that you "have been with us for a month and paid your first invoice", plus a limit request reviewed "on a case-by-case basis". Port 587 is open for external mail services ([Hetzner Cloud FAQ](https://docs.hetzner.com/cloud/servers/faq/)). Direct delivery to recipients' MX servers needs port 25, so it can't work by 2026-11-12, and approval is not guaranteed after that.
- Even when unblocked, a fresh cloud IP has no sending reputation. You would also own PTR, SPF/DKIM/DMARC alignment, bounce handling, blocklist monitoring and warm-up, for ~100 emails/month where a code that lands in spam blocks sign-in. Spamhaus has a notice for Hetzner-hosted resolvers being refused for DNSBL queries ([Spamhaus](https://submit.spamhaus.org/resources/query-the-legacy-dnsbls-via-hetzner/)). That affects receiving, not sending, but it shows the extra operational surface. Hetzner publishes no IP-reputation guidance for mail ❓.

## 6. Flagged uncertainties

- ✅ **Lettermint compliance review time** (resolved 2026-11-06): the verification form says "Most reviews finish within a few hours". Our account was approved the same day.
- ⚠️ **Lettermint pricing fetch conflict:** one fetch of the pricing page returned extra text claiming a card is required. The visible page says "No credit card required" twice. Treated as no card; confirm at sign-up.
- ⚠️ Lettermint DNS: the overview lists 4 records and some knowledge-base guides list 3. Follow the dashboard.
- ⚠️ **Scaleway**: scaleway.com returned 403 to every fetch. Pricing (per-email price on Essential, domain limits, card requirement) is unverified; facts come from Scaleway's docs source on GitHub.
- ⚠️ **Brevo**: the help center, GDPR and privacy pages couldn't be loaded. The 300/day limit is from a search snippet. "Card required" on the pricing page is unusual for a free plan and unconfirmed.
- ⚠️ **Mailjet**: whether the free-plan logo is added to transactional API/SMTP mail, the free-plan domain limit, and log retention are not documented on the pages read. The help article returned 403.
- ⚠️ **Resend**: "3 domains" on Free looks recently changed; card requirement not stated; .NET SDK ownership on NuGet is an individual account.
- ⚠️ **Amazon SES**: the free tier changed to account-wide credits (6-month free plan), and new accounts default to the Essentials plan from 2026-07-21. Sandbox exit gets an "initial response within 96 hours".
- ⚠️ **SendGrid**: the pricing page redirect-loops; only a free trial is described.
- ❓ Mailgun Free + EU region; Postmark Free domain count (flattened table).
- ❓ Real volume: depends on session lifetime. Measure in the pilot before choosing a paid tier.

## 7. Sources

All checked 2026-11-06. GitHub data via `gh api`.

- Lettermint: https://lettermint.co/ · https://lettermint.co/pricing · https://lettermint.co/dpa · https://lettermint.co/subprocessors · https://lettermint.co/docs/guides/send-email-with-smtp · https://lettermint.co/docs/guides/send-email-with-dotnet · https://lettermint.co/docs/platform/domains/introduction · https://lettermint.co/docs/platform/onboarding/compliance-verification · https://lettermint.co/docs/platform/webhooks/introduction · https://lettermint.co/docs/platform/webhooks/events · https://www.nuget.org/packages/Lettermint · https://github.com/lettermint/lettermint-dotnet
- Mailjet: https://www.mailjet.com/pricing/ · https://www.mailjet.com/security-privacy/ · https://documentation.mailjet.com/hc/en-us/articles/360043048393 (403) · https://github.com/mailjet/mailjet-apiv3-dotnet
- Scaleway TEM (docs source): https://github.com/scaleway/docs-content/tree/main/pages/transactional-email (`faq.mdx`, `concepts.mdx`, `how-to/manage-tem-plans.mdx`, `reference-content/{tem-capabilities-and-limits,data-privacy,smtp-configuration}.mdx`); https://www.scaleway.com/en/pricing/managed-services/ (403)
- Resend: https://resend.com/pricing · https://resend.com/legal/dpa · https://resend.com/docs/dashboard/domains/introduction · https://resend.com/docs/dashboard/domains/regions · https://resend.com/docs/send-with-dotnet · https://resend.com/docs/send-with-smtp · https://www.nuget.org/packages/Resend
- Amazon SES: https://aws.amazon.com/ses/pricing/ · https://docs.aws.amazon.com/ses/latest/dg/request-production-access.html · https://docs.aws.amazon.com/ses/latest/dg/send-email-authentication-dkim-easy.html · https://docs.aws.amazon.com/ses/latest/dg/smtp-connect.html
- Brevo: https://www.brevo.com/pricing/ · https://developers.brevo.com/docs/send-a-transactional-email · https://help.brevo.com/hc/en-us/articles/208580669 (403)
- Others: https://postmarkapp.com/pricing · https://www.mailgun.com/pricing/ · https://www.mailgun.com/legal/dpa/ · https://www.twilio.com/en-us/sendgrid · https://www.mailersend.com/pricing · https://www.sweego.io/pricing · https://www.sweego.io/
- Testing: https://github.com/axllent/mailpit · https://mailpit.axllent.org/docs/install/docker/ · https://mailpit.axllent.org/docs/api-v1/ · https://mailpit.axllent.org/docs/usage/search-filters/ · https://github.com/rnwood/smtp4dev (`docs/Installation.md`, `docs/Testing.md`, `docs/API.md`) · https://www.nuget.org/packages/MailKit
- Hetzner / reputation: https://docs.hetzner.com/cloud/servers/faq/ · https://submit.spamhaus.org/resources/query-the-legacy-dnsbls-via-hetzner/
