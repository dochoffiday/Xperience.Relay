# get-content-hub-folder

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content-hub-folder` | [Write](../../concepts/access-tiers.md) | [`GetContentHubFolderResult`](../../reference/types.md#getcontenthubfolderresult) | `GetContentHubFolderCommand` |
<!-- relay:end header -->

Finds a content hub folder by ID, code name or path, and creates the folders of a path that don't exist yet. The result is the folder ID that [`create-content-item`](create-content-item.md) and [`move-content-item`](move-content-item.md) take.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentFolderId` | integer | No | Look up by numeric ID. Mutually exclusive with `codeName` and `folderPath`. |
| `codeName` | string | No | Look up by code name. Mutually exclusive with `contentFolderId` and `folderPath`. |
| `folderPath` | string | No | Slash-separated path, e.g. "Imports/Audio". All missing segments are created. Mutually exclusive with `contentFolderId` and `codeName`. |
| `workspaceName` | string | No | Required when using `folderPath`. Defaults to [`RelayKenticoOptions.DefaultWorkspaceName`](../../configuration.md#defaultworkspacename) when null. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [GetContentHubFolderResult](../../reference/types.md#getcontenthubfolderresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentFolderId` | integer | ID of the folder that was found or created, for [`create-content-item`](create-content-item.md) and [`move-content-item`](move-content-item.md). |
<!-- relay:end result -->

## Behavior

- **Exactly one way to find the folder.** Give `contentFolderId`, `codeName` or `folderPath`. Giving none, or more than one, fails.
- **Paths create folders.** `folderPath` is a slash-separated path of display names, such as `Events/2026`. Every segment that doesn't exist yet is created, so the command is safe to run again: the second run finds the folders and changes nothing. A segment is matched by display name inside its parent. Code names are global, so Relay gives each new folder one built from its whole path, with `/` replaced by `_` (`Events/2026` becomes `Events_2026`), and shortens it with a hash when it would be longer than 49 characters.
- **Workspace.** A path starts at the root of a workspace, so `workspaceName` (a workspace code name) is needed with `folderPath`; it defaults to [`DefaultWorkspaceName`](../../configuration.md#defaultworkspacename). The other two ways don't use it.
- **A Write command.** A path can create folders, so the command needs a Write key even when the folders already exist.
- **Messages.** "Folder 'Events/2026' is ready (ID=5)." for a path, "Found folder '2026' (ID=5)." for the other two ways.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Find a folder by path, creating the folders that are missing

Neither folder existed, so both were created. Running the same command again finds them and changes nothing.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-hub-folder",
      "parameters": {
        "folderPath": "Events/2026",
        "workspaceName": "DancingGoat.DancingGoatEvents"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentHubFolderCommand
    {
        FolderPath = "Events/2026",
        WorkspaceName = "DancingGoat.DancingGoatEvents",
    });
    var data = result.GetData<GetContentHubFolderResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Folder 'Events/2026' is ready (ID=5).",
  "error": null,
  "data": {
    "contentFolderId": 5
  }
}
```

### Find a folder by its ID

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-hub-folder",
      "parameters": {
        "contentFolderId": 5
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentHubFolderCommand
    {
        ContentFolderId = 5,
    });
    var data = result.GetData<GetContentHubFolderResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found folder '2026' (ID=5).",
  "error": null,
  "data": {
    "contentFolderId": 5
  }
}
```

### Find a folder by its code name

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-hub-folder",
      "parameters": {
        "codeName": "Events_2026"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentHubFolderCommand
    {
        CodeName = "Events_2026",
    });
    var data = result.GetData<GetContentHubFolderResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found folder '2026' (ID=5).",
  "error": null,
  "data": {
    "contentFolderId": 5
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No way of identifying the folder"

    Parameters:

    ```json
    {}
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "One of ContentFolderId, CodeName, or FolderPath must be provided.",
      "data": null
    }
    ```

???+ failure "More than one way of identifying the folder"

    Parameters:

    ```json
    {
      "contentFolderId": 5,
      "folderPath": "Events/2026"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Only one of ContentFolderId, CodeName, or FolderPath may be provided.",
      "data": null
    }
    ```

???+ failure "A path without a workspace"

    Parameters:

    ```json
    {
      "folderPath": "Events/2026"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WorkspaceName is required when using FolderPath. Set it on the command or configure RelayKenticoOptions.DefaultWorkspaceName.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`create-content-item`](create-content-item.md)
- [`move-content-item`](move-content-item.md)
