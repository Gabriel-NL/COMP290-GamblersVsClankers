using UnityEngine;

public class Coin : MonoBehaviour
{
   

    private void OnMouseDown()
    {
        if (ScoreManager.instance == null)
        {
           
            return;
        }

        if (gameObject.CompareTag("Bronze"))
        {
            ScoreManager.instance.AddPoints(10);
            Destroy(gameObject);
            Debug.Log("Bronze coin clicked!");
        }
        else if (gameObject.CompareTag("Silver"))
        {
            ScoreManager.instance.AddPoints(100);
            Destroy(gameObject);
            Debug.Log("Silver coin clicked!");
        }
        else if (gameObject.CompareTag("Gold"))
        {
            ScoreManager.instance.AddPoints(1000);
            Destroy(gameObject);
            Debug.Log("Gold coin clicked!");
        }
       
    }
}


