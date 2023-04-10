using TMPro;
using UnityEngine;

namespace Controllers
{
    public class VictoryController : MonoBehaviour
    {
        [SerializeField] private TMP_Text victoryText;
        private ParticleSystem particles;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            victoryText.enabled = false;
            PointController.OnAllPickUp += Victory;
        }

        private void Victory()
        {
            particles.Play();
            victoryText.enabled = true;
        }
    }
}
