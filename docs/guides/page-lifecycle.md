# Create, edit and publish a page

This walk-through follows one page from creation to deletion, with the command for each step and what comes back. The calls are real ones, run against the Dancing Goat sample site. The page is an article under **Articles** (page 1), and the IDs and messages below come from that run.

Every step is a [command](../commands/index.md) sent as `{ "verb": ..., "parameters": ... }`; only the `parameters` are shown. To follow along you need a key with the **Write** tier.

## 1. Create the page as a draft

[`create-web-page`](../commands/web-pages/index.md#create-web-page) creates a page under a parent and fills in its fields. Here the parent is the Articles section:

```json
{
  "websiteChannelName": "DancingGoatPages",
  "parentWebPageItemId": 1,
  "contentTypeName": "DancingGoat.ArticlePage",
  "displayName": "Cold brew at home",
  "fields": {
    "ArticleTitle": "Cold brew at home",
    "ArticlePageSummary": "Steep coarse grounds in cold water overnight for a smooth, low-acid coffee.",
    "ArticlePageText": "<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>",
    "ArticlePagePublishDate": "2026-10-01"
  }
}
```

The answer is `Created web page 'Cold brew at home' (ID=68), draft.` and `data.webPageItemId` is **68**, the ID every later step uses as `webPageId`. The page is an `InitialDraft`: it exists in the admin UI but not on the live site. (Add `"publishAfterCreate": true` to publish it in the same step.)

If a field name is wrong, or a value doesn't fit, nothing is created and the answer lists every problem: `Field 'ArticleSubtitle' was not found on content type 'DancingGoat.ArticlePage'.`

## 2. Read it back

[`get-page`](../commands/web-pages/index.md#get-page) returns the page's fields as they are in its latest version, drafts included:

```json
{ "webPageId": 68 }
```

[`get-page-info`](../commands/web-pages/index.md#get-page-info) returns only the system fields (type, tree path, parent, channel) when you don't need the values.

## 3. Change a field

[`update-web-page`](../commands/web-pages/index.md#update-web-page) edits through a draft. The page has never been published, so the change goes into its draft and stays there:

```json
{
  "webPageId": 68,
  "fields": {
    "ArticleTitle": "Cold brew at home: a beginner's guide",
    "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve."
  }
}
```

The answer: `Updated 2 field(s) on web page 68. Saved as a draft; it is not published.`

## 4. Publish it

[`publish-web-page`](../commands/web-pages/index.md#publish-web-page) puts the draft live:

```json
{ "webPageId": 68 }
```

The answer: `Published web page 68 (en).` Sending the same command again is harmless: `Web page 68 (en) is already published.`

## 5. Change the URL

[`update-slug`](../commands/web-pages/index.md#update-slug) changes the last part of the page's URL. The page is published, so Relay publishes the change too, and the new URL is live when the answer returns:

```json
{ "webPageId": 68, "slug": "cold-brew-guide" }
```

The answer: `Updated slug on web page 68 to 'cold-brew-guide'.`

## 6. Edit a published page

Editing a published page works the same way as step 3, but the change is published again automatically, because the page was live. The answer carries no "saved as a draft" caveat:

```json
{ "webPageId": 68, "fields": { "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve - no special equipment." } }
```

The answer: `Updated 1 field(s) on web page 68.`

## 7. Check where it stands

[`query-web-page-items`](../commands/web-pages/index.md#query-web-page-items) shows a page's status and URL together:

```json
{
  "websiteChannelName": "DancingGoatPages",
  "contentTypeNames": [ "DancingGoat.ArticlePage" ],
  "columns": [ "WebPageItemID", "ArticleTitle" ],
  "whereEquals": { "WebPageItemID": 68 },
  "includeUrls": true,
  "includeStatus": true
}
```

```json
{
  "WebPageItemID": 68,
  "ArticleTitle": "Cold brew at home: a beginner's guide",
  "WebPageStatus": "Published",
  "WebPageIsPublished": true,
  "WebPageRelativeUrl": "~/articles/cold-brew-guide",
  "WebPageAbsoluteUrl": "https://localhost/articles/cold-brew-guide"
}
```

## 8. Take it offline, and bring it back

[`unpublish-web-page`](../commands/web-pages/index.md#unpublish-web-page) takes the page offline and keeps its content. [`publish-web-page`](../commands/web-pages/index.md#publish-web-page) brings it back: for an unpublished page Relay makes a draft first, then publishes it.

```json
{ "webPageId": 68 }
```

The answers: `Unpublished web page 68 (en).` and then `Published web page 68 (en).`

## 9. Delete it

[`delete-web-page`](../commands/web-pages/index.md#delete-web-page) sends the page to the recycle bin, where it can be restored from the admin UI. Add `"permanently": true` to remove it for good. Pages below the deleted page go with it.

```json
{ "webPageId": 68, "languageName": "en" }
```

The answer: `Deleted web page 68 (en).`

## The same flow in C#

```csharp
using System.Text.Json;
using Xperience.Relay.Client;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;

public class ArticlePublisher(RelayClient relay)
{
    public async Task<int> PublishAsync(string title, string summary)
    {
        var created = await relay.ExecuteAsync(new CreateWebPageCommand
        {
            WebsiteChannelName = "DancingGoatPages",
            ParentWebPageItemId = 1,
            ContentTypeName = "DancingGoat.ArticlePage",
            DisplayName = title,
            Fields = new()
            {
                ["ArticleTitle"] = JsonSerializer.SerializeToElement(title),
                ["ArticlePageSummary"] = JsonSerializer.SerializeToElement(summary),
            },
        });

        Check(created);
        var pageId = created.GetData<CreateWebPageResult>()!.WebPageItemId;

        Check(await relay.ExecuteAsync(new PublishWebPageCommand { WebPageId = pageId }));
        Check(await relay.ExecuteAsync(new UpdateSlugCommand { WebPageId = pageId, Slug = Slugify(title) }));

        return pageId;
    }

    private static void Check(RelayCommandResult result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error);
        }
    }

    private static string Slugify(string title) => title.ToLowerInvariant().Replace(' ', '-');
}
```

The steps depend on each other (each needs the ID the first one returns), so they are separate calls rather than a [batch](batches.md).

## What to read next

- [Drafts and publishing](../concepts/drafts-and-publishing.md), for the rules behind every step
- [Query content](querying.md), for finding the pages you want to act on
- [Upload files](uploading-assets.md), for pages and items with images and documents
