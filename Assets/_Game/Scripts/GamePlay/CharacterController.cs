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

    public event Action<VFXMoment> OnMoment;

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
        OnMoment?.Invoke(VFXMoment.IntroStart);
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
    public void PlayWinThenIdle(Action onDone)
    {
        PlayResultNoReturn(winAnim, true, onDone);
    }

    public void PlayLoseThenIdle(Action onDone)
    {
        PlayResultNoReturn(loseAnim, false, onDone);
    }

    public void PlayIntroThenIdle(string intro, string idle, Action onIntroDone)
    {
        if (!Ready()) { onIntroDone?.Invoke(); return; }

        bool hasIntro = HasAnim(intro);
        bool hasIdle  = HasAnim(idle);

        skeletonAnim.AnimationState.ClearTrack(baseTrack);
        ResetToSetupPoseNow();

        if (!hasIntro)
        {
            if (hasIdle) skeletonAnim.AnimationState.SetAnimation(baseTrack, idle, true);
            onIntroDone?.Invoke();
            return;
        }

        OnMoment?.Invoke(VFXMoment.IntroStart);

        var entry = skeletonAnim.AnimationState.SetAnimation(baseTrack, intro, false);

        bool fired = false;
        entry.Complete += _ =>
        {
            if (fired) return;
            fired = true;

            OnMoment?.Invoke(VFXMoment.IntroComplete);

            if (hasIdle)
                skeletonAnim.AnimationState.SetAnimation(baseTrack, idle, true);
            else
                skeletonAnim.AnimationState.SetAnimation(baseTrack, intro, true);

            onIntroDone?.Invoke();
        };

    }

    public void PlayResultNoReturn(string resultAnim, bool isWin, Action onDone)
    {
        if (!Ready()) { onDone?.Invoke(); return; }

        bool hasResult = HasAnim(resultAnim);

        skeletonAnim.AnimationState.ClearTrack(baseTrack);
        ResetToSetupPoseNow();

        if (!hasResult)
        {
            onDone?.Invoke();
            return;
        }

        OnMoment?.Invoke(isWin ? VFXMoment.WinStart : VFXMoment.LoseStart);

        var entry = skeletonAnim.AnimationState.SetAnimation(baseTrack, resultAnim, false);

        bool fired = false;
        entry.Complete += _ =>
        {
            if (fired) return;
            fired = true;

            OnMoment?.Invoke(isWin ? VFXMoment.WinComplete : VFXMoment.LoseComplete);
            onDone?.Invoke();
        };
    }



}
