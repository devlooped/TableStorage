A source-only Json.NET serializer for use with document-based repositories.

Usage:

```csharp
  var repo = DocumentRepository.Create<Product>(..., serializer: JsonDocumentSerializer.Default);
```

<!-- include ../../readme.md#documents -->
<!-- include ../../readme.md#sponsors -->

<!-- Exclude from auto-expansion by devlooped/actions-include GH action -->
<!-- exclude -->