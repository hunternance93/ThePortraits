using UnityEngine;

public class DevCommentaryInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Message displayed when interacted")]
    [SerializeField] private string message = "";
    [Tooltip("(Optional) How long message should display for")]
    [SerializeField] private float messageLength = -1;
    [Tooltip("(Optional) Should a sound play when you inspect this?")]
    [SerializeField] private AudioSource aud = null;

    private Vector3 rotation = new Vector3(0, 50, 0);
    private const float _bounceDistance = .1f;
    private const float _bounceDuration = 3;
    private float bounceTimer = _bounceDuration / 2;
    private float bounceCurrentHeight = 0;
    private bool bouncingUp = true;

    private void Update()
    {
        if (aud != null && aud.isPlaying)
        {
            transform.Rotate(rotation * Time.deltaTime);
            if (bouncingUp)
            {
                bounceTimer += Time.deltaTime;
                float targetHeight = Mathf.Lerp(-_bounceDistance, _bounceDistance, bounceTimer / _bounceDuration);
                if (bounceTimer >= _bounceDuration)
                {
                    targetHeight = _bounceDistance;
                    bouncingUp = false;
                    bounceTimer = 0;
                }
                float deltaY = targetHeight - bounceCurrentHeight;
                bounceCurrentHeight = targetHeight;
                transform.position += new Vector3(0, deltaY, 0);
            }
            else
            {
                bounceTimer += Time.deltaTime;
                float targetHeight = Mathf.Lerp(_bounceDistance, -_bounceDistance, bounceTimer / _bounceDuration);
                if (bounceTimer >= _bounceDuration)
                {
                    targetHeight = -_bounceDistance;
                    bouncingUp = true;
                    bounceTimer = 0;
                }
                float deltaY = targetHeight - bounceCurrentHeight;
                bounceCurrentHeight = targetHeight;
                transform.position += new Vector3(0, deltaY, 0);
            }
        }
    }

    public void Interacted()
    {

        if (aud != null && !aud.isPlaying)
        {
            AudioSource[] devCommentaryAudio = transform.parent.parent.GetComponentsInChildren<AudioSource>();
            foreach(AudioSource audio in devCommentaryAudio)
            {
                audio.Stop();
            }

            if (messageLength < 0) GameManager.instance.DisplayMessage(message);
            else GameManager.instance.DisplayMessage(message, messageLength);
            aud.Play();
        }
        else if (aud != null) aud.Stop();
    }
}
