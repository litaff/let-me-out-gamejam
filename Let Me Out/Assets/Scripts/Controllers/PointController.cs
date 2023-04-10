using System;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class PointController : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueDisplay;
        [SerializeField] private GameObject levelParent;
        private int totalPoints;
        private int gainedPoints;
        private AudioSource audioSource;

        public static event Action OnAllPickUp;
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            totalPoints = 0;
            gainedPoints = 0;
            foreach (var point in levelParent.GetComponentsInChildren<Point>())
            {
                if(!point) continue;

                totalPoints++;
            }

            Point.OnPickUp += OnPointGained;
            valueDisplay.text = $"{gainedPoints}/{totalPoints}";
        }

        private void OnPointGained()
        {
            gainedPoints++;
            audioSource.Play();
            valueDisplay.text = $"{gainedPoints}/{totalPoints}";
            if (gainedPoints < totalPoints) return;
            
            OnAllPickUp?.Invoke();
        }
    }
}
