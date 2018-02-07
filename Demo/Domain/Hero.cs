using DapperLambdaExtensions.Interfaces;

namespace Demo.Domain
{
    public class Hero : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
