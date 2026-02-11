using UnityEngine;

public enum BranchRoute
{
    None = 0,
    Route1 = 1,
    Route2 = 2
}

public class StageContext
{
    public BranchRoute route = BranchRoute.None;
    public int lastChosenIndex = -1; 
}
