using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    public class ExitGame
    {
        public void Exit()
        {
            Application.Quit();
        }
    }
}