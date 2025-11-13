using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Pacman_Projection
{
    internal class Ghost : Entity
    {
        /// <summary>
        /// The navbox (picturebox) component of the Ghost. Used for navigation and pathfinding.
        /// Always placed in the middle of the ghost entity, following the gameGrid.
        /// </summary>
        public PictureBox navBox = new PictureBox();
        /// <summary>
        /// The current behaviour of the ghost.
        /// </summary>
        public GhostBehaviour CurrentBehaviour { get; private set; }
        /// <summary>
        /// Determines whether the ghost's behaviour is currently overridden or not.
        /// If it is, it will follow its own behaviour hierarchy rather than the global behaviour.
        /// </summary>
        public bool BehaviourOverridden { get; set; }
        /// <summary>
        /// Indicates for how long the ghosts behaviour has been overridden.
        /// </summary>
        public int BehaviourOverrideDuration { get; set; }
        /// <summary>
        /// Indicates if the ghost is inside the ghost house.
        /// </summary>
        public bool EnteringGhostHouse { get; set; }
        /// <summary>
        /// Indicates if the ghost is exiting the ghost house.
        /// </summary>
        public bool ExitingGhostHouse { get; set; }
        /// <summary>
        /// Indicates if the ghost starts in the ghost house when the game starts.
        /// </summary>
        public bool StartsInGhostHouse { get; private set; }
        /// <summary>
        /// Indicates if the ghost has been eaten during the current frightened mode - a ghost can only be eaten once per frightened mode.
        /// </summary>
        public bool HasBeenEaten { get; set; } 
        /// <summary>
        /// Indicates if the ghost is "white", if it should have it's white image-variant while frightened.
        /// </summary>
        public bool White { get; set; }
        /// <summary>
        /// The target's X index.
        /// </summary>
        public int TargetPosX { get; set; }
        /// <summary>
        /// The target's Y index.
        /// </summary>
        public int TargetPosY { get; set; }
        /// <summary>
        /// The ghost's target position as [x, y] indexes.
        /// </summary>
        public int[] TargetPos { get; set; }
        /// <summary>
        /// The corner of the map the ghost targets while scattering.
        /// </summary>
        public MapCorner ScatterCorner { get; private set; }
        /// <summary>
        /// The ghost's starting X index.
        /// </summary>
        public int StartX { get; private set; }

        /// <summary>
        /// The ghost's starting Y index.
        /// </summary>
        public int StartY { get; private set; }

        /// <summary>
        /// The ghost's starting Image.
        /// </summary>
        public Image StartImage { get; private set; }

        /// <summary>
        /// The ghost's starting direction.
        /// </summary>
        public Direction StartDirection { get; private set; }

        public Dictionary<ImageType, Image> Images { get; private set; }

        /// <summary>
        /// The template the ghost was created according to. 
        /// </summary>
        public GhostTemplate Template { get; private set; }

        /// <summary>
        /// Contains the behaviour hierarchy for the ghost.
        /// The lower the index, the higher the priority the behaviour has.
        /// A ghost only follows its own specified behaviour hierarchy if its behaviour is overridden - if the ghost's behaviour is overridden
        /// and the global behaviour attempts to change it, the ghost will check if the new (global) behaviour can override its current behaviour 
        /// based on the hierarchy specified here.
        /// </summary>
        private List<GhostBehaviour> BehaviourHierarchy;  

        public Ghost(GhostTemplate ghostTemplate)
        {
            box.LocationChanged += UpdateLocation;

            switch (ghostTemplate)
            {
                case GhostTemplate.Blinky:
                    Template = GhostTemplate.Blinky;

                    ScatterCorner = MapCorner.TopRight;

                    StartX = GhostConstants.Blinky.StartX;
                    StartY = GhostConstants.Blinky.StartY;

                    StartDirection = GhostConstants.Blinky.StartDirection;
                    StartImage = GhostConstants.Images.Blinky.StartImage;

                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Blinky.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Blinky.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Blinky.up },
                        { ImageType.Up2, GhostConstants.Images.Blinky.up2 },
                        { ImageType.Down, GhostConstants.Images.Blinky.down },
                        { ImageType.Down2, GhostConstants.Images.Blinky.down2 },
                        { ImageType.Left, GhostConstants.Images.Blinky.left },
                        { ImageType.Left2, GhostConstants.Images.Blinky.left2 },
                        { ImageType.Right, GhostConstants.Images.Blinky.right },
                        { ImageType.Right2, GhostConstants.Images.Blinky.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 }
                    };

                    BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
                    break;
                case GhostTemplate.Pinky:
                    Template = GhostTemplate.Pinky;

                    ScatterCorner = MapCorner.TopLeft;

                    StartX = GhostConstants.Pinky.StartX;
                    StartY = GhostConstants.Pinky.StartY;
                    StartsInGhostHouse = true;
                    ExitingGhostHouse = true;

                    StartDirection = GhostConstants.Pinky.StartDirection;
                    StartImage = GhostConstants.Images.Pinky.StartImage;

                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Pinky.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Pinky.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Pinky.up },
                        { ImageType.Up2, GhostConstants.Images.Pinky.up2 },
                        { ImageType.Down, GhostConstants.Images.Pinky.down },
                        { ImageType.Down2, GhostConstants.Images.Pinky.down2 },
                        { ImageType.Left, GhostConstants.Images.Pinky.left },
                        { ImageType.Left2, GhostConstants.Images.Pinky.left2 },
                        { ImageType.Right, GhostConstants.Images.Pinky.right },
                        { ImageType.Right2, GhostConstants.Images.Pinky.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 }
                    };

                    BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
                    break;
                case GhostTemplate.Inky:
                    Template = GhostTemplate.Inky;

                    ScatterCorner = MapCorner.BottomRight;

                    StartX = GhostConstants.Inky.StartX;
                    StartY = GhostConstants.Inky.StartY;

                    StartsInGhostHouse = true;
                    ExitingGhostHouse = true;

                    StartDirection = GhostConstants.Inky.StartDirection;
                    StartImage = GhostConstants.Images.Inky.StartImage;

                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Inky.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Inky.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Inky.up },
                        { ImageType.Up2, GhostConstants.Images.Inky.up2 },
                        { ImageType.Down, GhostConstants.Images.Inky.down },
                        { ImageType.Down2, GhostConstants.Images.Inky.down2 },
                        { ImageType.Left, GhostConstants.Images.Inky.left },
                        { ImageType.Left2, GhostConstants.Images.Inky.left2 },
                        { ImageType.Right, GhostConstants.Images.Inky.right },
                        { ImageType.Right2, GhostConstants.Images.Inky.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 }
                    };

                    BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
                    break;
                case GhostTemplate.Clyde:
                    Template = GhostTemplate.Clyde;

                    ScatterCorner = MapCorner.BottomLeft;

                    StartX = GhostConstants.Clyde.StartX;
                    StartY = GhostConstants.Clyde.StartY;

                    StartsInGhostHouse = true;
                    ExitingGhostHouse = true;

                    StartDirection = GhostConstants.Clyde.StartDirection;
                    StartImage = GhostConstants.Images.Clyde.StartImage;

                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Clyde.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Clyde.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Clyde.up },
                        { ImageType.Up2, GhostConstants.Images.Clyde.up2 },
                        { ImageType.Down, GhostConstants.Images.Clyde.down },
                        { ImageType.Down2, GhostConstants.Images.Clyde.down2 },
                        { ImageType.Left, GhostConstants.Images.Clyde.left },
                        { ImageType.Left2, GhostConstants.Images.Clyde.left2 },
                        { ImageType.Right, GhostConstants.Images.Clyde.right },
                        { ImageType.Right2, GhostConstants.Images.Clyde.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 }
                    };
                    
                    // Clyde's behaviour is overridden if within a certain distance of pacman, Scatter thus
                    // holds a higher prioritiy in his behaviour hierarchy.
                    BehaviourHierarchy = new List<GhostBehaviour>
                    {
                        GhostBehaviour.ExitingHouse,
                        GhostBehaviour.Returning,
                        GhostBehaviour.Frightened,
                        GhostBehaviour.Scatter,
                        GhostBehaviour.Chase
                    };

                    break;
                case GhostTemplate.Custom:
                    ScatterCorner = MapCorner.None;
                    // TODO: Custom ghost implementation
                    break;
            }

            // box.Size is the same for all GhostTemplates
            box.Size = new Size(GameConstants.EntitySize, GameConstants.EntitySize);
            navBox.Size = new Size(GameConstants.BoxSize, GameConstants.BoxSize);
        }

        private void UpdateLocation(object sender, EventArgs e)
        {
            if (Template.Equals(GhostTemplate.Blinky))
            {

            }
            navBox.Location = new Point(box.Left + navBox.Width / 2, box.Top + navBox.Width / 2);

            CurrentPosX = navBox.Left / GameConstants.BoxSize;
            CurrentPosY = (navBox.Top - GameConstants.GameGridOffset_Vertical) / GameConstants.BoxSize;
                  
            CurrentPos = new int[] { CurrentPosX, CurrentPosY };
        }

        public void UpdateLocation()
        {
            navBox.Location = new Point(box.Left + navBox.Width / 2, box.Top + navBox.Width / 2);

            CurrentPosX = navBox.Left / GameConstants.BoxSize;
            CurrentPosY = (navBox.Top - GameConstants.GameGridOffset_Vertical) / GameConstants.BoxSize;

            CurrentPos = new int[] { CurrentPosX, CurrentPosY };
        }

        internal void SetTarget(int[] targetIndex)
        {
            if (targetIndex[0] < 0)
            {
                targetIndex[0] = 0;
            }
            else if (targetIndex[0] > GameConstants.GameBoxes_Horizontally - 1)
            {
                targetIndex[0] = GameConstants.GameBoxes_Horizontally - 1;
            }

            if (targetIndex[1] < 0)
            {
                targetIndex[1] = 0;
            }
            else if (targetIndex[1] > GameConstants.GameBoxes_Vertically - 1)
            {
                targetIndex[1] = GameConstants.GameBoxes_Vertically - 1;
            }

            TargetPosX = targetIndex[0];
            TargetPosY = targetIndex[1];

            TargetPos = new int[] { TargetPosX, TargetPosY };
        }

        internal void SetTarget(int targetX, int targetY)
        {
            if (targetX < 0)
            {
                targetX = 0;
            }
            else if (targetX > GameConstants.GameBoxes_Horizontally - 1)
            {
                targetX = GameConstants.GameBoxes_Horizontally - 1;
            }

            if (targetY < 0)
            {
                targetY = 0;
            }
            else if (targetY > GameConstants.GameBoxes_Vertically - 1)
            {
                targetY = GameConstants.GameBoxes_Vertically - 1;
            }

            TargetPosX = targetX;
            TargetPosY = targetY;

            TargetPos = new int[] { TargetPosX, TargetPosY };
        }

        internal void SetTarget(MapCorner corner)
        {
            int[] targetIndex = new int[2];
            switch (corner)
            {
                case MapCorner.BottomLeft:
                    targetIndex = GhostConstants.MapCorner_BottomLeftIndex;
                    break;
                case MapCorner.TopLeft:
                    targetIndex = GhostConstants.MapCorner_TopLeftIndex;
                    break;
                case MapCorner.TopRight:
                    targetIndex = GhostConstants.MapCorner_TopRightIndex;
                    break;
                case MapCorner.BottomRight:
                    targetIndex =  GhostConstants.MapCorner_BottomRightIndex;
                    break;
            }

            TargetPos = targetIndex;

            TargetPosX = targetIndex[0];
            TargetPosY = targetIndex[1];
        }

        internal void SetTarget(TeleportSide teleporter)
        {
            if (teleporter.Equals(TeleportSide.Left))
            {
                TargetPos = GameConstants.TeleporterLeftIndex;

                TargetPosX = GameConstants.TeleporterLeftIndex[0];
                TargetPosY = GameConstants.TeleporterLeftIndex[1];
            }
            else
            {
                TargetPos = GameConstants.TeleporterRightIndex;

                TargetPosX = GameConstants.TeleporterRightIndex[0];
                TargetPosY = GameConstants.TeleporterRightIndex[1];
            }
        }

        internal void SetPath(PossiblePaths path)
        {
            switch (path)
            {
                case PossiblePaths.Up:
                    SetDirection(Direction.Up);
                    break;
                case PossiblePaths.Down:
                    SetDirection(Direction.Down);
                    break;
                case PossiblePaths.Left:
                    SetDirection(Direction.Left);
                    break;
                case PossiblePaths.Right:
                    SetDirection(Direction.Right);
                    break;
            }
        }

        internal void SetPath(PossiblePaths path, TeleportSide teleportSide)
        {
            switch (path)
            {
                case PossiblePaths.Up:
                    SetDirection(Direction.Up);
                    break;
                case PossiblePaths.UpThroughPortal:
                    SetDirection(Direction.Up);
                    SetTarget(teleportSide);
                    break;

                case PossiblePaths.Down:
                    SetDirection(Direction.Down);
                    break;
                case PossiblePaths.DownThroughPortal:
                    SetDirection(Direction.Down);
                    SetTarget(teleportSide);
                    break;

                case PossiblePaths.Left:
                    SetDirection(Direction.Left);
                    break;
                case PossiblePaths.LeftThroughPortal:
                    SetDirection(Direction.Left);
                    SetTarget(teleportSide);
                    break;

                case PossiblePaths.Right:
                    SetDirection(Direction.Right);
                    break;
                case PossiblePaths.RightThroughPortal:
                    SetDirection(Direction.Right);
                    SetTarget(teleportSide);
                    break;
            }
        }

        /// <summary>
        /// Returns a bool indicating if the ghost's current behaviour can be overridden by a
        /// specified behaviour depending in the ghost's individual behaviour hierarchy.
        /// </summary>
        internal bool BehaviourCanBeChangedTo(GhostBehaviour newBehaviour)
        {
            // Return true if the ghost already follows the specified behaviour
            if (CurrentBehaviour == newBehaviour)
            {
                return true;
            }

            if (BehaviourOverridden)
            {
                // If the ghosts behaviour is overridden it's behaviour will change according to it's own behaviour hierarchy. 
                // If the new behaviour has a lower index (higher priority) than the current
                // behaviour in the hierarchy, it can override it
                if (BehaviourHierarchy.IndexOf(newBehaviour) < BehaviourHierarchy.IndexOf(CurrentBehaviour))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // If the ghost's behaviour is not overridden, it will change according 
                // to the default behaviour hierarchy
                if (GhostConstants.DefaultBehaviourHierarchy.IndexOf(newBehaviour) < GhostConstants.DefaultBehaviourHierarchy.IndexOf(CurrentBehaviour))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal void SetBehaviour(GhostBehaviour behaviour)
        {
            switch (behaviour)
            {
                case GhostBehaviour.Scatter:
                    SetScatter();
                    break;
                case GhostBehaviour.Chase:
                    SetChase();
                    break;
                case GhostBehaviour.Frightened:
                    SetFrightened();
                    break;
                case GhostBehaviour.ExitingHouse:
                    CurrentBehaviour = GhostBehaviour.ExitingHouse;
                    EnteringGhostHouse = false;
                    ExitingGhostHouse = true;

                    SetTarget(GhostConstants.OutOfHouseIndex);

                    SetDirection(Direction.Up);
                    break;
                case GhostBehaviour.Returning:
                    CurrentBehaviour = GhostBehaviour.Returning;
                    HasBeenEaten = true;

                    // The Return behaviour always overrides other behaviours 
                    BehaviourOverridden = true;
                    break;
            }
        }

        internal void SetScatter()
        {
            CurrentBehaviour = GhostBehaviour.Scatter;

            if (White)
            {
                White = false;
            }

            SetTarget(ScatterCorner);
        }

        internal void SetChase()
        {
            CurrentBehaviour = GhostBehaviour.Chase;

            if (White)
            {
                White = false;
            }
        }

        internal void SetFrightened()
        {
            CurrentBehaviour = GhostBehaviour.Frightened;
        }
    }
}
