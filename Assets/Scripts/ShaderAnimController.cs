using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShaderAnimController : MonoBehaviour
{
    [Header("애니메이션 데이터")]
    [SerializeField] private ShaderAnimData currentAnim;

    [Header("속도 배율")]
    [SerializeField] private float speedMultiplier = 1f;

    private SpriteRenderer rend;
    private MaterialPropertyBlock mpb;
    private float baseFrameRate;
    public bool isInitialized;

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SetAnimation(ShaderAnimData animData)
    {
        if (animData == null || animData.atlas == null || rend == null) return;
        if (animData.frames != null && animData.frames.Length > 0)
        {
            rend.sprite = animData.frames[0];
        }

        currentAnim = animData;
        baseFrameRate = animData.defaultFrameRate;
        animData.GetUVData(out float uvWidth, out float uvHeight, out float startU, out float startV);

        rend.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", animData.atlas);
        mpb.SetFloat("_FrameWidth", uvWidth);
        mpb.SetFloat("_FrameHeight", uvHeight);
        mpb.SetFloat("_StartU", startU);
        mpb.SetFloat("_StartV", startV);
        mpb.SetFloat("_TotalFrames", animData.totalFrames);
        mpb.SetFloat("_FrameColumns", animData.columns);
        mpb.SetFloat("_FrameRate", baseFrameRate * speedMultiplier);
        mpb.SetFloat("_TimeOffset", animData.sync ? 0f : Random.Range(0f, 10f));

        rend.SetPropertyBlock(mpb);
        isInitialized = true;
    }

    public void SetStaticSprite(Sprite sprite)
    {
        if (sprite == null || rend == null) return;

        rend.sprite = sprite;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_FrameRate", 0f);
        mpb.SetTexture("_MainTex", sprite.texture);
        rend.SetPropertyBlock(mpb);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;

        if (!isInitialized) return;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_FrameRate", baseFrameRate * speedMultiplier);
        rend.SetPropertyBlock(mpb);
    }

    public void Refresh()
    {
        if (currentAnim != null)
            SetAnimation(currentAnim);
    }

    public void Pause()
    {
        if (!isInitialized) return;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_FrameRate", 0f);
        rend.SetPropertyBlock(mpb);
    }

    public void Resume()
    {
        if (!isInitialized) return;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_FrameRate", baseFrameRate * speedMultiplier);
        rend.SetPropertyBlock(mpb);
    }
}