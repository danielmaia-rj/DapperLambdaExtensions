using Demo.Domain;
using Demo.Model;
using Demo.Repositories;
using System.Collections.Generic;

namespace Demo.Application
{
    public class HeroService : IHeroService
    {
        private readonly IHeroRepository heroRepository;

        public HeroService(IHeroRepository heroRepository)
        {
            this.heroRepository = heroRepository;
        }

        /// <summary>
        /// Builds and execute a select with name property only. The other properties in the class will not be considered in the select
        /// </summary>
        /// <param name="heroRepository"></param>
        public void SelectHeroName()
        {
            var partialHero = heroRepository.GetModel((Hero hero) =>
                new
                {
                    hero.Name
                }
            );
        }

        /// <summary>
        /// Builds and execute a select on entity 'Hero', selecting only the property 'Name' and Mapping it into 'HeroModel' class
        /// </summary>
        /// <param name="heroRepository"></param>
        public void SelectHeroModelFromHeroEntity()
        {
            var heroMapToHeroModel = heroRepository.GetModel(entity =>
                new HeroModel()
                {
                    Name = entity.Name,
                    Skill = entity.HeroHabilities.Skill,
                    XPTO = entity.HeroHabilities.SkillXPTO.Descricao,
                    Adquirido = entity.HeroHabilities.Adquirido,                    
                    Data = entity.HeroHabilities.DataHabilidade
                }
            );
        }

        /// <summary>
        /// Builds and execute an update on entity 'Hero', only in the 'Name' column
        /// </summary>
        /// <param name="heroRepository"></param>
        public void UpdateHeroNameOnly()
        {
            // Name of column and value to be used into where conditional.
            var parameter = new Dictionary<string, object>()
                {
                    { "Id", 10 }
                };

            heroRepository.Update(hero =>
                new Hero()
                {
                    Name = "Updated name"
                },
                parameter
            );
        }
    }
}
