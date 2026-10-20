namespace Xperience.Relay.Docs;

/// <summary>A group of commands that share a section of the docs: one folder, one landing page.</summary>
sealed record Area(string Folder, string Title, string[] Verbs);

static class Areas
{
    // The order of the verbs here is the order of the section's navigation and of its tables.
    public static readonly Area[] All =
    [
        new("web-pages", "Web pages",
        [
            "create-web-page", "update-web-page", "update-slug", "move-web-page", "publish-web-page", "unpublish-web-page",
            "delete-web-page", "get-page-info", "get-page", "query-web-page-items",
        ]),
        new("content-items", "Content items",
        [
            "create-content-item", "update-content-item", "move-content-item", "publish-content-item", "unpublish-content-item",
            "delete-content-item", "get-content-info", "get-content", "get-content-item-usage", "get-content-hub-folder",
            "query-reusable-items", "search-content",
        ]),
        new("assets", "Assets", ["rename-asset", "reoptimize-asset"]),
        new("taxonomies", "Taxonomies and tags",
        [
            "create-taxonomy", "update-taxonomy", "delete-taxonomy", "get-taxonomies",
            "create-tag", "update-tag", "move-tag", "delete-tag", "get-tags",
        ]),
        new("custom-objects", "Custom objects",
            ["create-custom-object", "update-custom-object", "delete-custom-object", "query-custom-objects"]),
        new("forms-and-contacts", "Forms and contacts", ["get-form-submissions", "delete-contacts"]),
        new("sql", "SQL", ["query-sql", "execute-sql"]),
    ];

    public static Area? ForVerb(string verb) => All.FirstOrDefault(a => a.Verbs.Contains(verb));
}
