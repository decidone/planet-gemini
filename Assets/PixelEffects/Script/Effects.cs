using UnityEngine;
using ca.HenrySoftware;
using ca.HenrySoftware.Rage;
[RequireComponent(typeof(Pool))]
public class Effects : MonoBehaviour
{
	public ModelEffectAnimation Block;
	public ModelEffectAnimation Box;
	public ModelEffectAnimation Bubble;
	public ModelEffectAnimation Circle;
	public ModelEffectAnimation Claw;
	public ModelEffectAnimation Consume;
	public ModelEffectAnimation Dark;
	public ModelEffectAnimation Earth;
	public ModelEffectAnimation Electric;
	public ModelEffectAnimation Explode;
	public ModelEffectAnimation Fire;
	public ModelEffectAnimation Footprints;
	public ModelEffectAnimation Glint;
	public ModelEffectAnimation Heal;
	public ModelEffectAnimation Ice;
	public ModelEffectAnimation Lightning;
	public ModelEffectAnimation Nuclear;
	public ModelEffectAnimation Poison;
	public ModelEffectAnimation Puff;
	public ModelEffectAnimation Shield;
	public ModelEffectAnimation Slash;
	public ModelEffectAnimation Sparks;
	public ModelEffectAnimation SplatterBlood;
	public ModelEffectAnimation SplatterSlime;
	public ModelEffectAnimation Square;
	public ModelEffectAnimation Star;
	public ModelEffectAnimation Teleport;
	public ModelEffectAnimation Touch;
	public ModelEffectAnimation Warp;
	public ModelEffectAnimation Water;
	public ModelEffectAnimation Web;
	Pool _pool;

    public static Effects instance;

    void Awake()
	{
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
		_pool = GetComponent<Pool>();
	}

	void Finish()
	{
		StopAllCoroutines();
		Ease.GoAlpha(this, 1f, 0f, 1f, null, null, EaseType.Linear);
	}

	[ContextMenu("TriggerBlock")]
	public void TriggerBlock(GameObject obj)
	{
		Trigger(Block, obj);
	}

	[ContextMenu("TriggerBox")]
	public void TriggerBox(bool alternate, GameObject obj)
	{
		Trigger(Box, obj, true, alternate);
	}

	[ContextMenu("TriggerBubble")]
	public void TriggerBubble(GameObject obj)
	{
		Trigger(Bubble, obj);
	}

	[ContextMenu("TriggerCircle")]
	public void TriggerCircle(GameObject obj)
	{
		Trigger(Circle, obj);
	}

	[ContextMenu("TriggerClaw")]
	public void TriggerClaw(bool alternate, GameObject obj)
	{
		Trigger(Claw, obj, true, alternate);
	}

	[ContextMenu("TriggerConsume")]
	public void TriggerConsume(float type, GameObject obj)
	{
		Trigger(Consume, obj, false, false, true, type);
	}

	[ContextMenu("TriggerDark")]
    public void TriggerDark(GameObject obj)
    {
        Trigger(Dark, obj);
    }

    [ContextMenu("TriggerEarth")]
	public void TriggerEarth(GameObject obj)
	{
		Trigger(Earth, obj);
	}

	[ContextMenu("TriggerElectric")]
	public void TriggerElectric(GameObject obj)
	{
		Trigger(Electric, obj);
	}

	[ContextMenu("TriggerExplode")]
	public void TriggerExplode(float type, GameObject obj)
	{
		Trigger(Explode, obj, false, false, true, type);
	}

	[ContextMenu("TriggerFire")]
	public void TriggerFire(GameObject obj)
	{
		Trigger(Fire, obj);
	}

	[ContextMenu("TriggerGlint")]
	public void TriggerGlint(GameObject obj)
	{
		Trigger(Glint, obj);
	}

	[ContextMenu("TriggerHeal")]
	public void TriggerHeal(bool alternate, GameObject obj)
	{
		Trigger(Heal, obj, true, alternate);
	}

	[ContextMenu("TriggerIce")]
	public void TriggerIce(GameObject obj)
	{
		Trigger(Ice, obj);
	}

