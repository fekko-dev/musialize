# Passwordless sign-in with email codes and cookie sessions

An Account signs in by entering an email address and then a 6-digit code sent to it; there are no passwords. Signing in and signing up are the same flow: the backend always sends a code, and creates the Account only when the code is verified. We build this ourselves on ASP.NET Core cookie authentication (`AddCookie`) with our own tables for Accounts, sign-in codes and Group invite links. We compared this with ASP.NET Core Identity, Keycloak and hosted providers ([research](../research/passwordless-sign-in.md)). Identity's built-in email code can be reused within its 6–9 minute window and its ready-made endpoints assume passwords, so we would need our own code table anyway. A hosted provider would only replace the code-and-cookie part, while the invite link, Claims and Account–Member links stay ours, and it adds a redirect-based sign-in flow and a second place where user data lives. Identity Core with our own code table is the runner-up if passkeys become urgent.

The session is an encrypted cookie issued by our own API, which also authenticates SignalR connections without extra setup. Because the SPA and the API share one origin, there is no CORS policy and no bearer token in the browser.

A Group has one invite link at a time. It works for anyone, never expires on its own, and stops working as soon as a Group admin replaces it. A person must be signed in to Claim: someone who opens the invite link while signed out signs in first and then Claims. Signing in already proves the email address, so a Claim needs no second code. An Account can be linked to at most one Member per Group, and to Members of several Groups.

## Consequences

- We own the security code, so the checklist in the research (§4) is a requirement, not a suggestion. In short:
  - codes come from a cryptographic random generator, expire after 10 minutes, work once, are stored as a keyed hash and are compared in constant time;
  - a code dies after 5 wrong guesses, and code requests are limited per email (in the database) and per IP (rate limiting middleware);
  - the response to a code request is the same whether or not an Account exists.
- The cookie is `__Host-` prefixed, `HttpOnly`, always `Secure` (TLS ends at Dokploy's proxy) and `SameSite=Strict`, and lasts at most 30 days with no sliding renewal. A "Remember me" checkbox on sign-in decides whether it survives closing the browser. Unchecked, it is a browser-session cookie, because EU guidance treats login cookies that outlive the browser session as needing consent.
- `SameSite` alone does not stop CSRF from other `*.fekko.dev` apps, which count as the same site. A small middleware rejects state-changing requests and WebSocket upgrades whose `Sec-Fetch-Site` or `Origin` shows another origin. Nothing changes state on GET.
- The cookie holds only the Account ID. Group membership and Group admin rights are checked in the database on each request and hub call, so an undone Claim takes effect immediately.
- Invite tokens have 256 bits of entropy, are stored hashed, and travel in the URL fragment (`/join#<token>`) so they stay out of server logs and `Referer` headers.
- Data Protection keys must be persisted in Postgres, otherwise every deploy signs everyone out. Local development uses Vite's dev-server proxy so it is same-origin too.
- Emailed codes are not a recognised authenticator under NIST SP 800-63B-4, which forbids email for out-of-band authentication. We accept that for a trusted group where a Group admin can undo a Claim. Passkeys are the upgrade path: .NET 10 Identity's passkey support works through `UserManager` with a pluggable store, so it can be added over our Account table later.
- Integration tests use `WebApplicationFactory` with a fake email sender that captures the code, and `FakeTimeProvider` for expiry. Our code and cookie auth read the injected `TimeProvider`; the rate limiters do not.

## Still to verify while building

- Whether SignalR's `CloseOnAuthenticationExpiration` closes connections when a cookie session expires.
- Monthly email volume (see [ADR 0006](0006-lettermint-smtp-for-transactional-email.md)): Accounts that leave "Remember me" unchecked get a code on every new browser session.
