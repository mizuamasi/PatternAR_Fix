using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private GameObject loadingScreenPrefab;
    private GameObject loadingScreenInstance;
    private Image progressBarFill;
    private Text progressText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // トランジション画面をインスタンス化
        loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
        progressBarFill = loadingScreenInstance.transform.Find("ProgressBar/Fill").GetComponent<Image>();
        progressText = loadingScreenInstance.transform.Find("ProgressText").GetComponent<Text>();

        // トランジション画面を非表示に設定
        CanvasGroup canvasGroup = loadingScreenInstance.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        // フェードイン
        yield return StartCoroutine(FadeLoadingScreen(true));

        // フェードインが完了してからシーンのロードを開始
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgressUI(progress);

            if (operation.progress >= 0.9f)
            {
                UpdateProgressUI(1f);
                // ここで少し待機して、100%の表示を見せる
                yield return new WaitForSeconds(0.2f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 新しいシーンが完全に読み込まれるまで待機
        yield return new WaitForSeconds(0.5f);

        // フェードアウト
        yield return StartCoroutine(FadeLoadingScreen(false));

        // トランジション画面を破棄
        Destroy(loadingScreenInstance);
    }

    private void UpdateProgressUI(float progress)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }
        if (progressText != null)
        {
            progressText.text = $"Loading... {Mathf.Round(progress * 100)}%";
        }
    }

    private IEnumerator FadeLoadingScreen(bool fadeIn)
    {
        CanvasGroup canvasGroup = loadingScreenInstance.GetComponent<CanvasGroup>();
        float duration = 0.5f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}