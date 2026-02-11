using System;
using System.Collections.Generic;
using UnityEngine;

public enum VFXSpawnSpace { UI, World }
public enum VFXAnchor { FixedPosition, FollowTransform }

[CreateAssetMenu(menuName = "VFX/Stage VFX Config")]
public class StageVFXConfig : ScriptableObject
{
    [Serializable]
    public class Rule
    {
        public VFXMoment moment;

        [Header("Particle")]
        public ParticleSystem prefab;
        public float delay; // realtime seconds

        [Header("Spawn")]
        public VFXSpawnSpace space = VFXSpawnSpace.UI;

        // UI fixed pos
        public Vector2 uiAnchoredPos;

        // World fixed pos
        public Vector3 worldPos;

        [Header("Optional Follow")]
        public VFXAnchor anchor = VFXAnchor.FixedPosition;
        public string followTransformName;
    }

    public List<Rule> rules = new();
}
