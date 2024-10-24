using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "CustomPostProcessingMaterials", menuName = "Game/CustomPostProcessingMaterials")]
public class CustomPostProcessingMaterials : ScriptableObject
{
    // Material/s
    public Material customEffect;

    // Accessing the data from the Pass
    static CustomPostProcessingMaterials instance;

    public static CustomPostProcessingMaterials Instance
    {
        get
        {
            // TODO check if application is quitting
            // and avoid loading if that is the case
            if (instance == null)
            {
                instance = Resources.Load("CustomPostProcessingMaterials") as CustomPostProcessingMaterials;
            }

            return instance;
        }
    }
}
