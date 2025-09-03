using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using Dreamteck.Forever;

public class GameResetter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private MainMenuUI menu;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Gameplay")]
    [SerializeField] private GameObject levelGenerator;
    [SerializeField] private Runner runner;
    [SerializeField] private Transform playerRoot;

    [Header("Call on restart")]
    [SerializeField] private UnityEvent onRunReset;

    Vector3 playerStartPos;
    Quaternion playerStartRot;
    Vector3 playerStartScale;

    
    List<Transform> initialSegments = new();

    void Awake()
    {
        if (playerRoot)
        {
            playerStartPos = playerRoot.position;
            playerStartRot = playerRoot.rotation;
            playerStartScale = playerRoot.localScale;
        }

        if (levelGenerator)
        {
            foreach (Transform t in levelGenerator.transform)
                initialSegments.Add(t);
        }

        SetHudVisible(true);
    }

    
    public void PrepareNewRun()
    {
        Time.timeScale = 0f;

        if (runner) runner.enabled = false;
        if (gameOverPanel) gameOverPanel.SetActive(false);

       

        SetHudVisible(false);
    }

    public void RestartFromBeginning()
    {
        PrepareNewRun();

        ClearGeneratedSegments();
        ResetPlayerTransform();
        ResetRunnerDistanceByReflection();
        RebuildGeneratorByReflection();     
        StartCoroutine(NudgeGenerator());   

        onRunReset?.Invoke();
        if (startScreen) startScreen.SetActive(true);
    }

    public void ReturnToMenu()
    {
        PrepareNewRun();

        ClearGeneratedSegments();
        ResetPlayerTransform();
        ResetRunnerDistanceByReflection();
        RebuildGeneratorByReflection();
        StartCoroutine(NudgeGenerator());

        if (startScreen) startScreen.SetActive(false);
        if (menu) menu.BackToHome();
    }

    
    void ClearGeneratedSegments()
    {
        if (!levelGenerator) return;

        var keep = new HashSet<Transform>(initialSegments);
        var toDestroy = new List<GameObject>();

        foreach (Transform child in levelGenerator.transform)
        {
            if (!keep.Contains(child) || child.name.EndsWith("(Clone)"))
                toDestroy.Add(child.gameObject);
        }

        foreach (var go in toDestroy) Destroy(go);

        foreach (var t in initialSegments)
            if (t) t.gameObject.SetActive(true);
    }

    void ResetPlayerTransform()
    {
        if (!playerRoot) return;

        
        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        var rb = playerRoot.GetComponent<Rigidbody>();
        if (rb)
        {
            ResetRigidbody(rb, playerStartPos, playerStartRot);
        }
        else
        {
            playerRoot.SetPositionAndRotation(playerStartPos, playerStartRot);
        }

        playerRoot.localScale = playerStartScale;

        if (cc) cc.enabled = true;

        
        var anim = playerRoot.GetComponentInChildren<Animator>(true);
        if (anim)
        {
            anim.Rebind();
            if (anim.isActiveAndEnabled) anim.Update(0f);
            
        }
    }

    void ResetRigidbody(Rigidbody rb, Vector3 pos, Quaternion rot)
    {
        if (!rb) return;

        if (rb.isKinematic)
        {
            rb.position = pos;
            rb.rotation = rot;
            rb.Sleep(); 
            return;
        }

       
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif

        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(pos);
        rb.MoveRotation(rot);
    }

    
    void ResetRunnerDistanceByReflection()
    {
        if (!runner) return;

        var t = runner.GetType();
        try
        {
            
            var prop = t.GetProperty("distance",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite) prop.SetValue(runner, 0f);

            
            var m = t.GetMethod("Reset") ?? t.GetMethod("Restart") ?? t.GetMethod("Rebuild");
            if (m != null) m.Invoke(runner, null);
        }
        catch { /* no-op */ }
    }

    
    void RebuildGeneratorByReflection()
    {
        if (!levelGenerator) return;

        foreach (var c in levelGenerator.GetComponents<MonoBehaviour>())
        {
            var m = c.GetType().GetMethod("Reset") ?? c.GetType().GetMethod("Restart") ?? c.GetType().GetMethod("Rebuild");
            if (m != null)
            {
                try { m.Invoke(c, null); } catch { /* ignore */ }
            }
        }
    }

    
    IEnumerator NudgeGenerator()
    {
        if (!levelGenerator) yield break;

        
        if (!levelGenerator.activeSelf)
        {
            levelGenerator.SetActive(true);
            yield return null;
            yield break;
        }

        levelGenerator.SetActive(false);
        yield return new WaitForEndOfFrame(); 
        levelGenerator.SetActive(true);
    }

    private void SetHudVisible(bool v)
    {
        if (!hudRoot) return;
        var cg = hudRoot.GetComponent<CanvasGroup>() ?? hudRoot.AddComponent<CanvasGroup>();
        cg.alpha = v ? 1f : 0f;
        cg.interactable = v;
        cg.blocksRaycasts = v;
    }
}
