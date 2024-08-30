using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playerHealth : MonoBehaviour
{
    [SerializeField]
    private int health;

    [SerializeField]
    private TextMeshProUGUI healthText;

    private void Start()
    {
        if (!ConfigUIController.IsPlayerHP)
        {
            healthText.enabled = false;
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ConfigUIController.IsPlayerHP)
        {
            return;
        }

        if (other.gameObject.CompareTag("Enemy"))
        {
            health += -1;
            healthText.SetText("HP:" +  health.ToString());

            if (health <= 0)
            {
                health = 0;
                healthText.SetText("HP:" + health.ToString());
                SceneManager.LoadScene("GameOverScene");
            }
        }
    }
}
