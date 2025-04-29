using System.Linq;
using Passport;
using Unity.Passport.Runtime;
using System.Threading.Tasks;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    public class Identity
    {
        private static string DEFAULT_REALM_ID = "";
        public static Persona persona;

        public static async Task<string> GetRealmID()
        {
            if (!string.IsNullOrEmpty(DEFAULT_REALM_ID)) return DEFAULT_REALM_ID;
            var realms = await PassportSDK.Identity.GetRealms();
            if (realms.Any())
            {
                DEFAULT_REALM_ID =  realms[0].RealmID;
            }
            return DEFAULT_REALM_ID;
        }
    }
}