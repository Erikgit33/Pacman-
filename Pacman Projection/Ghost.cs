using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
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

        /// <summary>
        /// The set of images the ghost uses for its various states and directions.
        /// </summary>
        public Dictionary<ImageType, Image> Images { get; private set; }

        /// <summary>
        /// The template the ghost was created according to.
        /// A ghost's template determines its starting position, starting direction, ScatterCorner, images, and behaviour hierarchy.
        /// </summary>
        public GhostTemplate Template { get; private set; }

        /// <summary>
        /// The type of chase behaviour the ghost uses while in Chase mode, determining how the ghost calculates its target while chasing Pacman.
        /// ChaseType can be changed independantly of the ghost's Template.
        /// </summary>
        public GhostChaseType GhostChaseType { get; private set; }

        /// <summary>
        /// Dictates the specific type of ghost, i.e. which set of images the ghost should use. 
        /// </summary>
        public GhostImageType GhostImageType { get; private set; }

        /// <summary>
        /// Contains the behaviour hierarchy for the ghost.
        /// The lower the index, the higher the priority the behaviour has.
        /// A ghost only follows its own specified behaviour hierarchy if its behaviour is overridden - if the ghost's behaviour is overridden
        /// and the global behaviour attempts to change it, the ghost will check if the new (global) behaviour can override its current behaviour 
        /// based on the hierarchy specified here.
        /// </summary>
        private List<GhostBehaviour> BehaviourHierarchy;

        /// <summary>
        /// Used for target calculation ghosts with the chaseType Flank.
        /// </summary>
        internal Ghost CorrespondingChaseTypeGhost { get; set; }

        public Ghost(GhostTemplate ghostTemplate)
        {
            box.LocationChanged += UpdateLocation;

            switch (ghostTemplate)
            {
                case GhostTemplate.Blinky:
                    Template = GhostTemplate.Blinky;
                    GhostChaseType = GhostChaseType.Chase;
                    GhostImageType = GhostImageType.Blinky;

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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };

                    BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
                    break;
                case GhostTemplate.Pinky:
                    Template = GhostTemplate.Pinky;
                    GhostChaseType = GhostChaseType.Ambush;
                    GhostImageType = GhostImageType.Pinky;

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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };

                    BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
                    break;
                case GhostTemplate.Inky:
                    Template = GhostTemplate.Inky;
                    GhostChaseType = GhostChaseType.Flank;
                    GhostImageType = GhostImageType.Inky;

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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };

                    BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
                    break;
                case GhostTemplate.Clyde:
                    Template = GhostTemplate.Clyde;
                    GhostChaseType = GhostChaseType.Fallback;
                    GhostImageType = GhostImageType.Clyde;

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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    
                    // Clyde's behaviour is overridden if within a certain distance of pacman (Fallback), Scatter thus
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
            }

            // box.Size is the same for all ghosts
            box.Size = new Size(GameConstants.EntitySize, GameConstants.EntitySize);
            navBox.Size = new Size(GameConstants.BoxSize, GameConstants.BoxSize);
        }

        public Ghost(GhostImageType selectedImageType, GhostChaseType selectedChaseType, MapCorner selectedScatterCorner, int[] selectedStartingPosition)
        {
            box.LocationChanged += UpdateLocation;

            GhostChaseType = selectedChaseType;
            GhostImageType = selectedImageType;

            ScatterCorner = selectedScatterCorner;

            StartX = selectedStartingPosition[0];
            StartY = selectedStartingPosition[1];

            if (GetStartsInGhostHouse(StartX, StartY))
            {
                StartsInGhostHouse = true;
                ExitingGhostHouse = true;
            }

            switch (GhostImageType)
            {   
                case GhostImageType.Blinky:
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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Pinky:
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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Inky:
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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Clyde:
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
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Sue:
                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Sue.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Sue.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Sue.up },
                        { ImageType.Up2, GhostConstants.Images.Sue.up2 },
                        { ImageType.Down, GhostConstants.Images.Sue.down },
                        { ImageType.Down2, GhostConstants.Images.Sue.down2 },
                        { ImageType.Left, GhostConstants.Images.Sue.left },
                        { ImageType.Left2, GhostConstants.Images.Sue.left2 },
                        { ImageType.Right, GhostConstants.Images.Sue.right },
                        { ImageType.Right2, GhostConstants.Images.Sue.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Funky:
                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Funky.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Funky.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Funky.up },
                        { ImageType.Up2, GhostConstants.Images.Funky.up2 },
                        { ImageType.Down, GhostConstants.Images.Funky.down },
                        { ImageType.Down2, GhostConstants.Images.Funky.down2 },
                        { ImageType.Left, GhostConstants.Images.Funky.left },
                        { ImageType.Left2, GhostConstants.Images.Funky.left2 },
                        { ImageType.Right, GhostConstants.Images.Funky.right },
                        { ImageType.Right2, GhostConstants.Images.Funky.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Spunky:
                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Spunky.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Spunky.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Spunky.up },
                        { ImageType.Up2, GhostConstants.Images.Spunky.up2 },
                        { ImageType.Down, GhostConstants.Images.Spunky.down },
                        { ImageType.Down2, GhostConstants.Images.Spunky.down2 },
                        { ImageType.Left, GhostConstants.Images.Spunky.left },
                        { ImageType.Left2, GhostConstants.Images.Spunky.left2 },
                        { ImageType.Right, GhostConstants.Images.Spunky.right },
                        { ImageType.Right2, GhostConstants.Images.Spunky.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
                case GhostImageType.Whimsy:
                    Images = new Dictionary<ImageType, Image>
                    {
                        { ImageType.Stationary, GhostConstants.Images.Whimsy.stationary },
                        { ImageType.Stationary2, GhostConstants.Images.Whimsy.stationary2 },
                        { ImageType.Up, GhostConstants.Images.Whimsy.up },
                        { ImageType.Up2, GhostConstants.Images.Whimsy.up2 },
                        { ImageType.Down, GhostConstants.Images.Whimsy.down },
                        { ImageType.Down2, GhostConstants.Images.Whimsy.down2 },
                        { ImageType.Left, GhostConstants.Images.Whimsy.left },
                        { ImageType.Left2, GhostConstants.Images.Whimsy.left2 },
                        { ImageType.Right, GhostConstants.Images.Whimsy.right },
                        { ImageType.Right2, GhostConstants.Images.Whimsy.right2 },

                        { ImageType.FrightenedBlue, GhostConstants.Images.frightenedBlue },
                        { ImageType.FrightenedBlue2, GhostConstants.Images.frightenedBlue2 },
                        { ImageType.FrightenedWhite, GhostConstants.Images.frightenedWhite },
                        { ImageType.FrightenedWhite2, GhostConstants.Images.frightenedWhite2 },

                        { ImageType.Stationary_Eyes, GhostConstants.Images.eyesStationary },
                        { ImageType.Up_Eyes, GhostConstants.Images.eyesUp },
                        { ImageType.Down_Eyes, GhostConstants.Images.eyesDown },
                        { ImageType.Left_Eyes, GhostConstants.Images.eyesLeft },
                        { ImageType.Right_Eyes, GhostConstants.Images.eyesRight }
                    };
                    break;
            }

            if (GhostChaseType.Equals(GhostChaseType.Fallback))
            {
                // Fallback behaviour hierarchy
                BehaviourHierarchy = new List<GhostBehaviour>
                {
                    GhostBehaviour.ExitingHouse,
                    GhostBehaviour.Returning,
                    GhostBehaviour.Frightened,
                    GhostBehaviour.Scatter,
                    GhostBehaviour.Chase
                };
            }
            else
            {
                BehaviourHierarchy = GhostConstants.DefaultBehaviourHierarchy;
            }

            // box.Size is the same for all ghosts
            box.Size = new Size(GameConstants.EntitySize, GameConstants.EntitySize);
            navBox.Size = new Size(GameConstants.BoxSize, GameConstants.BoxSize);
        }

        private bool GetStartsInGhostHouse(int startX, int startY)
        {
            int startXIndex = startX / GameConstants.BoxSize;
            int startYIndex = startY / GameConstants.BoxSize;

            if (startXIndex >= GameConstants.GhostHouse_TopLeftIndex[0] && startXIndex <= GameConstants.GhostHouse_BottomRightIndex[0]
             && startYIndex >= GameConstants.GhostHouse_TopLeftIndex[1] && startYIndex <= GameConstants.GhostHouse_BottomRightIndex[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void SetStartDirectionAndImage(List<Direction> possibleDirections)
        {
            int random = new Random().Next(0, possibleDirections.Count);
            StartDirection = possibleDirections[random];
            switch (StartDirection)
            {
                case Direction.Up:
                    StartImage = Images[ImageType.Up];
                    break;
                case Direction.Down:
                    StartImage = Images[ImageType.Down];
                    break;
                case Direction.Left:
                    StartImage = Images[ImageType.Left];
                    break;
                case Direction.Right:
                    StartImage = Images[ImageType.Right];
                    break;
            }
        }

        private void UpdateLocation(object sender, EventArgs e)
        {
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
