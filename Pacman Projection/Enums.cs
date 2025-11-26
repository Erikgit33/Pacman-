using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ********** SOUND ENUMS ********** //

public enum Sounds
{
    menuMusic,
    buttonPress,
    pacman_beginning,
    pacman_chomp,
    pacman_eatFruit,
    pacman_eatGhost,
    pacman_death,
    pacman_win,
    ghost_scared,
    ghost_return,
    ghost_scatter,
    ghost_chase1,
    ghost_chase2,
    ghost_chase3
}

// ********** GHOST ENUMS ********** //

public enum GhostImageType
{
    None,
    Blinky,
    Pinky,
    Inky,
    Clyde,
    Sue,
    Funky,
    Spunky, 
    Whimsy
}

public enum GhostTemplate
{
    None,
    Blinky,
    Pinky,
    Inky,
    Clyde,
    TestGhost,
}

public enum GhostChaseType
{
    None,
    Chase,
    Ambush,
    Flank,
    Fallback,
    Random
}

public enum GhostBehaviour
{
    Scatter,
    Chase,
    Frightened,
    Returning,
    ExitingHouse
}   

public enum PossiblePaths
{
    Up,
    Down,
    Left,
    Right,
    UpThroughPortal,
    DownThroughPortal,
    LeftThroughPortal,
    RightThroughPortal
}

public enum ImageType
{
    Stationary,
    Stationary2,
    Stationary_Eyes,
    Up,
    Up2,
    Up_Eyes,
    Down,
    Down2,
    Down_Eyes,
    Left,
    Left2,
    Left_Eyes,
    Right,
    Right2,
    Right_Eyes,
    FrightenedBlue,
    FrightenedBlue2,
    FrightenedWhite,
    FrightenedWhite2
}

// ********** ENTITY ENUMS ********** //

public enum EntityBox
{
    LowerLeft,
    UpperLeft,
    UpperRight,
    LowerRight,
}

public enum EntityState
{
    Standard,
    Teleporting,
    Eaten
}

// ********** GENERAL ENUMS ********** //

public enum Direction
{
    None,
    Stationary,
    Up,
    Down,
    Left,
    Right
}

public enum Key
{
    None,
    Up = 38, // Up arrow key
    Down = 40, // Down arrow key
    Left = 37, // Left arrow key
    Right = 39, // Right arrow key
    Escape = 27 // Escape key
}

public enum MapCorner
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public enum TeleportSide
{
    None,
    Left,
    Right
}

public enum Fruit
{
    Cherry,
    Strawberry,
    Apple,
    Banana,
    Melon
}

namespace Pacman_Projection { public class Enums { } }
