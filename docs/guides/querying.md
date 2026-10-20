# Query content

Relay offers several ways to find things. This page helps you pick one, and shows how the two main query commands work.

## Which command

| You want to | Use | Tier |
|---|---|---|
| Find **pages** by type, field value, status or URL | [`query-web-page-items`](../commands/web-pages/query-web-page-items.md) | Read |
| Find **reusable content items** by type or field value | [`query-reusable-items`](../commands/content-items/query-reusable-items.md) | Read |
| Find items that **contain some text** | [`search-content`](../commands/content-items/search-content.md) | Read |
| Read **one** page or item with its fields | [`get-page`](../commands/web-pages/get-page.md), [`get-content`](../commands/content-items/get-content.md) | Read |
| See what **refers to** an item | [`get-content-item-usage`](../commands/content-items/get-content-item-usage.md) | Read |
| List **tags** or taxonomies | [`get-tags`](../commands/taxonomies/get-tags.md), [`get-taxonomies`](../commands/taxonomies/get-taxonomies.md) | Read |
| Read **form submissions** | [`get-form-submissions`](../commands/forms-and-contacts/get-form-submissions.md) | Read |
| Read any **registered object type** | [`query-custom-objects`](../commands/custom-objects/query-custom-objects.md) | Admin |
| Ask the **database** anything else | [`query-sql`](../commands/sql/query-sql.md) | Admin |

Prefer the typed commands. They need only a Read key, they follow Kentico's rules for language fallback and versions, and they don't depend on the database's layout.

## Columns and filters

`query-web-page-items` and `query-reusable-items` work the same way:

```json
{
  "websiteChannelName": "DancingGoatPages",
  "contentTypeNames": [ "DancingGoat.ArticlePage" ],
  "columns": [ "WebPageItemID", "ArticleTitle" ],
  "whereEquals": { "ArticleTitle": "On Roasts" }
}
```

- **`contentTypeNames`** lists one or more content types, by code name. The answer is the union of their items. At least one is required.
- **`columns`** names what you want back, and at least one is required. Use the content type's field names, plus system columns: `WebPageItemID`, `WebPageItemGUID`, `WebPageUrlPath` for pages, and `ContentItemID`, `ContentItemGUID`, `ContentItemName` for reusable items. The answer holds only these columns.
- **`whereEquals`** keeps the items whose columns equal the given values, all conditions together (AND). The column must **also be listed in `columns`**, because the filter runs over the selected columns; otherwise the query fails with "Invalid column name". Values are strings, numbers or booleans. There are no other operators: for ranges, patterns or joins, use [`query-sql`](../commands/sql/query-sql.md).
- There is **no paging**. The answer holds every match, so narrow a query on a large type with `whereEquals`.

The answer is a list of objects, one per item, with just the columns you asked for:

```json
{
  "items": [
    { "WebPageItemID": 12, "ArticleTitle": "On Roasts" }
  ]
}
```

Reference and tag fields come back as real JSON (an array of objects), not as a string that contains JSON.

## Latest or published

By default the query reads each item's **latest version**, drafts included, which is what editors see. Set `publishedOnly: true` to read the **published** version of each item instead: its field values are the published ones, and items that have no published version are left out.

To find out whether pages are published without a second query, add `includeStatus: true` to `query-web-page-items`. Each page then gets `WebPageStatus` (the latest version's status) and `WebPageIsPublished`. For reusable items, run the query twice, once with `publishedOnly`, and compare the `ContentItemID`s: an item is published if it appears in the second answer.

## URLs

`query-web-page-items` with `includeUrls: true` adds `WebPageRelativeUrl` and `WebPageAbsoluteUrl` to every page. They are formatted the way the site serves them (lower case, with the channel's domain), and come from the same query, so a thousand pages don't cost a thousand lookups. `WebPageItemTreePath` is the page's place in the content tree, which is not its URL.

## Searching text

[`search-content`](../commands/content-items/search-content.md) looks for text, without regard to case, in every text, long text and rich text field of the **published** items of one content type:

```json
{ "contentTypeName": "DancingGoat.ArticlePage", "websiteChannelName": "DancingGoatPages", "filter": "cold brew" }
```

Each match carries the whole value of the fields that matched. The matching is done by Relay after Kentico returns the published items, so use it on types with a modest number of items.

## Languages

Queries read the language you name (`languageName`, default `en`) with Kentico's language fallback: an item with no variant in that language comes back with the values of the language it falls back to. See [Defaults and languages](../concepts/defaults-and-languages.md).

## Next

- [Types](../reference/types.md), for the shape of each answer
- [Run several commands at once](batches.md), to send several queries in one request