	[ContextMenu("TriggerLightning")]
	public void TriggerLightning(GameObject obj)
	{
		Trigger(Lightning, obj);
	}

	[ContextMenu("TriggerNuclear")]
	public void TriggerNuclear(GameObject obj)
	{
		Trigger(Nuclear, obj);
	}

	[ContextMenu("TriggerPoison")]
	public void TriggerPoison(GameObject obj)
	{
		Trigger(Poison, obj);
	}

	[ContextMenu("TriggerPuff")]
	public void TriggerPuff(GameObject obj)
	{
		Trigger(Puff, obj);
	}

	[ContextMenu("TriggerShield")]
	public void TriggerShield(GameObject obj)
	{
		Trigger(Shield, obj);
	}

	[ContextMenu("TriggerSlash")]
	public void TriggerSlash(float type, GameObject obj)
	{
		Trigger(Slash, obj, false, false, true, type);
	}

	[ContextMenu("TriggerSparks")]
	public void TriggerSparks(GameObject obj)
	{
		Trigger(Sparks, obj);
	}

	[ContextMenu("TriggerSplatterBlood")]
	public void TriggerSplatterBlood(GameObject obj)
	{
		Trigger(SplatterBlood, obj);
	}

	[ContextMenu("TriggerSplatterSlime")]
	public void TriggerSplatterSlime(Color? color, GameObject obj)
	{
		Trigger(SplatterSlime, obj, false, false, false, 0f, true, color);
	}

	[ContextMenu("TriggerSquare")]
	public void TriggerSquare(bool alternate, GameObject obj)
	{
		Trigger(Square, obj, true, alternate);
	}

	[ContextMenu("TriggerStar")]
	public void TriggerStar(GameObject obj)
	{
		Trigger(Star, obj);
	}

	[ContextMenu("TriggerTeleport")]
	public void TriggerTeleport(GameObject obj)
	{
		Trigger(Teleport, obj);
	}

	[ContextMenu("TriggerTouch")]
	public void TriggerTouch(GameObject obj, Color? color = null)
	{
		Trigger(Touch, obj, false, false, false, 0, true, color);
	}

	[ContextMenu("TriggerWarp")]
	public void TriggerWarp(GameObject obj)
	{
		Trigger(Warp, obj);
	}

	[ContextMenu("TriggerWater")]
	public void TriggerWater(GameObject obj)
	{
		Trigger(Water, obj);
	}

	[ContextMenu("TriggerWeb")]
	public void TriggerWeb(GameObject obj)
	{
		Trigger(Web, obj);
	}

	public static int AnimatorTrigger = Animator.StringToHash("Trigger");
	public static int AnimatorAlternate = Animator.StringToHash("Alternate");
	public static int AnimatorType = Animator.StringToHash("Type");

    public void Trigger(ModelEffectAnimation model, GameObject obj,
        bool setAlternate = false, bool alternate = false,
        bool setType = false, float type = 0f,
        bool setColor = false, Color? color = null)
    {
        var o = _pool.Enter();
        o.transform.SetParent(obj.transform, false);
        //o.transform.localPosition = new Vector3(p.x, p.y, transform.localPosition.z);
        o.transform.localScale = Vector3.one;
        var spriteRenderers = o.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = (setColor && color != null && color.HasValue) ? color.Value : Color.white;
        }
        spriteRenderers[1].transform.localPosition = model.BackOffset;
        spriteRenderers[1].sortingOrder = -1;
        spriteRenderers[2].transform.localPosition = model.ForeOffset;
        spriteRenderers[2].sortingOrder = 2;
        spriteRenderers[0].transform.localPosition = model.Offset;
        spriteRenderers[0].sortingOrder = 1;
        var animator = o.GetComponentInChildren<Animator>();
        animator.runtimeAnimatorController = model.Controller;
        if (setAlternate)
            animator.SetBool(AnimatorAlternate, alternate);
        if (setType)
            animator.SetFloat(AnimatorType, type);
        animator.SetTrigger(AnimatorTrigger);
    }

    public void Exit(GameObject o)
	{
		// handled by stateRemove when animaion done
		_pool.Exit(o);
	}
}
