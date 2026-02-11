using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageVFXPlayer : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private StageVFXConfig config;

    [Header("Roots")]
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private Transform worldRoot;

    CharacterController character;

    readonly List<Coroutine> _running = new();
    readonly List<ParticleSystem> _spawned = new();

    int _runId = 0;

    public void Bind(CharacterController c, StageVFXConfig cfg, RectTransform uiRootOverride = null, Transform worldRootOverride = null)
    {
        Unbind();
        ClearAll();

        _runId++; 

        character = c;
        config = cfg;

        if (uiRootOverride) uiRoot = uiRootOverride;
        if (worldRootOverride) worldRoot = worldRootOverride;

        if (character != null)
            character.OnMoment += OnMoment;
    }

    public void Unbind()
    {
        if (character != null)
            character.OnMoment -= OnMoment;
        character = null;
    }

    void OnDisable()
    {
        Unbind();
        ClearAll();
    }

    void OnDestroy()
    {
        Unbind();
        ClearAll();
    }

    public void ClearAll()
    {
        // stop delay coroutines
        for (int i = 0; i < _running.Count; i++)
        {
            if (_running[i] != null) StopCoroutine(_running[i]);
        }
        _running.Clear();

        // destroy spawned particles
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null) Destroy(_spawned[i].gameObject);
        }
        _spawned.Clear();
    }

    void OnMoment(VFXMoment moment)
    {
        if (config == null || config.rules == null) return;

        int myRun = _runId; 

        for (int i = 0; i < config.rules.Count; i++)
        {
            var r = config.rules[i];
            if (r.prefab == null) continue;
            if (r.moment != moment) continue;

            var co = StartCoroutine(PlayRuleCR(r, myRun));
            _running.Add(co);
        }
    }

    IEnumerator PlayRuleCR(StageVFXConfig.Rule r, int runToken)
    {
        if (r.delay > 0f)
            yield return new WaitForSecondsRealtime(r.delay);

        if (runToken != _runId) yield break;

        // spawn
        if (r.space == VFXSpawnSpace.UI)
        {
            if (!uiRoot) yield break;

            var ps = Instantiate(r.prefab, uiRoot);
            _spawned.Add(ps);

            var rt = ps.GetComponent<RectTransform>();
            if (rt) rt.anchoredPosition = r.uiAnchoredPos;

            ps.Play();
        }
        else
        {
            Vector3 pos = r.worldPos;
            var parent = worldRoot != null ? worldRoot : null;

            var ps = Instantiate(r.prefab, pos, Quaternion.identity, parent);
            _spawned.Add(ps);

            ps.Play();
        }
    }
}
