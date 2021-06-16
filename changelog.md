

## [v2.0.0](https://github.com/devlooped/TableStorage/tree/v2.0.0) (2021-06-16)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.3.0...v2.0.0)

:sparkles: Implemented enhancements:

- Add support for enhanced filtering for document-based repositories [\#37](https://github.com/devlooped/TableStorage/issues/37)
- Add support for filtering by enum property types [\#35](https://github.com/devlooped/TableStorage/issues/35)
- Add support for querying [\#33](https://github.com/devlooped/TableStorage/issues/33)
- Allow passing serialization options for MessagePack serializer [\#32](https://github.com/devlooped/TableStorage/issues/32)
- Switch default built-in serializer to System.Text.Json  [\#30](https://github.com/devlooped/TableStorage/issues/30)
- Make Document/Entity default table names plural [\#28](https://github.com/devlooped/TableStorage/issues/28)
- Don't duplicate PartitionKey/RowKey properties in storage [\#26](https://github.com/devlooped/TableStorage/issues/26)

:twisted_rightwards_arrows: Merged:

- Add support for enhanced filtering for document-based repo [\#38](https://github.com/devlooped/TableStorage/pull/38) (@kzu)
- Add support for filtering by enum property types [\#36](https://github.com/devlooped/TableStorage/pull/36) (@kzu)
- Add support for querying with LINQ and expressions [\#34](https://github.com/devlooped/TableStorage/pull/34) (@kzu)
- Switch default built-in serializer to System.Text.Json [\#31](https://github.com/devlooped/TableStorage/pull/31) (@kzu)
- Make Document/Entity default table names plural [\#29](https://github.com/devlooped/TableStorage/pull/29) (@kzu)
- Don't duplicate PartitionKey/RowKey properties in storage [\#27](https://github.com/devlooped/TableStorage/pull/27) (@kzu)

## [v1.3.0](https://github.com/devlooped/TableStorage/tree/v1.3.0) (2021-05-31)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.2.1...v1.3.0)

:sparkles: Implemented enhancements:

- Allow persisting entities as documents instead of individual columns [\#24](https://github.com/devlooped/TableStorage/issues/24)
- Avoid creating a separate Task for lazily initialization of CloudTable [\#23](https://github.com/devlooped/TableStorage/issues/23)
- No need to use DynamicTableEntity when deleting [\#21](https://github.com/devlooped/TableStorage/issues/21)
- When doing a PutAsync, use InsertOrMerge instead of InsertOrReplace [\#20](https://github.com/devlooped/TableStorage/issues/20)
- Allow persisting entities as documents [\#25](https://github.com/devlooped/TableStorage/pull/25) (@kzu)

:bug: Fixed bugs:

- Inconsistent default partition name in TablePartition.Create [\#22](https://github.com/devlooped/TableStorage/issues/22)

## [v1.2.1](https://github.com/devlooped/TableStorage/tree/v1.2.1) (2021-05-29)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.2.0...v1.2.1)

:sparkles: Implemented enhancements:

- Add support for TableEntity via ITableRepository and ITablePartition APIs [\#18](https://github.com/devlooped/TableStorage/issues/18)

## [v1.2.0](https://github.com/devlooped/TableStorage/tree/v1.2.0) (2021-05-26)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.1.1...v1.2.0)

:sparkles: Implemented enhancements:

- Add an AttributedTableRepository\<T\> for easy consumption in DI scenarios [\#16](https://github.com/devlooped/TableStorage/issues/16)

## [v1.1.1](https://github.com/devlooped/TableStorage/tree/v1.1.1) (2021-05-26)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.1.0...v1.1.1)

:bug: Fixed bugs:

- Fix usage in package description [\#15](https://github.com/devlooped/TableStorage/issues/15)

## [v1.1.0](https://github.com/devlooped/TableStorage/tree/v1.1.0) (2021-05-26)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.4...v1.1.0)

:sparkles: Implemented enhancements:

- Force factory method usage for default implementations [\#14](https://github.com/devlooped/TableStorage/issues/14)
- Apply factory method defaults to TableRepository/TablePartition constructors [\#13](https://github.com/devlooped/TableStorage/issues/13)

## [v1.0.4](https://github.com/devlooped/TableStorage/tree/v1.0.4) (2021-05-16)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.3...v1.0.4)

:sparkles: Implemented enhancements:

- Make TableRepository\<T\>/TablePartition\<T\> public [\#10](https://github.com/devlooped/TableStorage/issues/10)

## [v1.0.3](https://github.com/devlooped/TableStorage/tree/v1.0.3) (2021-05-15)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.2...v1.0.3)

## [v1.0.2](https://github.com/devlooped/TableStorage/tree/v1.0.2) (2021-05-10)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/cf1b7f069ac6d68482b498555c8dbdda8e1ae5b4...v1.0.2)

:twisted_rightwards_arrows: Merged:

- Add SourceLink to get repo/pdb linking [\#3](https://github.com/devlooped/TableStorage/pull/3) (@kzu)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
