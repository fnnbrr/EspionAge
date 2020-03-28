using UnityEngine;
using UnityEditor;

public class TeleportCheats : MonoBehaviour
{
    private static bool ValidateTeleport()
    {
        return Application.isPlaying && GameManager.Instance.GetPlayerTransform();
    }
    
    [MenuItem(Constants.CHEATS_ENABLE_3D_TELEPORT, true)]
    public static bool ValidateEnable3DTeleport()
    {
        return ValidateTeleport() && !GameManager.Instance.gameObject.GetComponent<CheatManager>().enabled;
    }

    [MenuItem(Constants.CHEATS_ENABLE_3D_TELEPORT)]
    public static void Enable3DTeleport()
    {
        GameManager.Instance.gameObject.GetComponent<CheatManager>().enabled = true;
    }

    [MenuItem(Constants.CHEATS_TELEPORT_NURSESROOM, true)]
    public static bool ValidateTeleportNursesRoom()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_NURSESROOM)]
    public static void TeleportNursesRoom()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-161f, 3f, 224.5f);
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_DININGHALL, true)]
    public static bool ValidateTeleportDiningHall()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_DININGHALL)]
    public static void TeleportDiningHall()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-66.8f, 3f, 0.9f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_DENTURES, true)]
    public static bool ValidateTeleportDentures()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_DENTURES)]
    public static void TeleportDentures()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(27f, 3f, 4f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_KITCHENSTART, true)]
    public static bool ValidateTeleportKitchenStart()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_KITCHENSTART)]
    public static void TeleportKitchenStart()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-42f, 3f, 23f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_PIANO, true)]
    public static bool ValidateTeleportPiano()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_PIANO)]
    public static void TeleportPiano()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-69.08f, 3f, -37.25f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_HALLWAY, true)]
    public static bool ValidateTeleportHallway()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_HALLWAY)]
    public static void TeleportHallway()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-124f, 3f, 235f);
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_NPC1, true)]
    public static bool ValidateTeleportNPC1()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_NPC1)]
    public static void TeleportNPC1()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-60f, 3f, 32f);
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(0f, 180f, 0f);
    }
}
