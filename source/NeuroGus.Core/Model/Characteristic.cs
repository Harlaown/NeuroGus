using System;
using System.Collections.Generic;

namespace NeuroGus.Core.Model
{
    public partial class Characteristic : IComparable
    {
        public Characteristic()
        {
            PossibleValues = new HashSet<CharacteristicValue>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CharacteristicValue> PossibleValues { get; set; }

        private Characteristic(int id, string name, ICollection<CharacteristicValue> possibleValues)
        {
            Id = id;
            Name = name;
            PossibleValues = possibleValues;
        }

        public Characteristic(int id, string name) : this(id, name, new HashSet<CharacteristicValue>())
        {
        }

        public Characteristic(string name, ICollection<CharacteristicValue> possibleValues) : this(0, name, possibleValues)
        {
        }

        public Characteristic(string name) : this(0, name, new HashSet<CharacteristicValue>())
        {
        }

        public override bool Equals(object o)
        {
            return o is Characteristic characteristic && Name.Equals(characteristic.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return Convert.ToInt32(Equals(obj));
        }
    }
}