# Send transactional email through Lettermint over SMTP

The backend sends transactional email, such as one-time sign-in codes for an Account, through Lettermint's SMTP relay with MailKit. Of the providers we compared ([research](../research/transactional-email-providers.md)), Lettermint is a Dutch company that processes mail in NL/DE, includes a DPA in its terms, and has a free tier of 300 emails a month with webhooks and 28 days of message history. We use SMTP with MailKit instead of Lettermint's .NET SDK ([comparison](../research/lettermint-smtp-vs-sdk.md)): the same code path sends to Mailpit locally and in CI, changing provider is a config change, and the SDK is only a few months old. SMTP still gives us Lettermint's message ID, in its `250` reply. Mailjet was the runner-up, with a larger free tier but a non-EU parent company; Resend is the fallback if EU residency stops mattering.

## Consequences

- SMTP settings come from configuration, not code. Production uses port **587** with STARTTLS: Hetzner Cloud blocks outbound 25 and 465, so Lettermint's recommended port 465 does not work. 587 was tested from the server. The Lettermint token is a secret and stays out of the repo.
- Locally and in CI the SMTP settings point at a mail catcher such as Mailpit, so no real mail is sent outside production.
- `fekko.dev` is registered at Porkbun, but its DNS is managed in Cloudflare, so Lettermint's DKIM, Return-Path and DMARC records live there. They must be **DNS only** (grey cloud): a proxied CNAME is answered with Cloudflare's own addresses, and DKIM verification fails.
- The free tier allows one sending domain. A domain per Group needs the Growth plan (up to 30 domains). More than 300 emails a month needs Starter or higher.
- Lettermint reviews every new account before it can send to outside addresses. Ours is approved. A new provider account would need the same lead time.

## Still to verify while building

- Real monthly volume during the pilot, which depends on how long sign-in sessions last.
