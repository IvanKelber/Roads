using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Levels/Manager")]
public class LevelManager : ScriptableObject
{
    public Level[] levels;

    int currentLevel = 0;

    public void OnEnable() {
        currentLevel = 0;
    }

    public Level CurrentLevel {
        get {
            return levels[currentLevel];
        }
    }

    public void NextLevel() {
        currentLevel++;
    }

    public void PreviousLevel() {
        currentLevel--;
    }

}
