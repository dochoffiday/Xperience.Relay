# Drafts and publishing

Pages and reusable content items in Xperience by Kentico are versioned, and every language has its own versions. Relay edits through drafts, the way the admin UI does. Knowing the model makes the commands easy to predict.

## The four statuses

A language variant of a page or item has a **latest version**, and that version has one of four statuses:

| Status | Meaning |
|---|---|
| `InitialDraft` | Created, never published. Nothing is live. |
| `Draft` | A change in progress on top of a published version. The published version is still live. |
| `Published` | Live. |
| `Unpublished` | Was published, now offline. The content is kept. |

A published page can have a newer draft at the same time: the latest version is the `Draft`, and a published version still exists. That is why [`includeStatus`](../commands/web-pages/query-web-page-items.md) reports two things: the status of the latest version, and whether anything is published at all.

## What Kentico allows

Kentico's own operations are strict, and they answer a plain `false` to anything they won't do:

- A draft can only be made from a published or unpublished version.
- Only a draft can be published.
- Only a published version can be unpublished, and not while it has a newer draft.
- A draft over a published version can be discarded, which puts the item back as it was. A draft over an unpublished item can't be undone: Kentico turns the item into an `InitialDraft` in place.

Because `false` doesn't say whether there was nothing to do or something went wrong, Relay reads the state first and decides what to do, then reports what actually happened.

## What each command does

| Command | The item is | Result |
|---|---|---|
| `create-web-page` | new | `InitialDraft`, or `Published` with `publishAfterCreate` |
| `create-content-item` | new | `Published` |
| `update-*`, `rename-asset`, `reoptimize-asset` | `Published` | Edited in a draft, then `Published` again |
| | `Published` with a newer `Draft` | The change goes into that draft; the live version is unchanged |
| | `InitialDraft` | Stays an `InitialDraft`, with the change |
| | `Unpublished` | Stays unpublished, with the change in a draft |
| `publish-*` | a draft | `Published` |
| | `Unpublished` | A draft is made, then `Published` |
| | `Published` | Nothing; the answer says it is already published |
| `unpublish-*` | `Published` | `Unpublished` |
| | `Unpublished` or only a draft | Nothing; the answer says so |
| | `Published` with a newer `Draft` | Refused until the draft is published or discarded |
| `delete-*` | any | Recycle bin (pages and items), or gone for good with `permanently` (pages) |

The rule behind the edits: **an item that was live goes live again with the edit; anything else stays as it was.** Editing a draft or an unpublished item never publishes it.

## Saying whether the change is live

When an edit leaves the page or item not live, the message says so:

- "Saved as a draft; it is not published." for something that was never published or is unpublished.
- "Saved as a draft; the published version is unchanged." for a live page that already had a pending draft.

## Failures leave nothing behind

An edit is made of several steps: make a draft, save the change, publish. Every step is checked.

- If Kentico declines a step, the result is a failure that says which one ("Kentico would not create a draft of web page 12 (en) to edit.").
- A draft that was made for an edit that then failed is discarded again, and a draft that was already there is kept as it was.
- Kentico's cached copy of the item is dropped after a failure, so the next edit doesn't write the refused value again.
- If only the final publish fails, the change stays saved as a draft and the live version is unchanged. It is reported as a failure, never as a success.

A page with a scheduled publish is one case where Kentico won't make a draft; the result then says so.

## Reading: latest or published

| Command | Reads |
|---|---|
| `get-page`, `get-content` | The **latest** version, drafts included: what editors see. |
| `query-web-page-items`, `query-reusable-items` | The latest version by default; `publishedOnly: true` reads the **published** version and leaves out items that have none. |
| `search-content` | Published items only. |

## Languages

Each language variant has its own versions and its own status. An edit, a publish or an unpublish acts on the language you name, and leaves the others alone. See [Defaults and languages](defaults-and-languages.md).
