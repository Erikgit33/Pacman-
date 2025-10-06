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

        internal Direction currentDirection { get; private set; }

        internal EntityState currentState { get; private set; }

        internal bool teleportedLastTick { get; set; }
        internal int blocksIntoTeleporter { get; set; }

        internal int currentPosX { get; set; }
        internal int currentPosY { get; set; }

        private int[] lowerLeftStandardPos = new int[2];
        private int[] upperLeftStandardPos = new int[2];
        private int[] upperRightStandardPos = new int[2];
        private int[] lowerRightStandardPos = new int[2];

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
                    return new int[] { lowerLeftStandardPos[0], lowerLeftStandardPos[1] };
                case EntityBox.UpperLeft:  
                    return new int[] { upperLeftStandardPos[0], upperLeftStandardPos[1] };
                case EntityBox.UpperRight:
                    return new int[] { upperRightStandardPos[0], upperRightStandardPos[1] };
                case EntityBox.LowerRight:
                    return new int[] { lowerRightStandardPos[0], lowerRightStandardPos[1] };
                default:
                    return null;
            }
        }

        public void UpdateStandardPositions()
        {
            lowerLeftStandardPos[0] = box.Left / GameConstants.boxSize;
            lowerLeftStandardPos[1] = (box.Top + GameConstants.boxSize - GameConstants.boxOffset_Vertical) / GameConstants.boxSize;

            upperLeftStandardPos[0] = lowerLeftStandardPos[0];
            upperLeftStandardPos[1] = lowerLeftStandardPos[1] - 1;

            upperRightStandardPos[0] = lowerLeftStandardPos[0] + 1;
            upperRightStandardPos[1] = lowerLeftStandardPos[1] - 1;

            lowerRightStandardPos[0] = lowerLeftStandardPos[0] + 1;
            lowerRightStandardPos[1] = lowerLeftStandardPos[1];
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
                    currentDirection = Direction.Left;
                    break;
                case Direction.Right:
                    currentDirection = Direction.Right;
                    break;
                case Direction.Up:
                    currentDirection = Direction.Up;
                    break;
                case Direction.Down:
                    currentDirection = Direction.Down;
                    break;
                case Direction.Stationary:
                    currentDirection = Direction.Stationary;
                    break;
            }
        }

        public void SetState(EntityState state)
        {
            switch (state)
            {
                case EntityState.Standard:
                    currentState = EntityState.Standard;
                    break;
                case EntityState.Teleporting:
                    currentState = EntityState.Teleporting;
                    break;
                case EntityState.Eaten:
                    currentState = EntityState.Eaten;
                    break;
            }
        }
    }
}
