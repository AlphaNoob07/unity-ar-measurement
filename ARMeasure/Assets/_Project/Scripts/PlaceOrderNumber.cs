using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlaceOrderNumber : MonoBehaviour
{
    [SerializeField] private GameObject m_startBtn;
    [SerializeField] private TMP_InputField m_InputField;

    private void Awake()
    {
        if (m_startBtn.activeSelf)
        {
            m_startBtn.SetActive(false);
        }
    }

    private void Update()
    {
        if (m_startBtn.activeSelf) return;
        CheckInput();
    }

    private void CheckInput()
    {
        string inputText = m_InputField.text;
        if (inputText.Length == 4 && int.TryParse(inputText, out int result))
        {
            
            m_startBtn.SetActive(true);
        }
        else
        {
            m_startBtn.SetActive(false);
        }
    }
}
