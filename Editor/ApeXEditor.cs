using UnityEngine;
using UnityEditor;

using ApeX.Extentions;

namespace ApeX
{
    [CustomEditor(typeof(ApeXPlayer))]
    public class ApeXEditor : Editor
    {
        private ApeXPlayer targetLoco;

        private Texture2D logo;

        public override void OnInspectorGUI()
        {
            if (logo == null)
                logo = Resources.Load<Texture2D>("ApeX/Assets/ApeXRigBanner");
            GUILayout.Label(new GUIContent() { image = logo });

            base.OnInspectorGUI();

            GUILayout.Space(15);

            if (GUILayout.Button("Set Drives"))
            {
                targetLoco = (ApeXPlayer)target;

                if (targetLoco.leftHandJoint && targetLoco.rightHandJoint)
                {
                    targetLoco.SetDrives(targetLoco.leftHandJoint);
                    targetLoco.SetDrives(targetLoco.rightHandJoint);

                    Debug.Log("Successfully set drives!");
                }

                else
                    Debug.LogError($"The HandJoints on {targetLoco.name} are not set!");
            }

            if (GUILayout.Button("Setup Layers"))
            {
                ApeXExtentions.CreateLayer("Player", 20);
                ApeXExtentions.CreateLayer("Grab", 21);
            }
        }
    }
}
