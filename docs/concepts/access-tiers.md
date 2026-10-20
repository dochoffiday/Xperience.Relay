# Access tiers and API keys

Every request carries an **API key**, and every key has one **tier**. A command can only be run by a key whose tier is at least the command's tier.

| Tier | What it is for |
|---|---|
| **Read** | Fetching and querying content. Nothing a Read key does changes the site. |
| **Write** | Everything in Read, plus creating, updating, moving, publishing and deleting content, taxonomies and assets. |
| **Admin** | Everything in Write, plus raw SQL, generic custom-object access and bulk contact deletion. |

Each tier includes the ones below it.

## Which command needs which tier

<!-- relay:begin tiers -->
| Tier | Commands |
|---|---|
| **Read** | [`get-content`](../commands/content-items/get-content.md), [`get-content-info`](../commands/content-items/get-content-info.md), [`get-content-item-usage`](../commands/content-items/get-content-item-usage.md), [`get-form-submissions`](../commands/forms-and-contacts/get-form-submissions.md), [`get-page`](../commands/web-pages/get-page.md), [`get-page-info`](../commands/web-pages/get-page-info.md), [`get-tags`](../commands/taxonomies/get-tags.md), [`get-taxonomies`](../commands/taxonomies/get-taxonomies.md), [`query-reusable-items`](../commands/content-items/query-reusable-items.md), [`query-web-page-items`](../commands/web-pages/query-web-page-items.md), [`search-content`](../commands/content-items/search-content.md) |
| **Write** | Everything in Read, plus: [`create-content-item`](../commands/content-items/create-content-item.md), [`create-tag`](../commands/taxonomies/create-tag.md), [`create-taxonomy`](../commands/taxonomies/create-taxonomy.md), [`create-web-page`](../commands/web-pages/create-web-page.md), [`delete-content-item`](../commands/content-items/delete-content-item.md), [`delete-tag`](../commands/taxonomies/delete-tag.md), [`delete-taxonomy`](../commands/taxonomies/delete-taxonomy.md), [`delete-web-page`](../commands/web-pages/delete-web-page.md), [`get-content-hub-folder`](../commands/content-items/get-content-hub-folder.md), [`move-content-item`](../commands/content-items/move-content-item.md), [`move-tag`](../commands/taxonomies/move-tag.md), [`move-web-page`](../commands/web-pages/move-web-page.md), [`publish-content-item`](../commands/content-items/publish-content-item.md), [`publish-web-page`](../commands/web-pages/publish-web-page.md), [`rename-asset`](../commands/assets/rename-asset.md), [`reoptimize-asset`](../commands/assets/reoptimize-asset.md), [`unpublish-content-item`](../commands/content-items/unpublish-content-item.md), [`unpublish-web-page`](../commands/web-pages/unpublish-web-page.md), [`update-content-item`](../commands/content-items/update-content-item.md), [`update-slug`](../commands/web-pages/update-slug.md), [`update-tag`](../commands/taxonomies/update-tag.md), [`update-taxonomy`](../commands/taxonomies/update-taxonomy.md), [`update-web-page`](../commands/web-pages/update-web-page.md) |
| **Admin** | Everything in Write, plus: [`create-custom-object`](../commands/custom-objects/create-custom-object.md), [`delete-contacts`](../commands/forms-and-contacts/delete-contacts.md), [`delete-custom-object`](../commands/custom-objects/delete-custom-object.md), [`execute-sql`](../commands/sql/execute-sql.md), [`query-custom-objects`](../commands/custom-objects/query-custom-objects.md), [`query-sql`](../commands/sql/query-sql.md), [`update-custom-object`](../commands/custom-objects/update-custom-object.md) |
<!-- relay:end tiers -->

A command's tier is set on its class: `[RelayCommand("verb", Access = RelayAccess.Write)]`. A command with no `Access` defaults to **Admin**, so a new command is never open to lower tiers by accident. Each command page shows its tier in the table at the top.

## How a request is checked

1. The key is looked up. No key, or an unknown one, gets `401` with no body.
2. The key's tier is compared with the command's. A key that is too low gets `403` and a failed result that names both tiers, for example: `This API key has Read access, but 'delete-web-page' requires Write.`
3. Only then does the command run.

A [batch](../guides/batches.md) is checked as a whole before its first command runs, against the most demanding command in it. If one command is out of reach the whole batch is refused and nothing runs.

An unknown verb is not an access problem: it needs no tier and comes back as a failed result (`Unknown verb 'x'.`).

## Managing keys

Keys are managed in the Xperience administration, in the **Relay API keys** application under the **Configuration** category. There is nothing to put in `appsettings.json`.

- **Creating a key.** Choose **New API key**, give it a name (who or what will use it, such as "Intranet WebForms app") and pick a tier. The key is shown **once**, in a message at the top of the page. Copy it then.
- **The listing** shows each key's name, the first characters of the key (to tell keys apart), its tier, when it was created, and when it was last used.
- **Revoking a key.** Delete it. It stops working from the very next request.
- **Who can manage keys.** The application has View, Create and Delete permissions. Give them to the roles that should have them.

### What is stored

A key looks like `rly_` followed by 43 characters of random text (256 bits). Only a SHA-256 hash of it is stored, in the `Relay_ApiKey` table, so a key can't be recovered: if it is lost, delete it and create another. The table and the module that owns it are created when the site starts.

For speed the keys are held in memory and refreshed whenever Kentico reports a change to the table, so a request never has to query the database, and a new or deleted key takes effect immediately. "Last used" is written at most once every five minutes per key.

## Choosing a tier

- Give each caller its own key, so you can tell them apart and revoke one without affecting the others.
- Use the **lowest tier that works**. A dashboard or a report needs Read. A publishing job needs Write. Reserve Admin for the few callers that really need SQL or custom objects.
- Treat an Admin key as full trust: it can do anything the database login can. See [Guardrails](guardrails.md) for what the checks around Admin commands do and don't do.
- To rotate a key, create the new one, switch the caller over, then delete the old one.

See [Security](../security.md) for the rest of the advice on keeping keys safe.
