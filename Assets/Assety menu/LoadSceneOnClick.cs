using UnityEngine;
using UnityEngine.SceneManagement; // To jest kluczowy import dla menedżera scen

public class LoadSceneOnClick : MonoBehaviour
{
    // Ta metoda zostanie wywołana przez przycisk. Możesz podać nazwę sceny.
    public void LoadByName(string sceneName)
    {
        // Ładuje scenę o podanej nazwie (np. "MainLevel")
        SceneManager.LoadScene(sceneName);
    }

    // Ta metoda zostanie wywołana przez przycisk. Możesz podać numer indeksu sceny.
    public void LoadByIndex(int sceneIndex)
    {
        // Ładuje scenę o podanym indeksie (np. 1)
        SceneManager.LoadScene(sceneIndex);
    }
}