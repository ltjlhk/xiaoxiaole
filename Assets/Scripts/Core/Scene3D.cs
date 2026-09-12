using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xio.Assets;

namespace Xio.Game
{
    /// <summary>
    /// 原版 level2 场景 3D 层一比一复刻（数据源 tools/extract_3d.py 的 dump）：
    /// - Main Camera: pos(0,50,-2.8) rot(90,0,0) 正交 size=26, near 0.3 far 100, bg #314D79
    /// - Build: Floor(45,1,90)@(0,-1.55,-2) + 左右墙(x=±14.4) + 前后墙(z=±17.2)
    /// - Droplocation@(0,0.5,-20.5): 7 槽 x=[-11.9,-7.95,-4.07,-0.12,3.72,7.61,11.55] 碰撞体(2.9,0.05,3.8)
    /// - Posall 槽底板(28.5,0.05,5.5) / MovePath(y=5.58 十点) / BlockParent 牌容器
    /// 坐标换算：屏幕x=世界x*S；屏幕y=(世界z+2.8)*S，S=1334/52=25.654
    /// </summary>
    public class Scene3D : MonoBehaviour
    {
        public static readonly float PX_PER_UNIT = 1334f / 52f;      // 25.6538
        /// <summary>7 槽世界 x（Droplocation.Pos1-7 精确值）。</summary>
        public static readonly float[] SlotX = { -11.9f, -7.95f, -4.07f, -0.12f, 3.72f, 7.61f, 11.55f };
        /// <summary>槽区世界 z（Droplocation z=-20.5 + Pos local 0.02）。</summary>
        public const float SlotZ = -20.48f;
        /// <summary>槽牌基准 y（Pos local y=0.04 + 父 0.5 + 半厚）。</summary>
        public const float SlotY = 1.2f;
        /// <summary>牌堆飞行路径高度（MovePath y=5.58）。</summary>
        public const float FlyY = 5.58f;
        /// <summary>牌尺寸（槽碰撞体反推：宽 2.9 / 深 3.8 / 厚 1.34）。</summary>
        public static readonly Vector3 BlockSize = new Vector3(2.9f, 1.34f, 3.8f);
        /// <summary>牌堆网格间距（= 槽间距 3.91）。</summary>
        public const float GridStep = 3.91f;

        public static Scene3D Inst { get; private set; }
        public Transform BlockParent;    // 牌容器
        public Transform Droplocation;   // 7 槽
        public Transform MovePath;       // 飞行路径点
        public Camera Cam;

        public static Scene3D Ensure()
        {
            if (Inst != null) return Inst;
            var go = new GameObject("Scene3D");
            var s = go.AddComponent<Scene3D>();
            s.Build();
            Inst = s;
            return s;
        }

        private void Build()
        {
            // ===== 相机（原版 Main Camera）=====
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
            }
            Cam = camGo.GetComponent<Camera>();
            Cam.orthographic = true;
            Cam.orthographicSize = 26f;
            Cam.nearClipPlane = 0.3f;
            Cam.farClipPlane = 100f;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = new Color(0.1921569f, 0.3019608f, 0.4745098f, 0f);
            camGo.transform.position = new Vector3(0f, 50f, -2.8f);
            camGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            if (camGo.GetComponent<AudioListener>() == null) camGo.AddComponent<AudioListener>();

            // ===== Build：地板 + 四墙（Unity Cube，原版 mesh 即单位立方体放大）=====
            MakeCube("Floor", new Vector3(0f, -1.55f, -2f), new Vector3(45f, 1f, 90f),
                Quaternion.Euler(0f, 180f, 0f), new Color(0.62f, 0.55f, 0.44f));
            MakeCube("WallLeft", new Vector3(-14.4f, 6f, 0f), new Vector3(0.42f, 27.81f, 57.42f),
                Quaternion.identity, new Color(0.78f, 0.74f, 0.66f));
            MakeCube("WallRight", new Vector3(14.4f, 6f, 0f), new Vector3(0.67f, 27.81f, 57.9f),
                Quaternion.identity, new Color(0.78f, 0.74f, 0.66f));
            MakeCube("WallDown", new Vector3(0f, 5.61f, -17.2f), new Vector3(0.42f, 27.81f, 49.8f),
                Quaternion.Euler(0f, 90f, 0f), new Color(0.78f, 0.74f, 0.66f));
            MakeCube("WallUp", new Vector3(0f, 5.61f, 17.2f), new Vector3(0.42f, 27.81f, 57.5f),
                Quaternion.Euler(0f, 90f, 0f), new Color(0.78f, 0.74f, 0.66f));

