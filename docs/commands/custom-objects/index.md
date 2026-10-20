# Custom objects

Generic access to any object type registered in Kentico, by its object type name: custom module classes, and built-in types too. These commands need an Admin key, because they can reach anything the type's table holds.

The examples use `cms.country`, a built-in type that exists on every site. Use your own object type name, such as `custom.redirect`.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-custom-object`](create-custom-object.md) | Admin | Creates an object of any registered Kentico object type (typically a custom module class) by setting the given columns and inserting it, so Kentico's events and cache invalidation run as usual. |
| [`update-custom-object`](update-custom-object.md) | Admin | Updates an object of any registered Kentico object type by ID, changing only the given columns, through Kentico so its events and cache invalidation run as usual. |
| [`delete-custom-object`](delete-custom-object.md) | Admin | Deletes an object of any registered Kentico object type by ID, through Kentico so its events and cache invalidation run as usual. |
| [`query-custom-objects`](query-custom-objects.md) | Admin | Reads objects of any registered Kentico object type (typically a custom module class). |
<!-- relay:end commands -->
