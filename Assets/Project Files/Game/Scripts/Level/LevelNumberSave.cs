using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelNumberSave : ISaveObject
    {
        public int modeNumber;
        public int ModeNumber => modeNumber;
        public int levelModeNumber;
        public int LevelModeNumber => levelModeNumber;

        public int levelNumber;
        public int LevelNumber => levelNumber;

        public void IncrementModeNumer()
        {
            modeNumber++;
            levelModeNumber = 0;
        }

        public void IncrementLevelNumber()
        {
            levelNumber++;
            levelModeNumber++;
        }

        public void SetLevelNumber(int levelNumber)
        {
            this.levelNumber = levelNumber;
        }

        public void Flush()
        {
            
        }
    }
}
