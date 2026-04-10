using UnityEngine;
using QFramework;
using TMPro;

namespace SkateGame
{
    public class UiTimer : ViewerControllerBase
    {
        public TextMeshProUGUI timerText;

        protected override void InitializeController()
        {
            this.RegisterEvent<TimerUpdatedEvent>(OnTimerUpdated);
            UpdateDisplay(0f);
        }

        private void OnTimerUpdated(TimerUpdatedEvent e)
        {
            UpdateDisplay(e.ElapsedTime);
        }

        private void UpdateDisplay(float elapsed)
        {
            if (timerText == null) return;

            int minutes = (int)(elapsed / 60f);
            int seconds = (int)(elapsed % 60f);
            int milliseconds = (int)((elapsed * 100f) % 100f);

            timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }

        protected override void OnRealTimeUpdate()
        {
        }

        protected override void OnDestroy()
        {
            this.UnRegisterEvent<TimerUpdatedEvent>(OnTimerUpdated);
            base.OnDestroy();
        }
    }
}
