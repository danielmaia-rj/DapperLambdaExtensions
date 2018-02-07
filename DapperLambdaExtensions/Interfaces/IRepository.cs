using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DapperLambdaExtensions.Interfaces
{
    public interface IRepository<TEntity> where TEntity : IEntity
    {
        int Add(TEntity entity);
        int Update(TEntity entity);
        int Delete(TEntity entity);
        int Update(Expression<Func<bool, TEntity>> lambda, IDictionary<string, object> param);
        IEnumerable<TModel> Get<TModel>(Expression<Func<TEntity, TModel>> lambda, IDictionary<string, object> parametros = null);
        IEnumerable<TEntity> Get(Expression<Func<TEntity, object>> lambda, IDictionary<string, object> param = null);
    }
}
