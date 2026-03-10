using System.IO;
using UnityEngine;

namespace Game.Infrastructure.Save
{
    public sealed class JsonProgressSaveService : IProgressSaveService
    {
        private readonly string _path;

        public JsonProgressSaveService(string fileName)
        {
            var safeFileName = string.IsNullOrWhiteSpace(fileName) ? "player_progress.json" : fileName;
            _path = Path.Combine(Application.persistentDataPath, safeFileName);
        }

        public void Save(PlayerProgressData data)
        {
            if (data == null)
            {
                return;
            }

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_path, json);
        }

        public PlayerProgressData Load()
        {
            if (!File.Exists(_path))
            {
                return null;
            }

            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<PlayerProgressData>(json);
        }
    }
}
