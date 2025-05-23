using UnityEngine;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    public class MainSceneManager : MonoBehaviour
    {

        public void Logout()
        {
            ClientInitHelper.Logout();
        }

        // Start is called before the first frame update
        void Start()
        {
          
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
