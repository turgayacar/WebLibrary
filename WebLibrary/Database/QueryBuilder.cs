using System.Text;

namespace WebLibrary.Database
{
    /// <summary>
    /// SQL sorgu oluşturucu
    /// </summary>
    public class QueryBuilder
    {
        private readonly StringBuilder _select = new();
        private readonly StringBuilder _from = new();
        private readonly StringBuilder _where = new();
        private readonly StringBuilder _orderBy = new();
        private readonly StringBuilder _groupBy = new();
        private readonly StringBuilder _having = new();
        private readonly List<string> _joins = new();
        private readonly Dictionary<string, object> _parameters = new();
        private int _limit = -1;
        private int _offset = -1;

        /// <summary>
        /// SELECT kısmını ekler
        /// </summary>
        /// <param name="columns">Sütunlar (virgülle ayrılmış)</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder Select(string columns = "*")
        {
            _select.Clear();
            _select.Append($"SELECT {columns}");
            return this;
        }

        /// <summary>
        /// FROM kısmını ekler
        /// </summary>
        /// <param name="table">Tablo adı</param>
        /// <param name="alias">Tablo alias'ı</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder From(string table, string? alias = null)
        {
            _from.Clear();
            _from.Append($"FROM {table}");
            if (!string.IsNullOrEmpty(alias))
                _from.Append($" AS {alias}");
            return this;
        }

        /// <summary>
        /// WHERE koşulu ekler
        /// </summary>
        /// <param name="condition">Koşul</param>
        /// <param name="parameterName">Parametre adı</param>
        /// <param name="value">Parametre değeri</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder Where(string condition, string? parameterName = null, object? value = null)
        {
            if (_where.Length > 0)
                _where.Append(" AND ");
            else
                _where.Append("WHERE ");

            _where.Append(condition);

            if (!string.IsNullOrEmpty(parameterName) && value != null)
                _parameters[parameterName] = value;

            return this;
        }

        /// <summary>
        /// OR WHERE koşulu ekler
        /// </summary>
        /// <param name="condition">Koşul</param>
        /// <param name="parameterName">Parametre adı</param>
        /// <param name="value">Parametre değeri</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder OrWhere(string condition, string? parameterName = null, object? value = null)
        {
            if (_where.Length > 0)
                _where.Append(" OR ");
            else
                _where.Append("WHERE ");

            _where.Append(condition);

            if (!string.IsNullOrEmpty(parameterName) && value != null)
                _parameters[parameterName] = value;

            return this;
        }

        /// <summary>
        /// INNER JOIN ekler
        /// </summary>
        /// <param name="table">Tablo adı</param>
        /// <param name="alias">Tablo alias'ı</param>
        /// <param name="condition">JOIN koşulu</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder InnerJoin(string table, string alias, string condition)
        {
            _joins.Add($"INNER JOIN {table} AS {alias} ON {condition}");
            return this;
        }

        /// <summary>
        /// LEFT JOIN ekler
        /// </summary>
        /// <param name="table">Tablo adı</param>
        /// <param name="alias">Tablo alias'ı</param>
        /// <param name="condition">JOIN koşulu</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder LeftJoin(string table, string alias, string condition)
        {
            _joins.Add($"LEFT JOIN {table} AS {alias} ON {condition}");
            return this;
        }

        /// <summary>
        /// RIGHT JOIN ekler
        /// </summary>
        /// <param name="table">Tablo adı</param>
        /// <param name="alias">Tablo alias'ı</param>
        /// <param name="condition">JOIN koşulu</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder RightJoin(string table, string alias, string condition)
        {
            _joins.Add($"RIGHT JOIN {table} AS {alias} ON {condition}");
            return this;
        }

        /// <summary>
        /// ORDER BY ekler
        /// </summary>
        /// <param name="column">Sütun adı</param>
        /// <param name="direction">Sıralama yönü (ASC/DESC)</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder OrderBy(string column, string direction = "ASC")
        {
            if (_orderBy.Length > 0)
                _orderBy.Append(", ");
            else
                _orderBy.Append("ORDER BY ");

            _orderBy.Append($"{column} {direction.ToUpper()}");
            return this;
        }

        /// <summary>
        /// GROUP BY ekler
        /// </summary>
        /// <param name="columns">Sütunlar (virgülle ayrılmış)</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder GroupBy(string columns)
        {
            if (_groupBy.Length > 0)
                _groupBy.Append(", ");
            else
                _groupBy.Append("GROUP BY ");

            _groupBy.Append(columns);
            return this;
        }

        /// <summary>
        /// HAVING koşulu ekler
        /// </summary>
        /// <param name="condition">Koşul</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder Having(string condition)
        {
            if (_having.Length > 0)
                _having.Append(" AND ");
            else
                _having.Append("HAVING ");

            _having.Append(condition);
            return this;
        }

        /// <summary>
        /// LIMIT ekler
        /// </summary>
        /// <param name="limit">Limit değeri</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder Limit(int limit)
        {
            _limit = limit;
            return this;
        }

        /// <summary>
        /// OFFSET ekler
        /// </summary>
        /// <param name="offset">Offset değeri</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder Offset(int offset)
        {
            _offset = offset;
            return this;
        }

        /// <summary>
        /// Parametre ekler
        /// </summary>
        /// <param name="name">Parametre adı</param>
        /// <param name="value">Parametre değeri</param>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder AddParameter(string name, object value)
        {
            _parameters[name] = value;
            return this;
        }

        /// <summary>
        /// SQL sorgusunu oluşturur
        /// </summary>
        /// <returns>SQL sorgusu</returns>
        public string Build()
        {
            var query = new StringBuilder();

            if (_select.Length > 0)
                query.Append(_select);

            if (_from.Length > 0)
                query.Append(" ").Append(_from);

            foreach (var join in _joins)
            {
                query.Append(" ").Append(join);
            }

            if (_where.Length > 0)
                query.Append(" ").Append(_where);

            if (_groupBy.Length > 0)
                query.Append(" ").Append(_groupBy);

            if (_having.Length > 0)
                query.Append(" ").Append(_having);

            if (_orderBy.Length > 0)
                query.Append(" ").Append(_orderBy);

            if (_limit > 0)
            {
                if (_offset > 0)
                    query.Append($" OFFSET {_offset} ROWS FETCH NEXT {_limit} ROWS ONLY");
                else
                    query.Append($" OFFSET 0 ROWS FETCH NEXT {_limit} ROWS ONLY");
            }

            return query.ToString();
        }

        /// <summary>
        /// Parametreleri alır
        /// </summary>
        /// <returns>Parametre dictionary'si</returns>
        public Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object>(_parameters);
        }

        /// <summary>
        /// QueryBuilder'ı temizler
        /// </summary>
        /// <returns>QueryBuilder instance</returns>
        public QueryBuilder Clear()
        {
            _select.Clear();
            _from.Clear();
            _where.Clear();
            _orderBy.Clear();
            _groupBy.Clear();
            _having.Clear();
            _joins.Clear();
            _parameters.Clear();
            _limit = -1;
            _offset = -1;
            return this;
        }
    }
}
