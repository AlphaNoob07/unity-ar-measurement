using UnityEngine;

public class LoginController : MonoBehaviour
{

    public static LoginController instance;

    [SerializeField] private GameObject[] flow1;
    [SerializeField] private GameObject[] flow2;
    [SerializeField] NxtBtn [] m_nxtBtns;
    private int activeIndex = 0;
    private int flowsToShow;
    private int flowsShown = 0;
    [SerializeField]
    private float nextTime = 3.0f;



    private void Awake()
    {
        if (instance == null) { instance = this; }
        flowsToShow = flow1.Length - 1;
        SetActiveFlow(0);

        for(int i=0;i<m_nxtBtns.Length;i++)
        {
            m_nxtBtns[i].btnNumber = i;
        }
    }

    private void Start()
    {
        InvokeRepeating("ShowNextFlow", nextTime, nextTime);
    }

    private void ShowNextFlow()
    {
        activeIndex = (activeIndex + 1) % flow1.Length;
        SetActiveFlow(activeIndex);

        flowsShown++;
        if (flowsToShow > flowsShown)
        {
            return;
        }
        else
        {
            CancelInvoke("ShowNextFlow");
        }
    }

    private void SetActiveFlow(int index)
    {
        for (int i = 0; i < flow1.Length; i++)
        {
            flow1[i].SetActive(i == index);
        }
    }

    [System.Obsolete]
    public void SetActiveFlow2(int index)
    {

        if (index == flow2.Length)
        {
            Application.LoadLevel(1);
            return;
        }

        for (int i = 0; i < flow2.Length; i++)
        {
            flow2[i].SetActive(i == index);
        }
    }
}
