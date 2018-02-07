using Dapper;
using DapperLambdaExtensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DapperLambdaExtensions
{
    public abstract class DapperRepository<TEntity> : IRepository<TEntity>
        where TEntity : IEntity
    {
        protected readonly IDbConnection dbConnection;

        public DapperRepository(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public int Add(TEntity entity)
        {
            var properties = GetPropertiesWithValuesNotNull(entity);

            var insert = new StringBuilder();
            var values = new StringBuilder();

            insert.AppendFormat(@"insert into {0} ", typeof(TEntity).Name);
            insert.Append(@"( ");

            foreach (var prop in properties)
            {
                insert.AppendFormat(@"{0},", prop.Name);
                values.AppendFormat(@":{0},", prop.Name);
            }

            insert = insert.Remove(insert.Length - 1, 1);
            values = values.Remove(insert.Length - 1, 1);

            insert.Append(@") ");
            insert.Append(@"values");
            insert.Append(@"(");
            insert.Append(values);
            insert.Append(@")");

            var sql = insert.ToString();

            return dbConnection.Execute(sql, entity);
        }

        public int Delete(TEntity entity)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"delete from {0} where Id = :Id", typeof(TEntity).Name);

            var sql = sb.ToString();

            return dbConnection.Execute(sql, entity);
        }

        public IEnumerable<TModel> Get<TModel>(Expression<Func<TEntity, TModel>> lambda, IDictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder(@"select ");
            var expressions = (MemberInitExpression)lambda.Body;

            foreach (var item in expressions.Bindings)
            {
                var model = item.Member.Name;
                var entity = ((MemberExpression)((MemberAssignment)item).Expression).Member.Name;

                sb.AppendFormat(@"{0} as {1},", entity, model);
            }

            sb = sb.Remove(sb.Length - 1, 1);

            sb.AppendFormat(@" from {0} ", typeof(TEntity).Name);

            var sqlParam = new DynamicParameters();

            if (parameters != null && parameters.Any())
            {
                sb.Append(@"where ");

                foreach (var param in parameters)
                {
                    sb.AppendFormat(@"{0} = :{0} and ", param.Key);

                    sqlParam.Add(string.Format(":{0}", param.Key), param.Value);
                }

                sb = sb.Remove(sb.Length - (" and ").Length, (" and ").Length);
            }

            var sql = sb.ToString();

            return dbConnection.Query<TModel>(sql, sqlParam);
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, object>> lambda, IDictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder(@"select ");
            var expressions = (NewExpression)lambda.Body;

            foreach (var item in expressions.Arguments)
            {
                var modelo = ((MemberExpression)item).Member.Name;

                sb.AppendFormat(@"{0},", modelo);
            }

            sb = sb.Remove(sb.Length - 1, 1);

            sb.AppendFormat(@" from {0} ", typeof(TEntity).Name);

            var sqlParam = new DynamicParameters();

            if (parameters != null && parameters.Any())
            {
                sb.Append(@"where ");

                foreach (var param in parameters)
                {
                    sb.AppendFormat(@"{0} = :{0} and ", param.Key);

                    sqlParam.Add(string.Format(":{0}", param.Key), param.Value);
                }

                sb = sb.Remove(sb.Length - (" and ").Length, (" and ").Length);
            }

            var sql = sb.ToString();

            return dbConnection.Query<TEntity>(sql, sqlParam);
        }

        public int Update(TEntity entity)
        {
            var properties = GetPropertiesWithValuesNotNull(entity);

            var sb = new StringBuilder();
            sb.AppendFormat(@"update {0} ", typeof(TEntity).Name);
            sb.Append(@"set ");

            foreach (var prop in properties)
            {
                sb.AppendFormat(@"{0} = :{0},", prop.Name);
            }

            sb = sb.Remove(sb.Length - 1, 1);

            sb.Append(@" where Id = :Id");

            var sql = sb.ToString();

            return dbConnection.Execute(sql, entity);
        }

        public int Update(Expression<Func<bool, TEntity>> lambda, IDictionary<string, object> parameters = null)
        {
            var sqlParam = new DynamicParameters();
            var expressions = (MemberInitExpression)lambda.Body;

            var sb = new StringBuilder();
            sb.AppendFormat(@"update {0} ", typeof(TEntity).Name);
            sb.Append(@"set ");

            foreach (var item in expressions.Bindings)
            {
                sb.AppendFormat(@"{0} = :{0},", item.Member.Name);

                sqlParam.Add(
                    string.Format(":{0}", item.Member.Name),
                    ((ConstantExpression)((MemberAssignment)item).Expression).Value
                );
            }

            sb = sb.Remove(sb.Length - 1, 1);

            if (parameters != null & parameters.Any())
            {
                sb.Append(@" where ");

                foreach (var param in parameters)
                {
                    sb.AppendFormat(@"{0} = :{0} and ", param.Key);

                    sqlParam.Add(string.Format(":{0}", param.Key), param.Value);
                }

                sb = sb.Remove(sb.Length - (" and ").Length, (" and ").Length);
            }

            var sql = sb.ToString();

            return dbConnection.Execute(sql, sqlParam);
        }

        private static IEnumerable<PropertyInfo> GetPropertiesWithValuesNotNull(TEntity entity)
        {
            return typeof(TEntity).GetProperties().Where(p => p.GetValue(entity, null) != null);
        }
    }
}
