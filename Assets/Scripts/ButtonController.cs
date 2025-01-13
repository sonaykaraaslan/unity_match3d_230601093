using UnityEngine;
using UnityEngine.UI;
using Match.View;
public class ButtonController : MonoBehaviour
{
    private Button _button;
    private UIController _uiController;

    void Start()
    {
        _button = GetComponent<Button>();
        _uiController = FindObjectOfType<UIController>();

        if (_button != null && _uiController != null)
        {
            _button.onClick.AddListener(() => {
                Debug.Log("Reset button clicked");
                _uiController.ResetGame();
            });
        }
    }

    void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}