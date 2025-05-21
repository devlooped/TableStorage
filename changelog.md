# Changelog

## [v5.2.4](https://github.com/devlooped/TableStorage/tree/v5.2.4) (2025-05-21)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.3...v5.2.4)

:bug: Fixed bugs:

- Fix filtering by Timestamp [\#365](https://github.com/devlooped/TableStorage/pull/365) (@kzu)

:hammer: Other:

- Filtering by Timestamp isn't working [\#328](https://github.com/devlooped/TableStorage/issues/328)

:twisted_rightwards_arrows: Merged:

- Address minor build warnings [\#367](https://github.com/devlooped/TableStorage/pull/367) (@kzu)

## [v5.2.3](https://github.com/devlooped/TableStorage/tree/v5.2.3) (2025-05-17)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.2...v5.2.3)

:sparkles: Implemented enhancements:

- Make default DocumentSerializer public [\#363](https://github.com/devlooped/TableStorage/pull/363) (@kzu)

## [v5.2.2](https://github.com/devlooped/TableStorage/tree/v5.2.2) (2025-05-06)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.1...v5.2.2)

:sparkles: Implemented enhancements:

- Add .NET8 generated regex support for performance [\#355](https://github.com/devlooped/TableStorage/pull/355) (@kzu)

## [v5.2.1](https://github.com/devlooped/TableStorage/tree/v5.2.1) (2024-10-22)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.0...v5.2.1)

:twisted_rightwards_arrows: Merged:

- Make sure to include note on SponsorLink v2 [\#332](https://github.com/devlooped/TableStorage/pull/332) (@kzu)

## [v5.2.0](https://github.com/devlooped/TableStorage/tree/v5.2.0) (2024-07-24)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.0-rc.1...v5.2.0)

## [v5.2.0-rc.1](https://github.com/devlooped/TableStorage/tree/v5.2.0-rc.1) (2024-07-11)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.0-rc...v5.2.0-rc.1)

:sparkles: Implemented enhancements:

- Add support for batch saving multiple entities [\#309](https://github.com/devlooped/TableStorage/pull/309) (@kzu)

:twisted_rightwards_arrows: Merged:

- Improve test tables management [\#312](https://github.com/devlooped/TableStorage/pull/312) (@kzu)

## [v5.2.0-rc](https://github.com/devlooped/TableStorage/tree/v5.2.0-rc) (2024-07-10)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.2.0-beta...v5.2.0-rc)

:twisted_rightwards_arrows: Merged:

- Add SponsorLink v2 analyzer [\#302](https://github.com/devlooped/TableStorage/pull/302) (@kzu)

## [v5.2.0-beta](https://github.com/devlooped/TableStorage/tree/v5.2.0-beta) (2024-07-06)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.1.2...v5.2.0-beta)

:sparkles: Implemented enhancements:

- Improve key property detection for attributed types [\#292](https://github.com/devlooped/TableStorage/pull/292) (@kzu)

:bug: Fixed bugs:

- Fix TableEntity deserialization issue with newer Azurite [\#291](https://github.com/devlooped/TableStorage/pull/291) (@kzu)

:twisted_rightwards_arrows: Merged:

- Sync latest changes from oss template on build/publish workflows [\#300](https://github.com/devlooped/TableStorage/pull/300) (@kzu)
- Move ToEntity to the entity properties mapper [\#284](https://github.com/devlooped/TableStorage/pull/284) (@kzu)

## [v5.1.2](https://github.com/devlooped/TableStorage/tree/v5.1.2) (2024-01-25)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.1.1...v5.1.2)

:sparkles: Implemented enhancements:

- Update to latest protobuf with built-in support for DateOnly and TimeOnly [\#273](https://github.com/devlooped/TableStorage/pull/273) (@kzu)

## [v5.1.1](https://github.com/devlooped/TableStorage/tree/v5.1.1) (2023-10-04)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.1.0...v5.1.1)

:bug: Fixed bugs:

- KeyProperties are not being persisted properly [\#252](https://github.com/devlooped/TableStorage/issues/252)
- Properly persist computed colums for same type [\#253](https://github.com/devlooped/TableStorage/pull/253) (@kzu)

## [v5.1.0](https://github.com/devlooped/TableStorage/tree/v5.1.0) (2023-08-11)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.0.2...v5.1.0)

:twisted_rightwards_arrows: Merged:

- Remove current implementation of SponsorLink for now [\#244](https://github.com/devlooped/TableStorage/pull/244) (@kzu)

## [v5.0.2](https://github.com/devlooped/TableStorage/tree/v5.0.2) (2023-08-08)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.0.1...v5.0.2)

## [v5.0.1](https://github.com/devlooped/TableStorage/tree/v5.0.1) (2023-07-25)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v5.0.0...v5.0.1)

:sparkles: Implemented enhancements:

- Use OData type annotations to disambiguate property types when persisting [\#235](https://github.com/devlooped/TableStorage/issues/235)
- Empty query results does not throw [\#237](https://github.com/devlooped/TableStorage/pull/237) (@kzu)

## [v5.0.0](https://github.com/devlooped/TableStorage/tree/v5.0.0) (2023-07-25)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.3.0...v5.0.0)

:sparkles: Implemented enhancements:

- When hydrating queries for TableEntity, use native .NET types for properties [\#233](https://github.com/devlooped/TableStorage/issues/233)
- Add support for querying over TableEntityRepository [\#231](https://github.com/devlooped/TableStorage/issues/231)
- Improve persistence by annotating supported types with OData Edm [\#236](https://github.com/devlooped/TableStorage/pull/236) (@kzu)
- When hydrating queries for TableEntity, use native .NET types for properties [\#234](https://github.com/devlooped/TableStorage/pull/234) (@kzu)
- Add support for querying over TableEntityRepository [\#232](https://github.com/devlooped/TableStorage/pull/232) (@kzu)

## [v4.3.0](https://github.com/devlooped/TableStorage/tree/v4.3.0) (2023-06-27)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.2.1...v4.3.0)

:sparkles: Implemented enhancements:

- Allow entity lookup from entity value [\#215](https://github.com/devlooped/TableStorage/issues/215)
- Allow entity lookup from entity value [\#216](https://github.com/devlooped/TableStorage/pull/216) (@kzu)

## [v4.2.1](https://github.com/devlooped/TableStorage/tree/v4.2.1) (2023-04-17)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.2.0...v4.2.1)

:sparkles: Implemented enhancements:

- Allow persisting key properties as columns [\#198](https://github.com/devlooped/TableStorage/pull/198) (@kzu)

## [v4.2.0](https://github.com/devlooped/TableStorage/tree/v4.2.0) (2023-03-28)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.1.3...v4.2.0)

:sparkles: Implemented enhancements:

- Add SponsorLink to ensure ongoing development and support [\#193](https://github.com/devlooped/TableStorage/pull/193) (@kzu)

:twisted_rightwards_arrows: Merged:

- Switch to Polysharp for the NS2 polyfills [\#194](https://github.com/devlooped/TableStorage/pull/194) (@kzu)

## [v4.1.3](https://github.com/devlooped/TableStorage/tree/v4.1.3) (2023-01-20)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.1.2...v4.1.3)

:sparkles: Implemented enhancements:

- Add support for TableConnection overload for DocumentRepository [\#173](https://github.com/devlooped/TableStorage/pull/173) (@kzu)
- As documented \[PartitionKey\], honor it at class level [\#172](https://github.com/devlooped/TableStorage/pull/172) (@kzu)
- Allow persistence of entity properties as columns in document [\#171](https://github.com/devlooped/TableStorage/pull/171) (@kzu)
- Allow retrieving the table client from the connection [\#165](https://github.com/devlooped/TableStorage/pull/165) (@kzu)

:bug: Fixed bugs:

- Update mode on table partition should set table repository mode [\#174](https://github.com/devlooped/TableStorage/pull/174) (@kzu)

## [v4.1.2](https://github.com/devlooped/TableStorage/tree/v4.1.2) (2023-01-16)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.0.0...v4.1.2)

:sparkles: Implemented enhancements:

- Add TableConnection overloads for TableRepository factory methods [\#161](https://github.com/devlooped/TableStorage/issues/161)
- Allow reusing/caching the combination of CloudStorageAccount and TableClient [\#155](https://github.com/devlooped/TableStorage/issues/155)
- Provide comprehensive readmes for all packages [\#164](https://github.com/devlooped/TableStorage/pull/164) (@kzu)
- Add TableConnection overloads for TableRepository factory methods [\#162](https://github.com/devlooped/TableStorage/pull/162) (@kzu)
- Allow creating partitions from the same table connection [\#156](https://github.com/devlooped/TableStorage/pull/156) (@kzu)

:twisted_rightwards_arrows: Merged:

- ⛙ ⬆️ Bump dependencies [\#158](https://github.com/devlooped/TableStorage/pull/158) (@github-actions[bot])

## [v4.0.0](https://github.com/devlooped/TableStorage/tree/v4.0.0) (2022-08-26)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.0.0-rc.1...v4.0.0)

:sparkles: Implemented enhancements:

- Improve dynamic entity support by exposing TableEntity directly [\#127](https://github.com/devlooped/TableStorage/issues/127)

## [v4.0.0-rc.1](https://github.com/devlooped/TableStorage/tree/v4.0.0-rc.1) (2022-08-15)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.0.0-rc...v4.0.0-rc.1)

## [v4.0.0-rc](https://github.com/devlooped/TableStorage/tree/v4.0.0-rc) (2022-08-15)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.0.0-beta...v4.0.0-rc)

:sparkles: Implemented enhancements:

- Improve dynamic entity support by exposing TableEntity directly [\#128](https://github.com/devlooped/TableStorage/pull/128) (@kzu)

:twisted_rightwards_arrows: Merged:

- +M▼ includes [\#123](https://github.com/devlooped/TableStorage/pull/123) (@github-actions[bot])
- +M▼ includes [\#117](https://github.com/devlooped/TableStorage/pull/117) (@github-actions[bot])

## [v4.0.0-beta](https://github.com/devlooped/TableStorage/tree/v4.0.0-beta) (2022-05-17)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v4.0.0-alpha...v4.0.0-beta)

:bug: Fixed bugs:

- If partition or row key expressions have complex lambda, property name should be null [\#100](https://github.com/devlooped/TableStorage/issues/100)

:twisted_rightwards_arrows: Merged:

- Reduce scope of key property lookup to direct property lambda [\#101](https://github.com/devlooped/TableStorage/pull/101) (@kzu)

## [v4.0.0-alpha](https://github.com/devlooped/TableStorage/tree/v4.0.0-alpha) (2022-05-04)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.2.0...v4.0.0-alpha)

:sparkles: Implemented enhancements:

- Upgrade to latest Azure v12 SDK [\#89](https://github.com/devlooped/TableStorage/pull/89) (@kzu)

:hammer: Other:

- Upgrade to latest Azure v12 SDK [\#88](https://github.com/devlooped/TableStorage/issues/88)
- Add support for DateOnly [\#78](https://github.com/devlooped/TableStorage/issues/78)

:twisted_rightwards_arrows: Merged:

- Fix test filter for theory tests [\#91](https://github.com/devlooped/TableStorage/pull/91) (@kzu)
- Address warnings [\#90](https://github.com/devlooped/TableStorage/pull/90) (@kzu)

## [v3.2.0](https://github.com/devlooped/TableStorage/tree/v3.2.0) (2021-12-13)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.1.1...v3.2.0)

:sparkles: Implemented enhancements:

- Add support for DateOnly [\#79](https://github.com/devlooped/TableStorage/pull/79) (@kzu)

## [v3.1.1](https://github.com/devlooped/TableStorage/tree/v3.1.1) (2021-08-29)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.1.0...v3.1.1)

:sparkles: Implemented enhancements:

- Add API docs for all packages [\#64](https://github.com/devlooped/TableStorage/issues/64)

:twisted_rightwards_arrows: Merged:

- Add missing API docs and fix all docs warnings [\#65](https://github.com/devlooped/TableStorage/pull/65) (@kzu)

## [v3.1.0](https://github.com/devlooped/TableStorage/tree/v3.1.0) (2021-08-13)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.0.3...v3.1.0)

:sparkles: Implemented enhancements:

- Support Timestamp property in POCO entities [\#60](https://github.com/devlooped/TableStorage/issues/60)

:twisted_rightwards_arrows: Merged:

- Support Timestamp property in POCO entities [\#61](https://github.com/devlooped/TableStorage/pull/61) (@kzu)

## [v3.0.3](https://github.com/devlooped/TableStorage/tree/v3.0.3) (2021-07-28)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.0.2...v3.0.3)

:bug: Fixed bugs:

- Azure Functions fails with SocketException sometimes when accessing table [\#58](https://github.com/devlooped/TableStorage/issues/58)

## [v3.0.2](https://github.com/devlooped/TableStorage/tree/v3.0.2) (2021-07-01)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.0.1...v3.0.2)

:bug: Fixed bugs:

- ContinuationToken not properly used for enumerating all entities [\#53](https://github.com/devlooped/TableStorage/issues/53)

## [v3.0.1](https://github.com/devlooped/TableStorage/tree/v3.0.1) (2021-07-01)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v3.0.0...v3.0.1)

:bug: Fixed bugs:

- Remove Create factory methods that cause ambiguous matches [\#52](https://github.com/devlooped/TableStorage/issues/52)

## [v3.0.0](https://github.com/devlooped/TableStorage/tree/v3.0.0) (2021-07-01)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v2.0.2...v3.0.0)

:sparkles: Implemented enhancements:

- Don't fail if deleting non-existent entity, return false instead [\#50](https://github.com/devlooped/TableStorage/issues/50)
- Switch from TableEntity to ITableEntity in the non-generic repository for flexibility [\#48](https://github.com/devlooped/TableStorage/issues/48)
- For PutAsync, allow selecting Replace vs Merge behavior [\#46](https://github.com/devlooped/TableStorage/issues/46)
- When deleting entities, return boolean status for success/failure [\#51](https://github.com/devlooped/TableStorage/pull/51) (@kzu)
- Allow additional properties in table entity repository [\#49](https://github.com/devlooped/TableStorage/pull/49) (@kzu)
- Support merge strategy when updating entities [\#47](https://github.com/devlooped/TableStorage/pull/47) (@kzu)

:hammer: Other:

- Document usage of \[Browsable\(false\)\] to skip persistence [\#45](https://github.com/devlooped/TableStorage/issues/45)

## [v2.0.2](https://github.com/devlooped/TableStorage/tree/v2.0.2) (2021-06-23)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v2.0.1...v2.0.2)

:sparkles: Implemented enhancements:

- Allow annotating record constructor parameters with PartitionKey/RowKey [\#43](https://github.com/devlooped/TableStorage/issues/43)

:twisted_rightwards_arrows: Merged:

- Add support for record constructor parameter annotations [\#44](https://github.com/devlooped/TableStorage/pull/44) (@kzu)

## [v2.0.1](https://github.com/devlooped/TableStorage/tree/v2.0.1) (2021-06-17)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v2.0.1-rc...v2.0.1)

## [v2.0.1-rc](https://github.com/devlooped/TableStorage/tree/v2.0.1-rc) (2021-06-17)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v2.0.1-beta...v2.0.1-rc)

## [v2.0.1-beta](https://github.com/devlooped/TableStorage/tree/v2.0.1-beta) (2021-06-17)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v2.0.0...v2.0.1-beta)

:sparkles: Implemented enhancements:

- Include readme in package [\#39](https://github.com/devlooped/TableStorage/issues/39)

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

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.1...v1.0.2)

## [v1.0.1](https://github.com/devlooped/TableStorage/tree/v1.0.1) (2021-05-07)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.0...v1.0.1)

## [v1.0.0](https://github.com/devlooped/TableStorage/tree/v1.0.0) (2021-05-07)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.0-rc...v1.0.0)

## [v1.0.0-rc](https://github.com/devlooped/TableStorage/tree/v1.0.0-rc) (2021-05-07)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.0-beta...v1.0.0-rc)

:twisted_rightwards_arrows: Merged:

- Add SourceLink to get repo/pdb linking [\#3](https://github.com/devlooped/TableStorage/pull/3) (@kzu)

## [v1.0.0-beta](https://github.com/devlooped/TableStorage/tree/v1.0.0-beta) (2021-05-05)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/v1.0.0-alpha...v1.0.0-beta)

## [v1.0.0-alpha](https://github.com/devlooped/TableStorage/tree/v1.0.0-alpha) (2021-05-05)

[Full Changelog](https://github.com/devlooped/TableStorage/compare/cf1b7f069ac6d68482b498555c8dbdda8e1ae5b4...v1.0.0-alpha)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
