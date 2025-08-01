using UnityEngine;
using Dreamteck.Splines;

namespace Dreamteck.Forever
{
    [AddComponentMenu("Dreamteck/Forever/Path Generators/Custom Path Generator")]
    public class CustomPathGenerator : LevelPathGenerator
    {
        [Header("Path points (optional)")]
        [SerializeField] private string entryName = "Entry";
        [SerializeField] private string exitName = "Exit";

        [Header("Orientation")]
        [SerializeField] private bool keepUpright = true;
        [SerializeField] private bool overrideUp = false;
        [SerializeField] private Vector3 upOverride = Vector3.up;

        [Header("Safety")]
        [SerializeField] private float minStraightLength = 0.01f;

        public override void Initialize(LevelGenerator generator)
        {
            base.Initialize(generator);
            LevelGenerator.onSegmentCreated += OnSegmentCreated;
        }

        private void OnDestroy()
        {
            LevelGenerator.onSegmentCreated -= OnSegmentCreated;
        }

        private void OnSegmentCreated(LevelSegment segment)
        {
            // Генерируем прямую без деформации геометрии
            BuildStraightPath(segment);
        }

        private void BuildStraightPath(LevelSegment segment)
        {
            // 1) Пытаемся найти Entry/Exit
            Transform entry = FindPoint(segment.transform, entryName);
            Transform exit = FindPoint(segment.transform, exitName);

            Vector3 a, b;

            if (entry != null && exit != null)
            {
                a = entry.position;
                b = exit.position;
            }
            else
            {
                // 2) Фоллбек — от краёв bounds по оси сегмента
                var bounds = segment.GetBounds(); // TS_Bounds
                var t = segment.transform;

                switch (segment.axis)
                {
                    case LevelSegment.Axis.X:
                        a = t.TransformPoint(new Vector3(-bounds.size.x * 0.5f, 0f, 0f));
                        b = t.TransformPoint(new Vector3(bounds.size.x * 0.5f, 0f, 0f));
                        break;
                    case LevelSegment.Axis.Y:
                        a = t.TransformPoint(new Vector3(0f, -bounds.size.y * 0.5f, 0f));
                        b = t.TransformPoint(new Vector3(0f, bounds.size.y * 0.5f, 0f));
                        break;
                    default: // Z
                        a = t.TransformPoint(new Vector3(0f, 0f, -bounds.size.z * 0.5f));
                        b = t.TransformPoint(new Vector3(0f, 0f, bounds.size.z * 0.5f));
                        break;
                }
            }

            // Страховка от нулевой длины
            if ((b - a).sqrMagnitude < minStraightLength * minStraightLength)
                b = a + Vector3.forward * minStraightLength;

            Vector3 forward = (b - a).normalized;
            Vector3 up = overrideUp ? upOverride : (keepUpright ? Vector3.up : Vector3.up);

            // 3) Записываем выборки напрямую (без spline.Rebuild и т.п.)
            if (segment.path.samples == null || segment.path.samples.Length != 2)
                segment.path.samples = new SplineSample[2];

            segment.path.samples[0] = new SplineSample(a, up, forward, Color.white, 1f, 0.0);
            segment.path.samples[1] = new SplineSample(b, up, forward, Color.white, 1f, 1.0);

            // Важно: обнуляем ссылку на spline, чтобы Forever использовал samples как есть
            segment.path.spline = null;
        }

        private Transform FindPoint(Transform root, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // Часто ноды лежат в корне или под "Path/"
            var t = root.Find(name);
            if (t == null) t = root.Find("Path/" + name);
            return t;
        }
    }
}
