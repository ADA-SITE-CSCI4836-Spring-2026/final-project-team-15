using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-installs itself on a GameObject named "FinishPoint" and shows the
/// finish overlay when the player reaches it.
/// </summary>
public class FinishPointTrigger : MonoBehaviour
{
    private Collider _finishCollider;
    private Transform _player;
    private CharacterController _playerController;
    private bool _finished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallAfterSceneLoad()
    {
        SceneManager.sceneLoaded += (_, __) => Install();
        Install();
    }

    private static void Install()
    {
        GameObject finishPoint = GameObject.Find("FinishPoint");
        if (finishPoint == null || finishPoint.GetComponent<FinishPointTrigger>() != null) return;

        finishPoint.AddComponent<FinishPointTrigger>();
    }

    private void Awake()
    {
        _finishCollider = GetComponent<Collider>();
        if (_finishCollider != null) _finishCollider.isTrigger = true;

        FindPlayer();
    }

    private void Update()
    {
        if (_finished) return;

        if (_player == null) FindPlayer();
        if (_player == null) return;

        if (IsPlayerInsideFinishPoint())
            Finish();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_finished || !IsPlayer(other.transform)) return;

        Finish();
    }

    private void FindPlayer()
    {
        PlayerHealth health = FindObjectOfType<PlayerHealth>();
        if (health != null)
        {
            _player = health.transform;
            _playerController = health.GetComponent<CharacterController>();
            return;
        }

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject == null) return;

        _player = playerObject.transform;
        _playerController = playerObject.GetComponent<CharacterController>();
    }

    private bool IsPlayerInsideFinishPoint()
    {
        if (_finishCollider == null)
            return Vector3.Distance(transform.position, _player.position) <= 2f;

        if (_playerController != null)
            return _finishCollider.bounds.Intersects(_playerController.bounds);

        return _finishCollider.bounds.Contains(_player.position);
    }

    private bool IsPlayer(Transform other)
    {
        if (other == null) return false;
        if (_player != null && other == _player) return true;
        return other.GetComponentInParent<PlayerHealth>() != null || other.name == "Player";
    }

    private void Finish()
    {
        _finished = true;
        LossScreenOverlay.ShowFinish();
    }
}
