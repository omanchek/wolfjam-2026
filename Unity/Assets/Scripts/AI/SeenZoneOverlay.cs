using UnityEngine;

// Covers the camera view in a color, leaving a soft-edged hole over the seen zone
[RequireComponent(typeof(MeshRenderer))]
public class SeenZoneOverlay : MonoBehaviour
{
    private static readonly int ImageId = Shader.PropertyToID("_MainTex");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");

    [SerializeField] private Texture image; // Stretched over the camera view, and left empty for a solid color
    [SerializeField] private Color color = Color.black; // Tints the image, so use white to show it unchanged
    [SerializeField, Min(0.001f)] private float edgeSoftness = 1f; // How far the fade reaches inward from the circle's edge

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock properties;

    public Texture Image
    {
        get => image;
        set => image = value;
    }

    public Color Color
    {
        get => color;
        set => color = value;
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        properties = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        AIGameManager manager = AIGameManager.Instance;
        Rect board = manager.BoardRect;
        transform.position = board.center;
        transform.localScale = new Vector3(board.width, board.height, 1f);

        properties.SetTexture(ImageId, image != null ? image : Texture2D.whiteTexture);
        properties.SetColor(ColorId, color);
        properties.SetVector(CenterId, manager.SeenZonePosition);
        properties.SetFloat(RadiusId, manager.SeenZoneRadius);
        properties.SetFloat(SoftnessId, Mathf.Min(edgeSoftness, manager.SeenZoneRadius + 0.001f)); // Small circles still keep a clear center
        meshRenderer.SetPropertyBlock(properties);
    }
}
