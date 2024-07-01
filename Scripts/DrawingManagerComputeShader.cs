using Unity.VisualScripting;
using UnityEngine;

public class DrawingManagerComputeShader : MonoBehaviour
{
    [SerializeField] Transform topLeftCorner, bottomRightCorner;
    [SerializeField] int pixelDensity = 2000;
    public ComputeShader computeShader;
    public Texture2D inputTexture;
    private RenderTexture renderTexture;
    private Material material;
    Vector3 mousePos = Vector3.zero;
    [SerializeField] Color drawColor = Color.black;
    [SerializeField] float drawSize = .01f;
    Vector2 drawPosition;
    Vector2 lastDrawPosition;
    Vector2Int textureSize = Vector2Int.zero;
    Vector2 physicalSize;
    [SerializeField] Texture2D stampTexture;
    [SerializeField] float stampSize;
    [SerializeField] float stampRotation;
    [SerializeField] Color stampColor;
    enum Tools
    {
        Brush,
        Eraser,
        Stamp
    }

    [SerializeField] Tools tool = Tools.Brush;

    private struct DrawData
    {
        public Vector2 position;
        public Color color;
        public float radius;
    }
    private DrawData[] drawDataArray;
    private ComputeBuffer drawDataBuffer;


    private struct StampData
    {
        public Vector2 position;
        public Color color;
        public float size;
        public Vector2 resolution;
        public float rotation;
    }
    private StampData[] stampDataArray;
    private ComputeBuffer stampDataBuffer;


    bool pressedDrawLastFrame = false;




    void Start()
    {


        physicalSize.x = Mathf.Abs(topLeftCorner.transform.position.x - bottomRightCorner.transform.position.x);
        physicalSize.y = Mathf.Abs(topLeftCorner.transform.position.z - bottomRightCorner.transform.position.z);

        textureSize.x = (int)(physicalSize.x * pixelDensity);
        textureSize.y = (int)(physicalSize.y * pixelDensity);



        mousePos = transform.position;
        renderTexture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        RenderTexture.active = renderTexture;
        GL.Clear(false, true, new Color(0, 0, 0, 0));
        RenderTexture.active = null;



        Graphics.Blit(inputTexture, renderTexture);

        drawDataArray = new DrawData[1];
        drawDataBuffer = new ComputeBuffer(drawDataArray.Length, sizeof(float) * (2 + 4 + 1)); // 2 for position, 4 for color, 1 for radius

        stampDataArray = new StampData[1];
        stampDataBuffer = new ComputeBuffer(stampDataArray.Length, sizeof(float) * (2 + 4 + 1 + 2 + 1)); // 2 for position, 4 for color, 1 for size, 2 for resolution, 1 for rotation


        material = GetComponent<Renderer>().material;
        material.SetTexture("_RenderTexture", renderTexture);
        Clear();

    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit)) return;
        if (!(hit.collider is MeshCollider)) return;
 
        drawPosition = hit.textureCoord;

        if (Input.GetMouseButtonDown(0)) 
        {
            if (tool == Tools.Stamp) Stamp(drawPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            if (tool == Tools.Brush) Draw();
        }
        else
        {
            pressedDrawLastFrame = false;
        }
            
        
    }



    void Draw()
    {
        
       // && (lastDrawPosition.x != drawPosition.x || lastDrawPosition.y != drawPosition.y)
       if ((pressedDrawLastFrame))
       {
            float dist = Mathf.Sqrt(
                   ((drawPosition.x * textureSize.x - lastDrawPosition.x * textureSize.x) * (drawPosition.x * textureSize.x - lastDrawPosition.x * textureSize.x)) +
                   ((drawPosition.y * textureSize.y - lastDrawPosition.y * textureSize.y) * (drawPosition.y * textureSize.y - lastDrawPosition.y * textureSize.y))
                   );
            for (int i = 0; i <= dist; i++)
            {
                DrawBrush(
                    new Vector2(
                        (i * drawPosition.x + (dist - i) * lastDrawPosition.x) / dist,
                        (i * drawPosition.y + (dist - i) * lastDrawPosition.y) / dist)
                    );
            }
        }
       else
       {
          // if ((lastDrawPosition.x != drawPosition.x || lastDrawPosition.y != drawPosition.y) || true)
               DrawBrush(new Vector2(drawPosition.x, drawPosition.y));
       }
       pressedDrawLastFrame = true;





        lastDrawPosition = drawPosition;
    }




    void Clear()
    {
       int kernelHandle = computeShader.FindKernel("CSClear");
       computeShader.SetTexture(kernelHandle, "RenderTexture", renderTexture);
       int threadGroupsX = Mathf.CeilToInt(renderTexture.width / 8.0f);
       int threadGroupsY = Mathf.CeilToInt(renderTexture.height / 8.0f);
       computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
    }


    void DrawBrush(Vector2 uv)
    {
        drawDataArray[0].position = uv;
        drawDataArray[0].color = drawColor;
        drawDataArray[0].radius = drawSize / 1000;

        drawDataBuffer.SetData(drawDataArray);

        int kernelHandle = computeShader.FindKernel("CSMain");

        computeShader.SetTexture(kernelHandle, "RenderTexture", renderTexture);
        computeShader.SetBuffer(kernelHandle, "drawData", drawDataBuffer);
        computeShader.SetInt("width", renderTexture.width);
        computeShader.SetInt("height", renderTexture.height);
        computeShader.SetInt("drawDataLength", drawDataArray.Length);

        int threadGroupsX = Mathf.CeilToInt(renderTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(renderTexture.height / 8.0f);

        computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
    }

    void Stamp(Vector2 uv)
    {
        stampDataArray[0].position = uv;
        stampDataArray[0].color = stampColor;
        stampDataArray[0].size = stampSize;
        stampDataArray[0].resolution = stampTexture.Size();
        stampDataArray[0].rotation = stampRotation;

        stampDataBuffer.SetData(stampDataArray);

        int kernelHandle = computeShader.FindKernel("CSStamp");

        computeShader.SetTexture(kernelHandle, "RenderTexture", renderTexture);
        computeShader.SetTexture(kernelHandle, "StampTexture", stampTexture);
        computeShader.SetBuffer(kernelHandle, "stampData", stampDataBuffer);
        computeShader.SetInt("width", renderTexture.width);
        computeShader.SetInt("height", renderTexture.height);
        computeShader.SetInt("stampDataLength", stampDataArray.Length);

        int threadGroupsX = Mathf.CeilToInt(renderTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(renderTexture.height / 8.0f);

        computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
    }


    void OnDestroy()
    {
        drawDataBuffer.Release();
        stampDataBuffer.Release();


        
    }



}

