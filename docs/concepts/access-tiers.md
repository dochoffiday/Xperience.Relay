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
| **Read** | [`get-content`](../commands/content-items/index.md#get-content), [`get-content-info`](../commands/content-items/index.md#get-content-info), [`get-content-item-usage`](../commands/content-items/index.md#get-content-item-usage), [`get-form-submissions`](../commands/forms-and-contacts/index.md#get-form-submissions), [`get-page`](../commands/web-pages/index.md#get-page), [`get-page-info`](../commands/web-pages/index.md#get-page-info), [`get-tags`](../commands/taxonomies/index.md#get-tags), [`get-taxonomies`](../commands/taxonomies/index.md#get-taxonomies), [`query-reusable-items`](../commands/content-items/index.md#query-reusable-items), [`query-web-page-items`](../commands/web-pages/index.md#query-web-page-items), [`search-content`](../commands/content-items/index.md#search-content) |
| **Write** | Everything in Read, plus: [`create-content-item`](../commands/content-items/index.md#create-content-item), [`create-tag`](../commands/taxonomies/index.md#create-tag), [`create-taxonomy`](../commands/taxonomies/index.md#create-taxonomy), [`create-web-page`](../commands/web-pages/index.md#create-web-page), [`delete-content-item`](../commands/content-items/index.md#delete-content-item), [`delete-tag`](../commands/taxonomies/index.md#delete-tag), [`delete-taxonomy`](../commands/taxonomies/index.md#delete-taxonomy), [`delete-web-page`](../commands/web-pages/index.md#delete-web-page), [`get-content-hub-folder`](../commands/content-items/index.md#get-content-hub-folder), [`move-content-item`](../commands/content-items/index.md#move-content-item), [`move-tag`](../commands/taxonomies/index.md#move-tag), [`move-web-page`](../commands/web-pages/index.md#move-web-page), [`publish-content-item`](../commands/content-items/index.md#publish-content-item), [`publish-web-page`](../commands/web-pages/index.md#publish-web-page), [`rename-asset`](../commands/assets/index.md#rename-asset), [`reoptimize-asset`](../commands/assets/index.md#reoptimize-asset), [`unpublish-content-item`](../commands/content-items/index.md#unpublish-content-item), [`unpublish-web-page`](../commands/web-pages/index.md#unpublish-web-page), [`update-content-item`](../commands/content-items/index.md#update-content-item), [`update-slug`](../commands/web-pages/index.md#update-slug), [`update-tag`](../commands/taxonomies/index.md#update-tag), [`update-taxonomy`](../commands/taxonomies/index.md#update-taxonomy), [`update-web-page`](../commands/web-pages/index.md#update-web-page) |
| **Admin** | Everything in Write, plus: [`create-custom-object`](../commands/custom-objects/index.md#create-custom-object), [`delete-contacts`](../commands/forms-and-contacts/index.md#delete-contacts), [`delete-custom-object`](../commands/custom-objects/index.md#delete-custom-object), [`execute-sql`](../commands/sql/index.md#execute-sql), [`query-custom-objects`](../commands/custom-objects/index.md#query-custom-objects), [`query-sql`](../commands/sql/index.md#query-sql), [`update-custom-object`](../commands/custom-objects/index.md#update-custom-object) |
<!-- relay:end tiers -->

A command's tier is set on its class: `[RelayCommand("verb", Access = RelayAccess.Write)]`. A command with no `Access` defaults to **Admin**, so a new command is never open to lower tiers by accident. Each command's section shows its tier in the table at its top.

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
