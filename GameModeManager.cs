using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [Header("First Person")]
    public GameObject fpsController;
    public Camera fpsCamera;

    [Header("Card Game")]
    public GameObject cardGameCanvas;
    public Camera cardGameCamera;

    private bool _inCardGame = false;
    private PlayerController _playerController;

    void Start()
    {
        _playerController = fpsController.GetComponent<PlayerController>();
        cardGameCamera.enabled = false;
    }

    public void ToggleMode()
    {
        _inCardGame = !_inCardGame;

        fpsCamera.enabled = !_inCardGame;
        cardGameCamera.enabled = _inCardGame;

        fpsController.GetComponent<CharacterController>().enabled = !_inCardGame;
        _playerController.enabled = !_inCardGame;

        Cursor.lockState = _inCardGame ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _inCardGame;

        cardGameCanvas.SetActive(_inCardGame);
    }

    public void EnterCardGame()
    {
        if (!_inCardGame) ToggleMode();
    }

    public void ExitCardGame()
    {
        if (_inCardGame) ToggleMode();
    }
}