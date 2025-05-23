using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwentyFour.Scripts.Features.Player
{
    public class Persona
    {
        public string PersonaID;
        public string DisplayName;
        public Dictionary<string, string> Properties;
    }
    public class Identity
    {
        public static Persona persona;
    }
}