# User journeys for the pilot

This maps how people move through Musialize during the [pilot](vision.md#pilot-review-meeting-12-november-2026). It is a map, not a spec: details belong in tickets. Terms are defined in [GLOSSARY.md](../../GLOSSARY.md).

## Legend

```mermaid
flowchart LR
    s([Start or end]) ~~~ p[[Screen]] ~~~ a(Action) ~~~ d{Decision} ~~~ o("Organizer only"):::org

    classDef org fill:#fde3c8,stroke:#d9822b,color:#000
```

## 1. Joining

```mermaid
flowchart TD
    G0(["Group, invite link and first Organizers set up (script)"]) --> S
    S[["Members: add names from the shared document (Organizer)"]]:::org --> A("Share the invite link in the group chat (Organizer)"):::org
    A --> B([Open the invite link])
    B --> V{Link still valid?}
    V -- no --> VX[[Join: link replaced, ask an Organizer]]
    V -- yes --> C{Signed in?}
    C -- no --> SI[[Sign in]]
    SI --> SI1(Enter email, choose Remember me)
    SI1 --> EC[[Enter code]]
    EC --> EC1(Enter the 6-digit code from the email)
    EC1 --> F
    C -- yes --> F{Already a Member of this Group?}
    F -- yes --> O[[Overview]]
    F -- no --> J[[Join]]
    J --> G{Your name listed?}
    G -- yes --> H(Claim it: take over its Sign-ups, Suggestions and Comments)
    G -- no --> I(Add yourself by name)
    H --> O
    I --> O
    H -. wrong name .-> K("Members: undo or move the Claim (Organizer)"):::org

    classDef org fill:#fde3c8,stroke:#d9822b,color:#000
```

Organizers Claim their names through the invite link like everyone else; the Organizer role itself is assigned by script, both at the start and whenever an Organizer is added or removed. Members can't switch names themselves; only an Organizer corrects a Claim. Opening the app without a valid session goes to Sign in and then to the Overview. Someone who signs in without ever using an invite link sees "You're not in a Group yet, ask for an invite link".

## 2. Between meetings

At home, on desktop or phone. Any Member can edit a Song's details and Parts.

```mermaid
flowchart TD
    O[[Overview]] --> N0(Suggest a Song)
    O --> P0(Open a Song)
    N0 --> N[[Suggest a Song]]
    N --> N1(Fill in title, artist, key, BPM, link and Parts)
    N1 --> DC{Same title and artist already exists?}
    DC -- yes --> DX(Open the existing Song, or clear the form)
    DX -- clear --> N
    DC -- no --> SV(Save)
    SV --> P[[Song page]]
    DX -- open --> P
    P0 --> P

    P --> SU("Sign up for a Part as regular or Backup, optional note")
    SU --> SR(Switch between regular and Backup, after confirming)
    SU --> W(Withdraw, with undo toast)
    P --> CM(Write a Comment)
    CM --> CE(Edit your own Comment, shown as edited)
    CM --> CD(Delete your own Comment, after confirming)
    CM --> CA("Delete anyone's Comment (Organizer)"):::org
    P --> ED(Edit Song details and Parts, then Save or Cancel)
    ED --> CF{Changed by someone else since you started?}
    CF -- no --> P
    CF -- yes --> RJ(Save rejected: your input kept, a banner shows the change)
    RJ --> ED

    classDef org fill:#fde3c8,stroke:#d9822b,color:#000
```

A Backup is allowed on any Part, including one with nobody on it, so a Member who plays several instruments can mark the Parts they could take. A Part with only Backups counts as open and shows who could play it. Line-up changes never conflict with edit mode. How signing up looks on the Song page (buttons, a menu or a sheet, and fitting Parts, Sign-ups and notes on a phone) is decided when that screen is designed.

The Overview shows the fill status of required Parts ("4/6", or Filled), plus a separate hint for optional Parts that have no regular Sign-up. How that hint looks (an icon with a count, for example) is left to the screen design.

## 3. Review Meeting

A projector shows an Organizer's screen and everyone also has the app open on their phone. Each device navigates on its own.

```mermaid
flowchart TD
    A[["Overview, filtered to Suggested, on the projector (Organizer)"]]:::org --> B(Open a Suggestion)
    B --> S[["Song page, showing e.g. 3 of 12"]]
    S --> C(Play the link and discuss)
    C --> D(Members sign up on their phones)
    C --> E("Sign up other Members, including Unclaimed ones (Organizer)"):::org
    D --> F("Accept, decline, or leave as Suggested (Organizer)"):::org
    E --> F
    F -. wrong click .-> U("Undo toast (Organizer)"):::org
    F --> L{Last Song in the list?}
    L -- no --> X(Next)
    X --> S
    L -- yes --> Y{Updates since the list loaded?}
    Y -- yes --> R("Next becomes View new: reloads the list")
    R --> B
    Y -- no --> Z([Done])

    classDef org fill:#fde3c8,stroke:#d9822b,color:#000
```

**Lists change only when you ask.** The Overview is sorted newest first. While it's open, the Songs in it update live (fill status, status, details), but none are added, removed or reordered. The list reloads when you open the app, change the filter, or reach the end: on the last Song, Next becomes "View new" when there are updates since the list loaded. Previous and Next follow the same list, so a Song that was just accepted stays where it is and Next still works. There is no banner for updates in the pilot; "View new" covers going through the list in the Review Meeting. Live updates therefore only refresh data and never move rows.

The decision has no preconditions: an Organizer can accept a Song that isn't Filled, and can change the decision later. Right after deciding, an undo toast fixes a wrong click in one tap. The Song shows the decision date, and every change is recorded in Song activity.

## 4. Rehearsal

On a phone, in a room where the Song is about to be played.

```mermaid
flowchart TD
    A([Open the app]) --> B[[Overview]]
    B --> B1(Filter to Accepted, then search or scroll)
    B1 --> D[[Song page]]
    D --> E(See who plays each Part, Backups and notes)
    D --> F(Open links, such as sheet music in Google Drive)
    D --> G(Sign up or withdraw on the spot)
```

## Screen map

Each node is a screen. Edit mode, sign-up menus and the duplicate warning live inside the screen they belong to.

```mermaid
flowchart TD
    INV([Invite link]) --> JOIN[[Join]]
    APP([Open the app]) --> OV[[Overview]]
    APP -- signed out --> SI[[Sign in]]
    JOIN -- signed out --> SI
    SI --> CODE[[Enter code]]
    CODE -- back to Join, the invite is remembered --> JOIN
    CODE -- has a Member --> OV
    CODE -- no Member in any Group --> NG[[Not in a Group yet]]
    JOIN --> OV
    OV --> SUG[[Suggest a Song]]
    OV --> SONG[[Song page]]
    SUG --> SONG
    SONG -- Previous and Next --> SONG
    SONG -- back --> OV
    OV -- menu --> MEM[["Members (Organizer)"]]:::org

    classDef org fill:#fde3c8,stroke:#d9822b,color:#000
```

The Overview header holds the Group's name and logo and a menu with "Sign out", plus "Members" for Organizers. The pilot has one Group, so there is no Group picker yet.

## Later

Not part of the pilot, but noted so the journeys can grow into them:

- **Create an Unclaimed Member inline** while signing someone up, with a dialog that asks the Organizer to check the spelling of a new name.
- **Tell Backups when a Part opens up** because its last regular player withdrew (needs notifications).
- **An in-app indicator for list updates**, such as a banner or a refresh button with a count. This belongs with live updates, not notifications; it was cut only to keep the pilot small.
- **Notify Members about new Suggestions**, outside the app.
