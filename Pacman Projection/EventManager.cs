using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{
    public class EventManager
    {
        SoundManager soundManager = new SoundManager();

        public void ButtonPress()
        {
            Task.Run(async() => 
            {
                soundManager.PlaySound(Sounds.buttonPress, false);
                await Task.Delay(GameConstants.EventTimes.buttonDelay);
            });
        }
    }
}
