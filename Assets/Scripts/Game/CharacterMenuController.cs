using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomizationUI : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Change to your main menu scene name
    }
}
