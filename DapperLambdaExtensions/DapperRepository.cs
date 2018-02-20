using Dapper;
using DapperLambdaExtensions.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public IEnumerable<TModel> GetModel<TModel>(Expression<Func<TEntity, TModel>> lambda, IDictionary<string, object> parameters = null)
        {
            var expression = (MemberInitExpression)lambda.Body;

            var relationship = new Dictionary<Type, Type>();

            var mainSelect = new StringBuilder(@"select ");

            var dynamicParameters = new DynamicParameters();

            BuildMainSelect(mainSelect, relationship, expression);

            BuildFrom(mainSelect, relationship);

            BuildWhere(mainSelect, parameters, dynamicParameters);

            var sql = mainSelect.ToString();

            return dbConnection.Query<TModel>(sql, dynamicParameters);
        }

        public IEnumerable<TEntity> GetEntity(Expression<Func<TEntity, object>> lambda, IDictionary<string, object> parameters = null)
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

        private static void BuildMainSelect(StringBuilder mainSelect, Dictionary<Type, Type> relationship, MemberInitExpression expressions)
        {
            foreach (var item in expressions.Bindings)
            {
                var expression = ((MemberExpression)((MemberAssignment)item).Expression);
                var entity = expression.Expression.Type;

                var entityPropertyName = expression.Member.Name;
                var modelPropertyName = item.Member.Name;

                var memberAssignment = (MemberAssignment)item;
                var memberExpression = (MemberExpression)memberAssignment.Expression;

                if (memberExpression.Expression is MemberExpression memberExpressionChild)
                {                 
                    if (!(relationship.TryGetValue(entity, out Type type) && type == memberExpressionChild.Member.ReflectedType))
                    {
                        relationship.Add(entity, memberExpressionChild.Member.ReflectedType);
                    }
                }

                mainSelect.AppendFormat(@"{0}.{1} as {2},",
                    entity.Name.ToLower(), entityPropertyName, modelPropertyName);
            }

            mainSelect = mainSelect.Remove(mainSelect.Length - 1, 1);
        }

        private static void BuildFrom(StringBuilder mainSelect, Dictionary<Type, Type> relationship)
        {
            mainSelect.AppendFormat(@" from {0} ", typeof(TEntity).Name);

            foreach (var item in relationship)
            {
                var foreignKeyFromMainEntity = item.Value
                    .GetProperties()
                    .Where(propInfo =>
                        propInfo.GetCustomAttributes(true).Any(attr => attr.GetType() == typeof(ForeignKeyAttribute)) &&
                        propInfo.GetCustomAttribute<ForeignKeyAttribute>().Name == item.Key.Name)
                    .FirstOrDefault();

                var primaryKeyFromEntityRelationship = item.Key
                    .GetProperties()
                    .Where(propInfo => propInfo.GetCustomAttributes(true).Any(a => a.GetType() == typeof(KeyAttribute)))
                    .FirstOrDefault();

                mainSelect.AppendFormat(@"inner join {0} ", item.Key.Name);
                mainSelect.AppendFormat(@"on {0}.{2} = {1}.{3} ",
                    item.Value.Name, item.Key.Name,
                    foreignKeyFromMainEntity.Name, primaryKeyFromEntityRelationship.Name);
            }
        }

        private static void BuildWhere(StringBuilder mainSelect, IDictionary<string, object> parameters, DynamicParameters dynamicParameters)
        {
            if (parameters != null && parameters.Any())
            {
                mainSelect.Append(@"where ");

                foreach (var param in parameters)
                {
                    mainSelect.AppendFormat(@"{0} = :{1} and ", param.Key, param.Key.Replace(".", ""));

                    dynamicParameters.Add(string.Format(":{0}", param.Key.Replace(".", "")), param.Value);
                }

                mainSelect = mainSelect.Remove(mainSelect.Length - (" and ").Length, (" and ").Length);
            }
        }
    }
}
