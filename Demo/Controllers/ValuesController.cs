using Demo.Application;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IHeroService heroService;

        public ValuesController(IHeroService heroService)
        {
            this.heroService = heroService;
        }

        [HttpGet]
        public string Get()
        {
            heroService.SelectHeroName();
            heroService.SelectHeroModelFromHeroEntity();
            heroService.UpdateHeroNameOnly();

            return "value1";
        }        
    }
}
