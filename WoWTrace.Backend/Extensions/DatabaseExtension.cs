using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.Mapping;
using LinqToDB.SqlProvider;
using System.Collections.Generic;
using System.Linq;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Extensions
{
    public static class DatabaseExtension
    {
        public static void MultiInsertIgnore<T>(this WowtraceDB db, ITable<T> table, IEnumerable<T> source, BulkCopyOptions options = null)
        {
            if (options == null)
                options = new BulkCopyOptions();

            var helper = BuildBulkCopySql(table, source, options, "INSERT IGNORE");

            helper.Execute();
        }

        public static void MultiInsertOnDuplicateUpdate<T>(this WowtraceDB db, ITable<T> table, IEnumerable<T> source, IEnumerable<string> updateColumns = null, BulkCopyOptions options = null)
        {
            if (options == null)
                options = new BulkCopyOptions();

            var helper = BuildBulkCopySql(table, source, options, "INSERT INTO");

            if (updateColumns == null)
                helper.Columns.Select(c => c.ColumnName).ToList();

            helper.StringBuilder
                    .AppendLine()
                    .Append("ON DUPLICATE KEY UPDATE");

            foreach (var updateColumn in updateColumns)
            {
                ColumnDescriptor column = helper.Columns.First(c => c.ColumnName == updateColumn || c.MemberName == updateColumn);

                helper.StringBuilder
                    .AppendLine()
                    .Append('\t');
                helper.SqlBuilder.Convert(helper.StringBuilder, column.ColumnName, ConvertType.NameToQueryField);
                helper.StringBuilder.AppendFormat(" = VALUES({0})", helper.SqlBuilder.ConvertInline(column.ColumnName, ConvertType.NameToQueryField));
            }

            helper.Execute();
        }

        public static void MultiInsertOnDuplicateUpdateRaw<T>(this WowtraceDB db, ITable<T> table, IEnumerable<T> source, IEnumerable<string> updateColumnsRaw = null, BulkCopyOptions options = null)
        {
            if (options == null)
                options = new BulkCopyOptions();

            var helper = BuildBulkCopySql(table, source, options, "INSERT INTO");

            if (updateColumnsRaw == null)
                helper.Columns.Select(c => c.ColumnName).ToList();

            helper.StringBuilder
                    .AppendLine()
                    .Append("ON DUPLICATE KEY UPDATE");

            foreach (var updateColumnRaw in updateColumnsRaw)
            {
                helper.StringBuilder
                    .AppendLine()
                    .Append("\t ")
                    .Append(updateColumnRaw);
            }

            helper.Execute();
        }

        private static MultipleRowsHelper BuildBulkCopySql<T>(ITable<T> table, IEnumerable<T> source, BulkCopyOptions options, string insertMode)
        {
            var helper = new MultipleRowsHelper<T>(table, options);

            helper.StringBuilder
                .AppendFormat("{0} {1}", insertMode, helper.TableName).AppendLine()
                .Append('(');

            foreach (var column in helper.Columns)
            {
                helper.StringBuilder
                    .AppendLine()
                    .Append('\t');
                helper.SqlBuilder.Convert(helper.StringBuilder, column.ColumnName, ConvertType.NameToQueryField);
                helper.StringBuilder.Append(',');
            }

            helper.StringBuilder.Length--;
            helper.StringBuilder
                .AppendLine()
                .Append(')');

            helper.StringBuilder
                .AppendLine()
                .Append("VALUES");

            helper.SetHeader();

            foreach (var item in source)
            {
                helper.StringBuilder
                .AppendLine()
                .Append('(');
                helper.BuildColumns(item);
                helper.StringBuilder.Append("),");

                helper.RowsCopied.RowsCopied++;
                helper.CurrentCount++;
            }

            helper.StringBuilder.Length--;

            return helper;
        }
    }
}
