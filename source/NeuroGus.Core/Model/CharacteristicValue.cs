using System.ComponentModel.DataAnnotations;

namespace NeuroGus.Core.Model
{
    public partial class CharacteristicValue
    {
        [Key]
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string Value { get; set; }
        public int? CharacteristicId { get; set; }

        public virtual Characteristic Characteristic { get; set; }

        public CharacteristicValue()
        {
        }

        public CharacteristicValue(int orderNumber, string value)
        {
            OrderNumber = orderNumber;
            Value = value;
        }

        public CharacteristicValue(string value)
        {
            Id = 0;
            Value = value;
        }

        public override bool Equals(object o)
        {
            return o is CharacteristicValue otherCharacteristicValue &&
                   Value.Equals(otherCharacteristicValue.Value);
        }


        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}