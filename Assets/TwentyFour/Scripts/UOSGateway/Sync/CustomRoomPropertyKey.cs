using System;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    public class CustomRoomPropertyKey
    {
        public const string ROOM_STATE = "room_state";
        public const string TournamentSlugName = "tournament_slug_name";
        
        public static CustomRoomState GetCustomRoomState(string state)
        {
            if (Enum.TryParse(typeof(CustomRoomState), state, true, out var currentState))
            {
                return (CustomRoomState)currentState;
            }

            return CustomRoomState.Default;
        }

    }

    public enum CustomRoomState
    {
        Default,
        InBattle
    }
}