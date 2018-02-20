using DapperLambdaExtensions.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Domain
{
    public class Hero : IEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Data { get; set; }

        [ForeignKey("HeroHabilities")]
        public int HeroHabilitiesId { get; set; }        
        public virtual HeroHabilities HeroHabilities { get; set; }
    }

    public class HeroHabilities : IEntity
    {
        [Key]
        public int Id { get; set; }
        public string Skill { get; set; }
        public bool Adquirido { get; set; }
        public DateTime DataHabilidade { get; set; }

        [ForeignKey("SkillXPTO")]
        public int SkillXptoId { get; set; }
        public virtual SkillXPTO SkillXPTO { get; set; }
    }

    public class SkillXPTO
    {
        [Key]
        public int Id { get; set; }
        public string Descricao { get; set; }
    }
}
