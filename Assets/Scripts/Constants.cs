using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const string TAG_NONE = "Untagged";
    public const string TAG_PLAYER = "Player";
    public const string TAG_ENEMY = "Enemy";

    public const string LAYER_PLAYER = "PLAYER";
    
    public const string ANIMATION_BIRDIE_ISWALKING = "IsWalking";
    public const string ANIMATION_BIRDIE_AWAKENESS = "Awakeness";
    public const string ANIMATION_BIRDIE_DASH= "Dash";
    public const string ANIMATION_BIRDIE_WAKEUP = "WakeUp";

    public const string ANIMATION_STEVE_MOVING = "IsMoving";

    public const string ANIMATION_INTERACTABLE_POPIN = "PopIn";
    public const string ANIMATION_INTERACTABLE_POPDOWN = "PopDown";

    public const string ANIMATION_OBJECTIVELIST_SLIDE = "slideOut";
    
    public const string ANIMATION_MAINMENU_LIGHTBULB_FLICKER = "Flicker";
    public const string ANIMATION_MAINMENU_LIGHTBULB_RED = "Red";

    public const string INPUT_INTERACTABLE_GETDOWN = "Interact";
    public const string INPUT_CANCEL_GETDOWN = "Cancel";
    public const string INPUT_THROW_GETDOWN = "Fire1";
    public const string INPUT_STARTBUTTON_GETDOWN = "Start";
    public const string INPUT_SPECIAL_GETDOWN = "Special";
    public const string INPUT_AXIS_HORIZONTAL = "Horizontal";
    public const string INPUT_AXIS_VERTICAL = "Vertical";
    public const string INPUT_AXIS_HORIZONTAL_RIGHT_STICK = "Horizontal Right Stick";
    public const string INPUT_AXIS_VERTICAL_RIGHT_STICK = "Vertical Right Stick";
    public const string INPUT_AXIS_HORIZONTAL_DPAD = "DPAD_Horizontal";

    public const string CHEATS_STARTMISSIONTUTORIAL= "Cheats/00-MissionTutorial/Start";
    public const string CHEATS_ENDMISSIONTUTORIAL = "Cheats/00-MissionTutorial/End";
    public const string CHEATS_STARTMISSIONKITCHEN1 = "Cheats/01-MissionKitchen1/Start";
    public const string CHEATS_ENDMISSIONKITCHEN1 = "Cheats/01-MissionKitchen1/End";

    public const string CHEATS_CAMERA_BLENDTOFAR = "Cheats/Camera/BlendToFarCamera";

    public const string CHEATS_TELEPORT_NURSESROOM = "Cheats/Teleport/Nurse's Room";
    public const string CHEATS_TELEPORT_DININGHALL = "Cheats/Teleport/Dining Hall";
    public const string CHEATS_TELEPORT_DENTURES = "Cheats/Teleport/Dentures";
    public const string CHEATS_TELEPORT_KITCHENSTART = "Cheats/Teleport/Kitchen Start";
    public const string CHEATS_TELEPORT_PIANO = "Cheats/Teleport/Piano";
    public const string CHEATS_TELEPORT_HALLWAY = "Cheats/Teleport/Hallway";
    public const string CHEATS_TELEPORT_NPC1 = "Cheats/Teleport/NPC1";

    public const string CHEATS_AWAKENESS_DEFAULT = "Cheats/Awakeness/Default";
    public const string CHEATS_AWAKENESS_ALWAYSMIN = "Cheats/Awakeness/Always Min";
    public const string CHEATS_AWAKENESS_ALWAYSMAX = "Cheats/Awakeness/Always Max";

    public const string SCENE_MAINMENU = "main_menu";
    public const string SCENE_MAIN = "MAIN";

    public const float INTERACT_BOUNDARY_RADIUS = 20.0f;
    public const float INTERACT_POPUP_RADIUS = 10.0f;
    public const float CHAR_TYPE_SPEED = 0.05f;
    public const float WAIT_TIME_CONVO_LINE = 1.0f;

    public const int RIGHT_PADDING_WITH_ABUTTON = 70;
    public const int RIGHT_PADDING_WITHOUT_ABUTTON = 40;

    public const string SHADER_NAME_SHAKE = "PlateShake";
}
