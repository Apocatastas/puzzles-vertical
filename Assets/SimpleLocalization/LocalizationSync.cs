using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SimpleLocalization
{

    [ExecuteInEditMode]
    public class LocalizationSync : MonoBehaviour
    {

        public string TableId;


        public Sheet[] Sheets;


        public UnityEngine.Object SaveFolder;

        private const string UrlPattern = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";

#if UNITY_EDITOR


        public void Sync()
        {
            StopAllCoroutines();
            StartCoroutine(SyncCoroutine());
        }

        private IEnumerator SyncCoroutine()
        {
            var folder = UnityEditor.AssetDatabase.GetAssetPath(SaveFolder);


            return null;

        }
#endif
    }
}