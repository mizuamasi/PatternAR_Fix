using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    public string sceneName;

    void Start()
    {
        // ボタンコンポーネントを取得し、クリックイベントにリスナーを追加
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        SceneTransitionManager.Instance.LoadScene(sceneName);
        // 指定されたシーンをロード
        SceneManager.LoadScene(sceneName);
    }
}
