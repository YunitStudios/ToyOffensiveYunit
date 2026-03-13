using UnityEngine;

public class GloryKillAnimationReceiver : MonoBehaviour
{
    // Receiver for animation events related to glory kills
    public GloryKill gloryKill;

    public void OnGloryKillFinished()
    {
        gloryKill.OnGloryKillFinished();
    }
}
