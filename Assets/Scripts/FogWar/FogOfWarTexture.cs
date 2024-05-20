using System;
using UnityEngine;

public class FogOfWarTexture : MonoBehaviour
{
    public Texture2D Texture;

    [SerializeField]
    private Color GreyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    [SerializeField]
    protected Color WhiteColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField]
    private Color StartColor = new Color(0, 0, 0, 1.0f);

    [SerializeField]
    private SpriteRenderer SpriteRenderer;

    [SerializeField]
    private float InterpolateSpeed = 50f;

    [NonSerialized]
    private Color[] Colors;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void CreateTexture(int width, int height, Vector2 scale)
    {
        Texture = new Texture2D(width, height);
        SpriteRenderer.sprite = Sprite.Create(Texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1);
        SpriteRenderer.transform.localScale = scale;

        int size = width * height;
        Colors = new Color[size];
        for (int i = 0; i < size; ++i)
            Colors[i] = StartColor;

        Texture.SetPixels(Colors);
        Texture.Apply();
    }

    public void SetTexture(Grid visibleGrid, Grid previousVisibleGrid, int team)
    {
        for (int i = 0; i < visibleGrid.Size; ++i)
        {
            bool isVisible = (visibleGrid.Get(i) & team) == team;
            bool wasVisible = (previousVisibleGrid.Get(i) & team) == team;

            Color newColor = StartColor;
            if (isVisible)
                newColor = WhiteColor;
            else if (wasVisible)
                newColor = GreyColor;

            newColor.r = Mathf.Lerp(Colors[i].r, newColor.r, Time.deltaTime * InterpolateSpeed);
            Colors[i] = newColor;
        }

        Texture.SetPixels(Colors);
        Texture.Apply();
    }
}
