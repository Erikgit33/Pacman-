using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    internal class Entity
    {
        public PictureBox box = new PictureBox();

        internal Direction CurrentDirection { get; private set; }

        internal EntityState CurrentState { get; private set; }

        internal bool Teleported { get; set; }
        internal int BlocksIntoTeleporter { get; set; }

        internal int CurrentPosX { get; set; }
        internal int CurrentPosY { get; set; }

        private int[] LowerLeftStandardPos = new int[2];
        private int[] UpperLeftStandardPos = new int[2];
        private int[] UpperRightStandardPos = new int[2];
        private int[] LowerRightStandardPos = new int[2];

        public Entity()
        {
            box.LocationChanged += EntityMoved;
        }

        public int[] GetStandardPosition(EntityBox box)
        {
            switch (box)
            {
                case EntityBox.LowerLeft:
                    // Only return the values as new integers to avoid external modification of the actual position values (aliasing)
                    return new int[] { LowerLeftStandardPos[0], LowerLeftStandardPos[1] };
                case EntityBox.UpperLeft:  
                    return new int[] { UpperLeftStandardPos[0], UpperLeftStandardPos[1] };
                case EntityBox.UpperRight:
                    return new int[] { UpperRightStandardPos[0], UpperRightStandardPos[1] };
                case EntityBox.LowerRight:
                    return new int[] { LowerRightStandardPos[0], LowerRightStandardPos[1] };
                default:
                    return null;
            }
        }

        public void UpdateStandardPositions()
        {
            // If the entity is teleporting, do not update the positions properly until it has fully teleporting
            // During teleportaion an entity is "invincible", so the positions do not matter (boxes[0,0] is a wall)
            if (CurrentState.Equals(EntityState.Teleporting))
            {
                LowerLeftStandardPos[0] = 0;
                LowerLeftStandardPos[1] = 0;

                UpperLeftStandardPos[0] = LowerLeftStandardPos[0];
                UpperLeftStandardPos[1] = LowerLeftStandardPos[1];

                UpperRightStandardPos[0] = LowerLeftStandardPos[0];
                UpperRightStandardPos[1] = LowerLeftStandardPos[1];

                LowerRightStandardPos[0] = LowerLeftStandardPos[0];
                LowerRightStandardPos[1] = LowerLeftStandardPos[1];
            }
            else
            {
                LowerLeftStandardPos[0] = box.Left / GameConstants.boxSize;
                LowerLeftStandardPos[1] = (box.Top + GameConstants.boxSize - GameConstants.boxOffset_Vertical) / GameConstants.boxSize;

                UpperLeftStandardPos[0] = LowerLeftStandardPos[0];
                UpperLeftStandardPos[1] = LowerLeftStandardPos[1] - 1;

                UpperRightStandardPos[0] = LowerLeftStandardPos[0] + 1;
                UpperRightStandardPos[1] = LowerLeftStandardPos[1] - 1;

                LowerRightStandardPos[0] = LowerLeftStandardPos[0] + 1;
                LowerRightStandardPos[1] = LowerLeftStandardPos[1];
            }
        }

        public void EntityMoved(object sender, EventArgs e)
        {
            UpdateStandardPositions();
        }

        public void SetDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    CurrentDirection = Direction.Left;
                    break;
                case Direction.Right:
                    CurrentDirection = Direction.Right;
                    break;
                case Direction.Up:
                    CurrentDirection = Direction.Up;
                    break;
                case Direction.Down:
                    CurrentDirection = Direction.Down;
                    break;
                case Direction.Stationary:
                    CurrentDirection = Direction.Stationary;
                    break;
            }
        }

        public void SetState(EntityState state)
        {
            switch (state)
            {
                case EntityState.Standard:
                    CurrentState = EntityState.Standard;
                    break;
                case EntityState.Teleporting:
                    CurrentState = EntityState.Teleporting;
                    break;
                case EntityState.Eaten:
                    CurrentState = EntityState.Eaten;
                    break;
            }
        }
    }
}