            // 中心装饰 Cylinder（原版 0,22.87,6 rot x90 scale1.5）
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "Cylinder";
            cyl.transform.SetParent(transform, false);
            cyl.transform.position = new Vector3(0f, 22.87f, 6f);
            cyl.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cyl.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Paint(cyl, new Color(0.85f, 0.82f, 0.72f));

            // ===== Posall 槽底板（暗色托盘 28.5×5.5，原版 Posall 有 MeshRenderer）=====
            // 坑：Unlit/Transparent 只采样 _MainTex 不乘 _Color，无贴图时渲染成纯白 → 用 Unlit/Color
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Posall";
            plate.transform.position = new Vector3(0f, 0.5f, -20.5f);
            plate.transform.localScale = new Vector3(28.5f, 0.1f, 5.5f);
            Object.Destroy(plate.GetComponent<BoxCollider>());
            Paint(plate, new Color(0.09f, 0.10f, 0.14f));

            // ===== 定位 Transform =====
            Droplocation = NewPoint("Droplocation", new Vector3(0f, 0.5f, -20.5f));
            for (int i = 0; i < 7; i++)
            {
                var p = NewPoint("Pos" + (i + 1), new Vector3(SlotX[i], 0.04f, 0.02f));
                p.SetParent(Droplocation, false);
                p.localPosition = new Vector3(SlotX[i], 0.04f, 0.02f);
                p.localScale = new Vector3(2.9f, 0.05f, 3.8f);
            }

            MovePath = NewPoint("MovePath", Vector3.zero);
            float[,] mp = {
                { 0f, 4f }, { 5.52f, 0f }, { 0.35f, -4f }, { -5.67f, 0f },
                { 0f, 10.04f }, { 6.04f, 5.94f }, { 6.1f, -4.16f }, { 0.38f, -9.31f },
                { -5.63f, -6.74f }, { -5.41f, 6.4f },
            };
            for (int i = 0; i < 10; i++)
            {
                var p = NewPoint("Pos1" + (i > 0 && i != 4 ? " (" + i + ")" : ""), Vector3.zero);
                p.SetParent(MovePath, false);
                p.localPosition = new Vector3(mp[i, 0], 5.58f, mp[i, 1]);
            }

            BlockParent = NewPoint("BlockParent", Vector3.zero);
        }

        /// <summary>世界坐标 → UI 中心系坐标（750×1334，供 GameFX 特效定位）。</summary>
        public static Vector2 WorldToScreen(Vector3 w)
        {
            return new Vector2(w.x * PX_PER_UNIT, (w.z + 2.8f) * PX_PER_UNIT);
        }

        /// <summary>槽位 i 的世界坐标（第 k 张牌抬高）。</summary>
        public static Vector3 SlotWorld(int i, int k)
        {
            return new Vector3(SlotX[i], SlotY + k * 0.3f, SlotZ);
        }

        /// <summary>牌堆格 (r,c) 层 k 的世界坐标：列对齐槽 x 间距；从后往前；层间错位遮挡。</summary>
        public static Vector3 CellWorld(int r, int c, int rows, int cols, int k)
        {
            // 动态步距：大棋盘(如 8×8)收窄，保证牌堆不插入侧墙/前墙。
            // 房间内壁 x±14.19（墙厚 0.42×0.5）、z +16.99/-16.99；块半宽 1.45、半深 1.9。
            // 可用跨度：宽 2×(14.19-1.45)=25.5，深 14.8..-14.9=29.7。
            float xStep = cols > 1 ? Mathf.Min(GridStep, 25.5f / (cols - 1)) : 0f;
            float zStep = rows > 1 ? Mathf.Min(GridStep, 29.7f / (rows - 1)) : 0f;
            float x = (c - (cols - 1) / 2f) * xStep;
            float z = 14.5f - r * zStep;
            // 层间只向镜头方向错位（顶视呈现叠压边缘，不做对角错位——对角会插进邻列/侧墙）
            float y = 0.6f + k * 0.6f;
            return new Vector3(x, y, z - k * 0.3f);
        }

