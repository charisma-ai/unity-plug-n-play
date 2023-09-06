using CharismaSDK.PlugNPlay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleSceneManager : MonoBehaviour
{
    [SerializeField]
    private PlaythroughInstance _playthrough;

    [SerializeField]
    private Animator _animator;

    private bool _requestToFade;

    // Start is called before the first frame update
    void Start()
    {
        // Begin loading playthrough.
        _playthrough.LoadPlaythrough();
    }

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
