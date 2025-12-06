using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Unity.Cinemachine;

public static class TwiningShortcutUtils
{
	/// <summary>Tweens a Cinemachine Lens <code>FieldOfView</code> to the given value.
	/// Also stores the CinemachineVirtualCamera as the tween's target so it can be used for filtered operations</summary>
	/// <param name="endValue">The end value to reach</param>
	/// <param name="duration">The duration of the tween</param>
	public static TweenerCore<float, float, FloatOptions> DOFieldOfView(
		this CinemachineVirtualCamera target,
		float endValue,
		float duration)
	{
		TweenerCore<float, float, FloatOptions> t = DOTween.To((DOGetter<float>)(() => target.m_Lens.FieldOfView), (DOSetter<float>)(x => target.m_Lens.FieldOfView = x), endValue, duration);
		t.SetTarget<TweenerCore<float, float, FloatOptions>>((object)target);
		return t;
	}
	
	public static TweenerCore<float, float, FloatOptions> DOFieldOfView(
		this CinemachineCamera target,
		float endValue,
		float duration)
	{
		TweenerCore<float, float, FloatOptions> t = DOTween.To((DOGetter<float>)(() => target.Lens.FieldOfView), (DOSetter<float>)(x => target.Lens.FieldOfView = x), endValue, duration);
		t.SetTarget<TweenerCore<float, float, FloatOptions>>((object)target);
		return t;
	}

	/// <summary>Tweens a Cinemachine Lens <code>FieldOfView</code> to the given value.
	/// Also stores the CinemachineFreeLook as the tween's target so it can be used for filtered operations</summary>
	/// <param name="endValue">The end value to reach</param>
	/// <param name="duration">The duration of the tween</param>
	public static TweenerCore<float, float, FloatOptions> DOFieldOfView(
		this CinemachineFreeLook target,
		float endValue,
		float duration)
	{
		TweenerCore<float, float, FloatOptions> t = DOTween.To((DOGetter<float>)(() => target.m_Lens.FieldOfView), (DOSetter<float>)(x => target.m_Lens.FieldOfView = x), endValue, duration);
		t.SetTarget<TweenerCore<float, float, FloatOptions>>((object)target);
		return t;
	}

	/// <summary>Tweens a Cinemachine Lens <code>Dutch</code> to the given value.
	/// Also stores the CinemachineVirtualCamera as the tween's target so it can be used for filtered operations</summary>
	/// <param name="endValue">The end value to reach</param>
	/// <param name="duration">The duration of the tween</param>
	public static TweenerCore<float, float, FloatOptions> DODutch(
		this CinemachineVirtualCamera target,
		float endValue,
		float duration)
	{
		TweenerCore<float, float, FloatOptions> t = DOTween.To((DOGetter<float>)(() => target.m_Lens.Dutch), (DOSetter<float>)(x => target.m_Lens.Dutch = x), endValue, duration);
		t.SetTarget<TweenerCore<float, float, FloatOptions>>((object)target);
		return t;
	}

	/// <summary>Tweens a Cinemachine Lens <code>Dutch</code> to the given value.
	/// Also stores the CinemachineFreeLook as the tween's target so it can be used for filtered operations</summary>
	/// <param name="endValue">The end value to reach</param>
	/// <param name="duration">The duration of the tween</param>
	public static TweenerCore<float, float, FloatOptions> DODutch(
		this CinemachineFreeLook target,
		float endValue,
		float duration)
	{
		TweenerCore<float, float, FloatOptions> t = DOTween.To((DOGetter<float>)(() => target.m_Lens.Dutch), (DOSetter<float>)(x => target.m_Lens.Dutch = x), endValue, duration);
		t.SetTarget<TweenerCore<float, float, FloatOptions>>((object)target);
		return t;
	}
	
	public static TweenerCore<float, float, FloatOptions> DODutch(
		this CinemachineCamera target,
		float endValue,
		float duration)
	{
		TweenerCore<float, float, FloatOptions> t = DOTween.To((DOGetter<float>)(() => target.Lens.Dutch), (DOSetter<float>)(x => target.Lens.Dutch = x), endValue, duration);
		t.SetTarget<TweenerCore<float, float, FloatOptions>>((object)target);
		return t;
	}
}
