[![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](osmfeula.txt)
[![OSS](https://img.shields.io/github/license/devlooped/oss.svg?color=blue)](license.txt) 
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/TableStorage)

A Protocol Buffers binary serializer for use with document-based repositories.

Usage:

```csharp
  var repo = DocumentRepository.Create<Product>(..., serializer: ProtobufDocumentSerializer.Default);
```

<!-- include ../../readme.md#documents -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
<!-- exclude -->