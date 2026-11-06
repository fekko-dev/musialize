# Members can exist without an Account

A Member can exist in a Group before anyone signs in as that person (an Unclaimed Member). A person later Claims it through the Group's invite link while signed in to their Account; signing in with an email code is what confirms the Claim ([ADR 0007](0007-passwordless-sign-in-with-cookie-sessions.md)). We chose this so a Group can move over from a shared document full of names: existing Sign-ups can be imported, and Organizers can sign people up during a Review Meeting before they've signed in. The alternative was requiring an Account before anyone appears in a Line-up.

## Consequences

- Anyone with the invite link can Claim any unclaimed name. That's acceptable for a trusted group, because an Organizer can undo or move a Claim. The same correction covers a person who changes email address.
- Member and Account are separate concepts throughout, never one identity.
