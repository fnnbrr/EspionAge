using UnityEngine;
using UnityEditor;

public class ThrowableCheats : MonoBehaviour
{
    [MenuItem(Constants.CHEATS_ADD_MEDICINE_BOTTLES, true)]
    public static bool ValidateAddMedicineBottles()
    {
        return Application.isPlaying && UIManager.Instance;
    }

    [MenuItem(Constants.CHEATS_ADD_MEDICINE_BOTTLES)]
    public static void AddMedicineBottles()
    {
        GameObject medicineBottlePrefab = Resources.Load<GameObject>("Cheats/Medicine_Bottle_Throwable_CHEATS");
        
        for (int i = 0; i < 7; i++)
        {
            GameObject spawnedBottle = Instantiate(medicineBottlePrefab);
            spawnedBottle.transform.Find("Canvas").gameObject.SetActive(false);
            spawnedBottle.GetComponent<Throwable>().OnInteract();
        }
    }
}