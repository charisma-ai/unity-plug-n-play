using CharismaSDK.PlugNPlay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SimpleSceneManager : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    private bool _requestToFade;

    private void Update()
    {
        if (_requestToFade)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnResetHit();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void RequestFadeToRestartScreen()
    {
        _requestToFade = true;
        _animator.SetBool("FadeOut", true);
    }

    private void OnResetHit()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
