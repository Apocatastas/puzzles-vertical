using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MathSolver : MonoBehaviour
{
    public Text[] buttons;
    public Text numeroUno;
    public Text numeroDuo;
   // public Text price;
    public GameObject m_hellointerface;
    public GameObject m_epinterface;
    List<int> m_unsortIndexes;
    List<int> m_sortIndexes;
    int one;
    int two;
    int correctAnswer;

    private void PlaceAnswers(string[] answers)
    {
        m_unsortIndexes = new List<int>();
        m_sortIndexes = new List<int>();
        //implementing Fisher-Yates algorithm
        for (int i = 0; i < 3; i++)
        {
            m_unsortIndexes.Add(i);
        }

        int numHolder;
        int counter = 3;
        int j = 0;
        while (m_sortIndexes.Count < counter)
        {
            if (j < counter)
            {
                numHolder = Mathf.CeilToInt(Random.Range(0, m_unsortIndexes.Count));
                m_sortIndexes.Add(m_unsortIndexes[numHolder]);
                m_unsortIndexes.Remove(m_unsortIndexes[numHolder]);
                j++;
            }

        }

        //Place answers
        for (int k = 0; k < m_sortIndexes.Count; k++)
        {
            buttons[m_sortIndexes[k]].text = answers[k];
        }



    }

    public IEnumerator Checker2()
    {
        yield return new WaitForSeconds(1f);
     //   Debug.Log("CHECKER WORKER COMPARE " + Broadcaster.ParentalButton + " with " + correctAnswer.ToString());
        if (correctAnswer.ToString() == Broadcaster.ParentalButton)
        {
     //       Debug.Log("PUSHED CORRECT");
            System.Collections.Generic.Dictionary<string, object> demoOptions2 = new System.Collections.Generic.Dictionary<string, object>() { { "ViewEP_From", "onboard" } };
            Amplitude.Instance.logEvent("Onboarding_buy", demoOptions2);
            m_hellointerface.SetActive(false);
            m_epinterface.SetActive(true);
            //price.text = Broadcaster.Week;
        }
        else
        {
       //     Debug.Log("PUSHED INCORRECT");
            MakeExpression();

        }

    }

    public void Checker()
    {
        StartCoroutine("Checker2");

    }

    private string[] GenerateAnswers(int correctAnswer)
    {
        string[] answers = new string[3];
        answers[0] = correctAnswer.ToString();
     //   Debug.Log("ANSWER 0 " + answers[0]);
        answers[1] = (correctAnswer + Mathf.CeilToInt(Random.Range(3, 10))).ToString();
     //   Debug.Log("ANSWER 1 " + answers[1]);
        answers[2] = (correctAnswer - Mathf.CeilToInt(Random.Range(3, 10))).ToString();
     //   Debug.Log("ANSWER 2 " + answers[2]);
        return answers;
    }

    private void MakeExpression()
    {
       
        one = Mathf.CeilToInt(Random.Range(3, 12));
      //  Debug.Log("one " + one);
       
        two = Mathf.CeilToInt(Random.Range(3, 12));
     //   Debug.Log("two " + two);
        correctAnswer = one + two;
     //   Debug.Log("CORRECT ANSWER " + correctAnswer);
        
        string correctAnswerString = correctAnswer.ToString();
        numeroUno.text = one.ToString();
        numeroDuo.text = two.ToString();
        PlaceAnswers(GenerateAnswers(correctAnswer));
        
    }

    private void Start()
    {
        MakeExpression();
    }
}
