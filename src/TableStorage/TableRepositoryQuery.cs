using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.OData.Client;

namespace Devlooped
{
    class TableRepositoryQuery<T> : IQueryable<T>, IQueryProvider, IAsyncEnumerable<T>
    {
        readonly CloudStorageAccount account;
        readonly IStringDocumentSerializer serializer;
        readonly string tableName;
        readonly string? partitionKeyProperty;
        readonly string? rowKeyProperty;
        readonly Expression expression;

        public TableRepositoryQuery(CloudStorageAccount account, IStringDocumentSerializer serializer, string tableName,
            string? partitionKeyProperty, string? rowKeyProperty, Expression? expression = default)
        {
            this.account = account;
            this.serializer = serializer;
            this.tableName = tableName;
            this.partitionKeyProperty = partitionKeyProperty;
            this.rowKeyProperty = rowKeyProperty;
            this.expression = expression ?? new DataServiceContext(account.TableStorageUri.PrimaryUri).CreateQuery<T>(tableName).Expression;
        }

        /// <summary>
        /// Allows scoping the query to a specific partition key. Used by <see cref="TablePartition{T}.CreateQuery"/>.
        /// </summary>
        internal string? PartitionKey { get; set; }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellation = default)
        {
            var query = (DataServiceQuery)new DataServiceContext(account.TableStorageUri.PrimaryUri).CreateQuery<T>(tableName)
                .Provider.CreateQuery(expression);

            // OData will translate the enum value in a filter to TYPENAME.'ENUMVALUE'.
            // The type name can contain the + sign if it's a nested type, too. So we
            // need to remove the type name plus the dot and just leave the string 
            // value as part of the filter string.
            var rawqs = Regex.Replace(
                query.RequestUri.GetComponents(UriComponents.Query, UriFormat.Unescaped),
                "(\\W)[\\w\\+\\.]+('\\w+')", "$1$2");

            // We need to count & break manually because $top is interpreted as the max records per page 
            // if the set matches more items. This is clearly unintuitive and *not* what one typically 
            // wants when using LINQ queries! See Note on https://docs.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities#supported-query-options
            var qs = HttpUtility.ParseQueryString(rawqs);
            if (!long.TryParse(qs["$top"], out var top))
                top = -1;

            long count = 0;
            // Covers weird case of top == 0.
            if (count == top)
                yield break;

            if (qs["$select"] != null)
            {
                // Collect the properties being projected, and append the built-in ones to them.
                var projection = new ProjectionVisitor();
                projection.Visit(expression);

                if (projection.Properties.Count > 0)
                {
                    qs["$select"] = string.Join(",", projection.Properties
                        // NOTE: skip key properties since they are renamed back before deserialization below
                        .Where(prop => prop != partitionKeyProperty && prop != rowKeyProperty)
                        // NOTE: always project the built-in props.
                        .Concat(new[]
                        {
                            nameof(ITableEntity.PartitionKey),
                            nameof(ITableEntity.RowKey),
                            nameof(ITableEntity.ETag),
                            nameof(ITableEntity.Timestamp)
                        }));
                }
            }

            var filter = qs["$filter"];
            if (filter != null && (partitionKeyProperty != null || rowKeyProperty != null))
            {
                if (partitionKeyProperty != null)
                    filter = Regex.Replace(filter, partitionKeyProperty + "(\\W)", "PartitionKey$1");
                if (rowKeyProperty != null)
                    filter = Regex.Replace(filter, rowKeyProperty + "(\\W)", "RowKey$1");

                qs["$filter"] = filter;
            }

            if (PartitionKey != null)
            {
                if (filter == null)
                    filter = "PartitionKey eq '" + PartitionKey + "'";
                else
                    filter = "PartitionKey eq '" + PartitionKey + "' and " + filter;

                qs["$filter"] = filter;
            }

            var builder = new UriBuilder(query.RequestUri)
            {
                Query = string.Join("&", qs.AllKeys.Select(x => $"{x}={qs[x]}"))
            };

            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri)
                .AddAuthorizationHeader(account);

            var response = await Http.Client.SendAsync(request);
            while (true)
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                foreach (var element in doc.RootElement.GetProperty("value").EnumerateArray())
                {
                    var mem = new MemoryStream();
                    var writer = new Utf8JsonWriter(mem);

                    writer.WriteStartObject();

                    // Write the renamed key properties, if any.
                    if (partitionKeyProperty != null)
                        writer.WriteString(partitionKeyProperty, element.GetProperty("PartitionKey").GetString());
                    if (rowKeyProperty != null)
                        writer.WriteString(rowKeyProperty, element.GetProperty("RowKey").GetString());

                    foreach (var property in element.EnumerateObject())
                        property.WriteTo(writer);

                    writer.WriteEndObject();
                    writer.Flush();

                    var data = Encoding.UTF8.GetString(mem.ToArray());
                    var item = serializer.Deserialize<T>(data);
                    if (item != null)
                    {
                        yield return item;
                        count++;
                        if (count == top)
                            yield break;
                    }
                }

                if (!response.Headers.TryGetValues("x-ms-continuation-NextPartitionKey", out var nextPartitionKeyValues) ||
                    nextPartitionKeyValues.FirstOrDefault() is not string nextPartitionKey ||
                    string.IsNullOrEmpty(nextPartitionKey) ||
                    !response.Headers.TryGetValues("x-ms-continuation-NextRowKey", out var nextRowKeyValues) ||
                    nextRowKeyValues.FirstOrDefault() is not string nextRowKey ||
                    string.IsNullOrEmpty(nextRowKey))
                    break;

                qs["NextPartitionKey"] = nextPartitionKey;
                qs["NextRowKey"] = nextRowKey;
                builder.Query = string.Join("&", qs.AllKeys.Select(x => $"{x}={qs[x]}"));

                request = new HttpRequestMessage(HttpMethod.Get, builder.Uri)
                    .AddAuthorizationHeader(account);

                response = await Http.Client.SendAsync(request);
            }
        }

        public IQueryable CreateQuery(Expression expression) => CreateQuery<T>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TableRepositoryQuery<TElement>(account, serializer, tableName, partitionKeyProperty, rowKeyProperty, expression)
            {
                PartitionKey = PartitionKey
            };

        public object Execute(Expression expression) => throw new NotSupportedException("Please use a asynchronous enumeration (i.e. 'async foreach') to execute the query.");

        public TResult Execute<TResult>(Expression expression) => throw new NotSupportedException("Please use a asynchronous enumeration (i.e. 'async foreach') to execute the query.");

        public IEnumerator<T> GetEnumerator() => throw new NotSupportedException("Please use an asynchronous enumeration (i.e. 'async foreach') instead.");

        public Type ElementType => typeof(T);

        public Expression Expression => expression;

        public IQueryProvider Provider => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        class ProjectionVisitor : ExpressionVisitor
        {
            readonly PropertyExpressionVisitor visitor = new PropertyExpressionVisitor();

            public HashSet<string> Properties => visitor.Properties;

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == nameof(Queryable.Select) &&
                    node.Method.DeclaringType == typeof(Queryable))
                {
                    visitor.Visit(node.Arguments[1]);
                }
                return node;
            }

            class PropertyExpressionVisitor : ExpressionVisitor
            {
                public HashSet<string> Properties { get; } = new HashSet<string>();

                protected override Expression VisitMember(MemberExpression node)
                {
                    if (node.Member is PropertyInfo)
                        Properties.Add(node.Member.Name);

                    return base.VisitMember(node);
                }
            }
        }
    }
}
