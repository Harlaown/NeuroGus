using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroGus.Core.Model
{
    public partial class KeyValuePair
    {
        public int ClassifiableTextId { get; set; }
        public int CharacteristicsValueId { get; set; }
        public int CharacteristicsNameId { get; set; }
        [Key]
        public int ClassifiableTextsCharacteristicsId { get; set; }

        public virtual Characteristic Key { get; set; }
        public virtual CharacteristicValue Value { get; set; }
    }
}
