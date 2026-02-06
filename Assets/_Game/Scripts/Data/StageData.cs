using System;
using UnityEngine;

[Serializable]
public class ChoiceData
{
    public Sprite sprite;
    public bool isWin;
}

[Serializable]
public class StageData
{
    public ChoiceData choiceA;
    public ChoiceData choiceB;
    public string stageTitle;
}
