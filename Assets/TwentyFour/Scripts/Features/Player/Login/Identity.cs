using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unity.UOS.TwentyFour.UOSGateway
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