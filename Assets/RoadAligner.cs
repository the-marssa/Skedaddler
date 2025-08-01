using UnityEngine;

// Скрипт для выравнивания дорожных сегментов по Z без щелей и смещений
public class RoadAligner : MonoBehaviour
{
    [Header("Список всех дорожных сегментов (по порядку)")]
    [SerializeField] private Transform[] segments;

    [Header("Начальная позиция выравнивания")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;

    [Header("Ручная длина сегмента (если не читаем из меша)")]
    [SerializeField] private float fallbackLength = 9f;

    void Start()
    {
        AlignSegments();
    }

    /// <summary>
    /// Основной метод — выравнивает все сегменты друг за другом вдоль оси Z
    /// </summary>
    private void AlignSegments()
    {
        Vector3 currentPosition = startPosition;

        foreach (Transform segment in segments)
        {
            float lengthZ = fallbackLength; // по умолчанию
            float offsetZ = 0f;

            // Пробуем получить Mesh из дочернего MeshFilter
            MeshFilter meshFilter = segment.GetComponentInChildren<MeshFilter>();

            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Mesh mesh = meshFilter.sharedMesh;

                // Длина и центр по Z
                lengthZ = mesh.bounds.size.z;
                offsetZ = mesh.bounds.center.z;

                // Можно вывести в консоль для дебага
                // Debug.Log($"[{segment.name}] length: {lengthZ}, offset: {offsetZ}");
            }

            // Ставим сегмент с учётом центра меша
            segment.localPosition = currentPosition - new Vector3(0, 0, offsetZ);

            // Обновляем текущую позицию для следующего сегмента
            currentPosition += new Vector3(0, 0, lengthZ);
        }
    }
}
