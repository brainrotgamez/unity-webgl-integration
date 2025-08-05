using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public int score;
    public TextMeshProUGUI scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        score = 0;
        scoreText.text = score.ToString();
    }

    public void AddScore(int value=1)
    {
        Debug.Log("Adding score: " + value);
        score += value;
        scoreText.text = score.ToString();
        GameObject.FindObjectOfType<SessionManager>().CallReportScore(score, "{}", false);
        Debug.Log("Score: " + score);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
