using System;
using Spine;
using Spine.Unity;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterController : MonoBehaviour
{
    [Header("Spine")]
    [SerializeField] private SkeletonGraphic skeletonAnim;

    [Header("Anim Names")]
    [SerializeField] private string introAnim = "intro";
    [SerializeField] private string idleAnim  = "idle";
    [SerializeField] private string winAnim   = "win";
    [SerializeField] private string loseAnim  = "lose";

    [SerializeField] private int baseTrack = 0;

    void Awake()
    {
        if (!skeletonAnim) skeletonAnim = GetComponentInChildren<SkeletonGraphic>(true);
        EnsureInit();
    }

    void OnEnable()
    {
        EnsureInit();
    }

    void EnsureInit()
    {
        if (!skeletonAnim) return;
        if (!skeletonAnim.IsValid)
            skeletonAnim.Initialize(true);
    }

    bool Ready()
    {
        if (!skeletonAnim)
        {
            Debug.LogError("[CharacterController] Missing SkeletonGraphic.");
            return false;
        }

        EnsureInit();

        if (skeletonAnim.AnimationState == null || skeletonAnim.SkeletonDataAsset == null || skeletonAnim.Skeleton == null)
        {
            Debug.LogError("[CharacterController] SkeletonGraphic not initialized yet.");
            return false;
        }
        return true;
    }

    bool HasAnim(string animName)
    {
        if (!Ready()) return false;
        if (string.IsNullOrEmpty(animName)) return false;

        var data = skeletonAnim.SkeletonDataAsset.GetSkeletonData(true);
        return data != null && data.FindAnimation(animName) != null;
    }

    void ResetToSetupPoseNow()
    {
        if (!Ready()) return;

        skeletonAnim.Skeleton.SetToSetupPose();

        skeletonAnim.AnimationState.Apply(skeletonAnim.Skeleton);

        skeletonAnim.Update(0f);
    }

    // ===================== INTRO -> IDLE (hoặc loop intro) =====================

    public void PlayIntroThenIdle(Action onIntroDone)
    {
        if (!Ready()) return;

        bool hasIntro = HasAnim(introAnim);
        bool hasIdle  = HasAnim(idleAnim);

        skeletonAnim.AnimationState.ClearTrack(baseTrack);
        ResetToSetupPoseNow();

        if (!hasIntro)
        {
            Debug.LogWarning($"[CharacterController] Missing intro '{introAnim}'.");
            if (hasIdle) skeletonAnim.AnimationState.SetAnimation(baseTrack, idleAnim, true);
            onIntroDone?.Invoke();
            return;
        }

        var entry = skeletonAnim.AnimationState.SetAnimation(baseTrack, introAnim, false);

        bool fired = false;
        entry.Complete += _ =>
        {
            if (fired) return;
            fired = true;

            if (hasIdle) skeletonAnim.AnimationState.SetAnimation(baseTrack, idleAnim, true);
            else skeletonAnim.AnimationState.SetAnimation(baseTrack, introAnim, true);

            onIntroDone?.Invoke();
        };
    }

    // ===================== WIN / LOSE =====================

    public void PlayWinThenIdle(Action onDone)  => PlayResultNoReturn(winAnim, onDone);
    public void PlayLoseThenIdle(Action onDone) => PlayResultNoReturn(loseAnim, onDone);

    void PlayResultNoReturn(string resultAnim, Action onDone)
    {
        if (!Ready()) return;

        bool hasResult = HasAnim(resultAnim);

        skeletonAnim.AnimationState.ClearTrack(baseTrack);
        ResetToSetupPoseNow();

        if (!hasResult)
        {
            Debug.LogWarning($"[CharacterController] Missing result anim '{resultAnim}'.");
            onDone?.Invoke();
            return;
        }

        var entry = skeletonAnim.AnimationState.SetAnimation(baseTrack, resultAnim, false);

        bool fired = false;
        entry.Complete += _ =>
        {
            if (fired) return;
            fired = true;
            onDone?.Invoke();
        };
    }
}
