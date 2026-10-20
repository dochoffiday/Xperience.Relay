# Taxonomies and tags

A taxonomy is a named group of tags, and tags can be nested. Content refers to tags by GUID, in tag fields: [`create-tag`](create-tag.md) and [`get-tags`](get-tags.md) return the GUIDs that [`create-content-item`](../content-items/create-content-item.md) and the update commands take.

Both kinds of commands identify things by ID or by code name; the code name is made from the title when you don't give one.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-taxonomy`](create-taxonomy.md) | Write | Creates a taxonomy. |
| [`update-taxonomy`](update-taxonomy.md) | Write | Changes a taxonomy's title and/or description. |
| [`delete-taxonomy`](delete-taxonomy.md) | Write | Deletes a taxonomy. |
| [`get-taxonomies`](get-taxonomies.md) | Read | Lists all taxonomies. |
| [`create-tag`](create-tag.md) | Write | Creates a tag in a taxonomy, at the root or under another tag. |
| [`update-tag`](update-tag.md) | Write | Changes a tag's title and/or description. |
| [`move-tag`](move-tag.md) | Write | Moves a tag under another tag, or to the root of its taxonomy, at a given position. |
| [`delete-tag`](delete-tag.md) | Write | Deletes a tag. |
| [`get-tags`](get-tags.md) | Read | Returns tags matching the given filter. |
<!-- relay:end commands -->
