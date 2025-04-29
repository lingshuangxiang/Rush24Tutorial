using TwentyFour.Scripts.Tournament;
using UnityEditor;

namespace Unity.UOS.TwentyFour.Editor
{
    public class TournamentDataEditor
    {
        [MenuItem("/Tools/TournamentData/GenerateJson")]
        static void GenerateJson()
        {
            TournamentData.GenJson();
        }
    }
}