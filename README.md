# DapperLambdaExtensions

The goal here is build some implementation on a base repository that make easier some sql select operations when using [Dapper](https://github.com/StackExchange/Dapper) as ORM framework.


Usage:
------------
I tried to make something similar to the approach when we uses DdSet and Linq

<br />

Select one or many properties from a entity
------------

### Approach when using this extension

    var partialHero = heroRepository.Get((Hero hero) =>
        new
        {
            hero.Name
        }
    );


### Approach when using DbSet and Linq

    var partialHero = Hero.Select(hero => hero.Name);

<br />

Select a entity into a new class
------------

### Approach when using this extension

      var heroMapToHeroModel = heroRepository.Get(entity =>
          new HeroModel()
          {
              Name = entity.Name
          }
      );


### Approach when using DbSet and Linq

      var heroMapToHeroModel = Hero.Select(entity => 
          new HeroModel()
          {
              Name = entity.Name
          }
      );
      
<br />

### _There is some demos in the web api project_
