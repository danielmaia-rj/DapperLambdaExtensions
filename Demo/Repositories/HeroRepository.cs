using DapperLambdaExtensions;
using System.Data;
using Demo.Domain;

namespace Demo.Repositories
{
    public class HeroRepository : DapperRepository<Hero>, IHeroRepository
    {
        public HeroRepository(IDbConnection dbConnection) 
            : base(dbConnection)
        {
        }
    }
}
