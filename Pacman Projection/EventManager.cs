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
            Task.Run(() => 
            {
                soundManager.PlaySound(Sounds.buttonPress, false);
                Task.Delay(GameConstants.EventTimes.buttonDelay).Wait();
            });
        }
    }
}
