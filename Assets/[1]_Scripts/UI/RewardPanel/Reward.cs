using UnityEngine;

namespace BrakeBricks
{
    public class Reward : MonoBehaviour
    {
        public void GetReward()
        {
            InputManager.Instance.GetReward(GameManager.Instance.RewardCount);
        }
    }
}
