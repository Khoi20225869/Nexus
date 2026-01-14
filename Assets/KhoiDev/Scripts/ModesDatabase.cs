using System;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Modes Database", menuName = "Data/Mode System/Modes Database")]
    public class ModesDatabase : ScriptableObject
    {
        [SerializeField] ModeData[] mode;

        [Serializable]
        public class ModeData
        {
            [SerializeField] int id;
            public int Id => id;
            [SerializeField] LevelsDatabase levelsDatabase;
            public LevelsDatabase LevelsDatabase => levelsDatabase;
        }

        public ModeData GetModeData(int id)
        {
            for(int i = 0; i < mode.Length; i++)
            {
                ModeData data = mode[i];

                if (mode[i].Id == id)
                {
                    return data;
                }
            }

            return null;
        }
    }
}
