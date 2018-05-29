using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuroGus.Core.Model;

namespace NeuroGus.Data
{
    public class DbEntities : DbContext
    {
        // Имя будущей базы данных можно указать через
        // вызов конструктора базового класса
        public DbEntities() : base("DBConnection")
        { }

        // Отражение таблиц базы данных на свойства с типом DbSet
        public DbSet<VocabularyWord> VocabularyWords { get; set; }
        public DbSet<Characteristic> Characteristics { get; set; }
        public DbSet<CharacteristicValue> CharacteristicValues { get; set; }
        public DbSet<ClassifiableText> ClassifiableTexts { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<CharacteristicValue>().HasRequired(c => c.Characteristic)
        //        .WithMany(c => c.PossibleValues).HasForeignKey(c => c.CharacteristicId);
        //}
    }
}
