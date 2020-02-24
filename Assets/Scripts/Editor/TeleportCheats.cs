using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TeleportCheats : MonoBehaviour
{
    private static bool ValidateTeleport()
    {
        return Application.isPlaying && GameManager.Instance.GetPlayerTransform();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_BIRDIESROOM, true)]
    public static bool ValidateTeleportBirdiesRoom()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_BIRDIESROOM)]
    public static void TeleportBirdiesRoom()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-98.12f, 3f, 136.48f);
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
        GameManager.Instance.GetPlayerTransform().position = new Vector3(33.91f, 3f, 0.22f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_PANTRY, true)]
    public static bool ValidateTeleportPantry()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_PANTRY)]
    public static void TeleportPantry()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(32f, 3f, 39.6f);
    }

    [MenuItem(Constants.CHEATS_TELEPORT_KITCHENSTART, true)]
    public static bool ValidateTeleportKitchenStart()
    {
        return ValidateTeleport();
    }

    [MenuItem(Constants.CHEATS_TELEPORT_KITCHENSTART)]
    public static void TeleportKitchenStart()
    {
        GameManager.Instance.GetPlayerTransform().position = new Vector3(-42.8f, 3f, 36.76f);
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
}
