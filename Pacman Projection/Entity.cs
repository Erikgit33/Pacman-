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
        /// <summary>
        /// The box (pivturebox) component of the Entity. It holds all images and displays the entity. 
        /// </summary>
        public PictureBox box = new PictureBox();

        /// <summary>
        /// The entity's current direction.
        /// </summary>
        public Direction CurrentDirection { get; private set; }

        /// <summary>
        /// The entity's current state.
        /// </summary>
        public EntityState CurrentState { get; private set; } = EntityState.Standard;

        /// <summary>
        /// Indicates whether the ghost has teleported or not, used while in the teleporting state.
        /// </summary>
        public bool Teleported { get; set; }

        /// <summary>
        /// Indicates how many blocks "into the teleporter", off the map, the ghost is. 
        /// </summary>
        public int BlocksIntoTeleporter { get; set; }

        /// <summary>
        /// The entity's current X index.
        /// </summary>
        public int CurrentPosX { get; internal set; }

        /// <summary>
        /// The entity's current Y index.
        /// </summary>
        public int CurrentPosY { get; internal set; }

        /// <summary>
        /// The entity's current position as [x, y] indexes.    
        /// </summary>
        public int[] CurrentPos { get; internal set; }

        /// <summary>
        /// The entity's lower-left standard-position box's position. 
        /// </summary>
        private int[] LowerLeftStandardPos = new int[2];
        /// <summary>
        /// The entity's upper-left standard-position box's position. 
        /// </summary>
        private int[] UpperLeftStandardPos = new int[2];
        /// <summary>
        /// The entity's upper-right standard-position box's position. 
        /// </summary>
        private int[] UpperRightStandardPos = new int[2];
        /// <summary>
        /// The entity's lower-right standard-position box's position. 
        /// </summary>
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
                LowerLeftStandardPos[0] = box.Left / GameConstants.BoxSize;
                LowerLeftStandardPos[1] = (box.Top + GameConstants.BoxSize - GameConstants.BoxOffset_Vertical) / GameConstants.BoxSize;

                // Crash preventing
                if (LowerLeftStandardPos[0] > GameConstants.Boxes_Horizontally - 1)
                {
                    LowerLeftStandardPos[0] = GameConstants.Boxes_Horizontally - 1;
                }
                if (LowerLeftStandardPos[1] > GameConstants.Boxes_Vertically - 1)
                {
                    LowerLeftStandardPos[1] = GameConstants.Boxes_Vertically - 1;
                }

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
