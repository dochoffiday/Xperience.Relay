# get-taxonomies

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-taxonomies` | [Read](../../concepts/access-tiers.md) | [`GetTaxonomiesResult`](../../reference/types.md#gettaxonomiesresult) | `GetTaxonomiesCommand` |
<!-- relay:end header -->

Lists every taxonomy with its ID, GUID, code name, title and description.

<!-- relay:begin parameters -->
## Parameters

This command takes no parameters. Send an empty `parameters` object, or leave `parameters` out.
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [GetTaxonomiesResult](../../reference/types.md#gettaxonomiesresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `taxonomies` | [TaxonomyResult](../../reference/types.md#taxonomyresult)[] | All taxonomies. |
<!-- relay:end result -->

## Behavior

- **No parameters.** Send `{}` as the parameters, or leave `parameters` out.
- **All of them.** There is no paging or filtering; a site has few taxonomies.
- **Next step.** [`get-tags`](get-tags.md) lists the tags of a taxonomy, by its ID or its code name.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### List the taxonomies

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-taxonomies",
      "parameters": {}
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetTaxonomiesCommand());
    var data = result.GetData<GetTaxonomiesResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 6 taxonomy/taxonomies.",
  "error": null,
  "data": {
    "taxonomies": [
      {
        "taxonomyId": 1,
        "taxonomyGuid": "0eb82e7d-4af6-496a-bfc2-c0b3419956b7",
        "taxonomyName": "CafePromotion",
        "taxonomyTitle": "Cafe Promotions & Offers",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 2,
        "taxonomyGuid": "3029353a-41f7-404b-a8be-3dd7e3f437e0",
        "taxonomyName": "CoffeeProcessing",
        "taxonomyTitle": "Coffee processing",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 3,
        "taxonomyGuid": "408d5f84-95c6-4ff4-8479-5e2930dbf755",
        "taxonomyName": "CoffeeTastes",
        "taxonomyTitle": "Coffee tastes",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 4,
        "taxonomyGuid": "b54af6a7-0bff-40e0-a29b-e7580ce78c6a",
        "taxonomyName": "ProductCategories",
        "taxonomyTitle": "Product categories",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 5,
        "taxonomyGuid": "d1e158ea-b068-436c-8d6f-eb0c9bfdcec2",
        "taxonomyName": "ProductManufacturers",
        "taxonomyTitle": "Product manufacturers",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 6,
        "taxonomyGuid": "1477faae-3c0b-4d40-baf0-e0d1315b8238",
        "taxonomyName": "ProductTags",
        "taxonomyTitle": "Product tags",
        "taxonomyDescription": ""
      }
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`get-tags`](get-tags.md)
- [`create-taxonomy`](create-taxonomy.md)
