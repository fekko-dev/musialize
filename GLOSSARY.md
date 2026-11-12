# Musialize

Musialize ("music" + "socialize") helps a collective of musicians decide which songs to play, who plays which part, and what to work on at the next rehearsal.

## Language

### People

**Group**:
A collective of musicians who pick songs together and form a different line-up for each song.
_Avoid_: Band, ensemble, club, collective

**Member**:
A person who belongs to a Group. One person can be a Member of several Groups.
_Avoid_: Musician, player, user

**Account**:
How a person signs in, independent of any Group. One Account can be linked to Members of several Groups, but to at most one Member per Group.
_Avoid_: User, profile, login

**Unclaimed Member**:
A Member that isn't linked to an Account yet. It exists by name so that others can sign it up for Parts.
_Avoid_: Placeholder, guest, ghost

**Claim**:
Linking an Account to an Unclaimed Member, so the person takes over its Sign-ups, Suggestions and Comments. A Group admin can undo or correct a Claim.
_Avoid_: Link, merge, register

**Invite link**:
The Group's link through which a person Joins it. A Group has one at a time; replacing it makes the old one stop working.
_Avoid_: Invitation, join code

**Join**:
Becoming a Member of a Group through its Invite link, either by Claiming an Unclaimed Member or by adding yourself by name.
_Avoid_: Register, sign up (a Sign-up is a commitment to a Part)

**Group admin**:
A Member who decides which Songs enter the Repertoire and manages the Group's Members. A Group has one or more Group admins.
_Avoid_: Organizer, board, moderator, committee, admin on its own

**Super admin**:
A person who manages a whole Musialize deployment and the Groups on it, above any single Group.
_Avoid_: Admin on its own, operator, superuser, owner, host

### Songs

**Song**:
A piece of music a Group is considering or playing, described by title, artist, key, BPM and links.
_Avoid_: Track, number, tune

**Suggestion**:
A Song a Member has proposed to the Group that the Group admins have not yet accepted.
_Avoid_: Proposal, request

**Repertoire**:
The Songs the Group admins have accepted for the Group to play.
_Avoid_: Library, catalogue

**Declined**:
Describes a Suggestion the Group admins decided not to play. A Declined Song can be suggested again.
_Avoid_: Rejected, archived

**Comment**:
A Member's remark on a Song, for example "this is a bit repetitive, maybe we can spice it up". Comments belong to the Song as a whole, not to a Part or a Sign-up.
_Avoid_: Feedback, message, post, note

### Line-ups

**Part**:
One role needed to play a specific Song, such as "lead vocals" or "higher harmony", optionally with a description such as "brushes" or "capo 2". A Part is either required or optional, and a Song can have several Parts for the same instrument.
_Avoid_: Instrument, role, voice

**Sign-up**:
A Member's commitment to play a Part if the Song makes it onto a Setlist, optionally with a note such as "feel free to take this off me". A Sign-up is regular unless it is a Backup.
_Avoid_: Assignment, claim, registration

**Backup**:
A Sign-up from a Member who could play a Part if needed: when the regular player can't, or when nobody has signed up regularly. A Part with only Backups is still open.
_Avoid_: Reserve, substitute, stand-in

**Line-up**:
All Sign-ups for one Song's Parts.
_Avoid_: Cast, roster, band

**Filled**:
A Song whose required Parts all have at least one regular Sign-up.
_Avoid_: Complete, full, ready

### Playing

**Review Meeting**:
A meeting where the Group goes through Suggestions together, discusses them and plans upcoming Rehearsals.
_Avoid_: Borrel, review, meetup

**Event**:
A dated occasion where the Group plays together. Rehearsal and Performance are kinds of Event.
_Avoid_: Meeting, session, occasion

**Rehearsal**:
An Event where the Group practises Songs.
_Avoid_: Practice, session, jam

**Performance**:
An Event where the Group plays for an audience, a few times a year.
_Avoid_: Gig, concert, show

**Setlist**:
The ordered Songs the Group plans to play at one Event.
_Avoid_: Playlist, programme, rehearsal list, rehearsal plan
