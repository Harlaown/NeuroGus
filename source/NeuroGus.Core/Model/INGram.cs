using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroGus.Core.Model
{
    public interface INGram
    {
        HashSet<string> GetNGram(string text);
    }
}
