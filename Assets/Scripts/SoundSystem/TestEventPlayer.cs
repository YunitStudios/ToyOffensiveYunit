using UnityEngine;

public class TestEventPlayer : MonoBehaviour
{
    public void TestSound()
    {
        AkSoundEngine.PostEvent("PlayTestSound", gameObject);
    }
}