        /// <summary>创建 3D 花牌方块（体块 Cube + 牌面 Quad + 根 BoxCollider + 点击）。</summary>
        public static Block3D CreateBlock(Transform parent, string texName, Vector3 worldPos)
        {
            var go = new GameObject("Block_" + texName);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;

            // 体块（白玉牌身）。原版 level2 无 Light 组件=无光照 shader → Unlit（Diffuse 无灯会渲染成深灰）
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = BlockSize;
            Object.Destroy(body.GetComponent<BoxCollider>());   // 根碰撞体接管
            Paint(body, new Color(0.97f, 0.95f, 0.9f));

            // 牌面（朝上 Quad，贴花牌贴图，透明队列不写深度）
            // Unity Quad 法线朝 -Z：R_x(+90) 把法线转到 +Y（朝上，俯视可见）。
            // 原版 Plane001 的 Euler(-90) 是 Plane 网格坐标系（法线 +Z），不能照搬到 Quad。
            var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = "Face";
            face.transform.SetParent(go.transform, false);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            face.transform.localPosition = new Vector3(0f, BlockSize.y / 2f + 0.01f, 0f);
            face.transform.localScale = new Vector3(BlockSize.x - 0.18f, BlockSize.z - 0.22f, 1f);
            var col = face.GetComponent<MeshCollider>();
            if (col != null) Object.Destroy(col);
            var sp = OriginalAssets.Get("flower", texName);
            face.GetComponent<MeshRenderer>().sharedMaterial = FaceMaterial(texName, sp);

            // 点击碰撞体（挂根上，覆盖整块）
            var bc = go.AddComponent<BoxCollider>();
            bc.size = BlockSize;

            var blk = go.AddComponent<Block3D>();
            blk.TexName = texName;
            return blk;
        }

        private static readonly Dictionary<string, Material> _faceMats = new Dictionary<string, Material>();

        /// <summary>花牌面材质缓存（16 花色共享）。
        /// Quad +90° 后 V 轴(+Y local)→+Z 世界=相机屏幕上方 → 贴图正立，无需 UV 翻转。</summary>
        private static Material FaceMaterial(string texName, Sprite sp)
        {
            Material m;
            if (_faceMats.TryGetValue(texName, out m) && m != null) return m;
            if (sp != null)
                m = new Material(Shader.Find("Unlit/Transparent")) { mainTexture = sp.texture };
            else
            {
                Debug.LogWarning("[Scene3D] 花牌贴图缺失: " + texName);
                m = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.8f, 0.8f, 0.85f) };
            }
            _faceMats[texName] = m;
            return m;
        }

        private static Transform NewPoint(string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            return go.transform;
        }

        private static GameObject MakeCube(string name, Vector3 pos, Vector3 scale, Quaternion rot, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = rot;
            Paint(go, color);
            return go;
        }

        private static void Paint(GameObject go, Color c)
        {
            var r = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Unlit/Color")) { color = c };
            r.sharedMaterial = mat;
        }
    }

    /// <summary>3D 花牌方块：点击收集；飞入槽（贝塞尔）；三消缩小消失。</summary>
    public class Block3D : MonoBehaviour
    {
        public string TexName;
        public int CellId = -1;
        public bool InSlot;
        public System.Action<Block3D> OnClick;
        public bool Clickable = true;

        private void OnMouseDown()
        {
            // 兜底：GamePanel 主动射线为主（UI 混合 3D 下 OnMouseDown 不可靠），此处不处理避免双触发
        }

        /// <summary>飞入槽（贝塞尔：起点→中点抬高→终点）。</summary>
        public Coroutine FlyTo(Vector3 to, float dur = 0.28f)
        {
            return StartCoroutine(FlyRoutine(to, dur));
        }

        private IEnumerator FlyRoutine(Vector3 to, float dur)
        {
            Vector3 from = transform.position;
            Vector3 mid = (from + to) * 0.5f + Vector3.up * Scene3D.FlyY;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                float u = Mathf.Clamp01(t);
                // 二次贝塞尔
                Vector3 a = Vector3.Lerp(from, mid, u);
                Vector3 b = Vector3.Lerp(mid, to, u);
                transform.position = Vector3.Lerp(a, b, u);
                yield return null;
            }
            transform.position = to;
        }

        /// <summary>三消消失：缩小 + 上升。</summary>
        public Coroutine Vanish()
        {
            return StartCoroutine(VanishRoutine());
        }

        private IEnumerator VanishRoutine()
        {
            Clickable = false;
            Vector3 s0 = transform.localScale;
            Vector3 p0 = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.25f;
                float u = Mathf.Clamp01(t);
                transform.localScale = s0 * (1f - u * 0.85f);
                transform.position = p0 + Vector3.up * (u * 2.2f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
