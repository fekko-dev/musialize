# One deployment serves multiple Groups

All data belongs to a Group, and one deployment serves several Groups. A person can be a Member of more than one Group. We chose this over running one copy of the app per Group because groups of musicians are unlikely to host their own servers, and adding Group ownership to every table and query after launch is expensive.

## Consequences

- There's no self-service creation or discovery of Groups yet. Groups are set up by a Super admin.
- A Group's name and logo are stored with the Group, not in the code. Separate addresses per Group (and per-Group installed app icons) remain possible later.
- Until a second Group arrives, the installed app's name and icon come from deployment settings, so the pilot deployment can carry the pilot group's branding without it being in the repository. Once Musialize has its own domain, branding moves to per-Group, chosen by address, on that same deployment. We don't run a separate copy of the app per Group.
