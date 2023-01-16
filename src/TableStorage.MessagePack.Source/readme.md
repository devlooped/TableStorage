A source-only MessagePack binary serializer for use with document-based repositories.

Usage:

```csharp
  var repo = DocumentRepository.Create<Product>(..., serializer: MessagePackDocumentSerializer.Default);
```

> NOTE: MessagePack attributes must be used as usual in order for the serialization to work.

<!-- include ../../readme.md#documents -->
<!-- include ../../readme.md#sponsors -->

<!-- Exclude from auto-expansion by devlooped/actions-include GH action -->
<!-- exclude -->