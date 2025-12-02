using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorChangesHistory
{
    [Serializable]
    public class SerializableChangeEvent
    {
        public string type;
        public string timestamp;
        public string data;
    }

    [Serializable]
    public class ChangesList
    {
        public List<SerializableChangeEvent> changes;
    }

    public class DiagramChangeTracker : MonoBehaviour
    {
        private static DiagramChangeTracker _instance;
        public static DiagramChangeTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DiagramChangeTracker");
                    _instance = go.AddComponent<DiagramChangeTracker>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private List<DiagramChangeEvent> _changes = new List<DiagramChangeEvent>();

        public void TrackChange(DiagramChangeEvent changeEvent)
        {
            _changes.Add(changeEvent);
            Debug.Log(SerializeChanges());
        }

        public List<DiagramChangeEvent> GetChanges()
        {
            return new List<DiagramChangeEvent>(_changes);
        }

        public void ClearChanges()
        {
            _changes.Clear();
        }

        public string SerializeChanges()
        {
            var serializableChanges = new List<SerializableChangeEvent>();
            
            foreach (var change in _changes)
            {
                serializableChanges.Add(new SerializableChangeEvent
                {
                    type = change.Type.ToString(),
                    timestamp = change.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                    data = change.Data
                });
            }

            var changesList = new ChangesList { changes = serializableChanges };
            return JsonUtility.ToJson(changesList, true);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
