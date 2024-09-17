using System.Collections.Generic;
[System.Serializable]
public class SceneSave
{
    public Dictionary<string, bool> boolDictionary;

    public List<SceneItem> listSceneItem;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
}
