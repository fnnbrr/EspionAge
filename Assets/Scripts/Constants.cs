using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const string TAG_PLAYER = "Player";

    public const string LAYER_PLAYER = "PLAYER";

    public const string ANIMATION_PLAYER_ISRESTING = "IsResting";

    public const string ANIMATION_INTERACTABLE_POPIN = "PopIn";
    public const string ANIMATION_INTERACTABLE_POPDOWN = "PopDown";

    public const string INPUT_INTERACTABLE_GETDOWN = "Interact";
    public const string INPUT_CANCEL_GETDOWN = "Cancel";
    public const string INPUT_THROW_GETDOWN = "Fire1";
    public const string INPUT_STARTBUTTON_GETDOWN = "Start";

    public const string ASSET_PATH_MISSIONTUTORIAL = "Assets/Prefabs/Missions/MissionTutorial.prefab";
    public const string ASSET_PATH_MISSIONCAFETERIA1 = "Assets/Prefabs/Missions/MissionCafeteria1.prefab";

    public const string CHEATS_STARTMISSIONTUTORIAL= "Cheats/00-MissionTutorial/Start";
    public const string CHEATS_ENDMISSIONTUTORIAL = "Cheats/00-MissionTutorial/End";
    public const string CHEATS_STARTMISSIONCAFETERIA1 = "Cheats/01-MissionCafeteria1/Start";
    public const string CHEATS_ENDMISSIONCAFETERIA1 = "Cheats/01-MissionCafeteria1/End";

    public const string CHEATS_CAMERA_BLENDTOFAR = "Cheats/Camera/BlendToFarCamera";

    public const string CHEATS_TELEPORT_BIRDIESROOM = "Cheats/Teleport/Birdies Room";
    public const string CHEATS_TELEPORT_DININGHALL = "Cheats/Teleport/Dining Hall";
    public const string CHEATS_TELEPORT_DENTURES = "Cheats/Teleport/Dentures";
    public const string CHEATS_TELEPORT_PANTRY = "Cheats/Teleport/Pantry";
    public const string CHEATS_TELEPORT_KITCHENSTART = "Cheats/Teleport/Kitchen Start";
    public const string CHEATS_TELEPORT_PIANO = "Cheats/Teleport/Piano";

    public const string SCENE_MAINMENU = "main_menu";
    public const string SCENE_MAIN = "MAIN";

    public const float INTERACT_BOUNDARY_RADIUS = 20.0f;
    public const float INTERACT_POPUP_RADIUS = 10.0f;
    public const float WAIT_TIME_CONVO_LINE = 2.0f;
}
