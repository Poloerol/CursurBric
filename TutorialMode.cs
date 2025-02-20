using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CursurBric // Replace 'YourNamespace' with an appropriate name
{
    public class TutorialMode
    {
        public enum TutorialStep
        {
            Introduction,
            Bidding,
            Playing,
            Scoring,
            Advanced
        }

        private TutorialStep currentStep;
        private Dictionary<TutorialStep, List<string>> tutorials;
        private Form? tutorialForm;
        private Label tutorialText;
        private Button nextButton;
        private Button previousButton;
        private readonly List<TutorialStep> steps; // Declare the steps field

        public TutorialMode()
        {
            InitializeTutorials();
            currentStep = TutorialStep.Introduction;
            steps = []; // Initialize the steps field
        }

        private void InitializeTutorials()
        {
            tutorials = new Dictionary<TutorialStep, List<string>>
            {
                [TutorialStep.Introduction] =
                [
                    "Briç, 4 oyuncunun oynadığı bir kart oyunudur.",
                    "Her oyuncuya 13 kart dağıtılır.",
                    "Oyun iki aşamadan oluşur: Deklare ve Oyun.",
                ],
                [TutorialStep.Bidding] =
                [
                    "Deklare aşamasında her oyuncu sırayla teklif verir.",
                    "Teklifler 1-7 arasında bir sayı ve renk içerir.",
                    "Kontr ve Sürkontr özel tekliflerdir.",
                ],
                // Diğer aşamalar için benzer açıklamalar...
            };
        }

        public void ShowTutorial()
        {
            tutorialForm = new Form
            {
                Text = "Briç Öğretici",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterScreen
            };

            tutorialText = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(360, 180),
                Text = GetCurrentTutorialText()
            };

            nextButton = new Button
            {
                Text = "İleri",
                Location = new Point(280, 220),
                Size = new Size(80, 30)
            };
            nextButton.Click += (s, e) => NextStep();

            previousButton = new Button
            {
                Text = "Geri",
                Location = new Point(180, 220),
                Size = new Size(80, 30)
            };
            previousButton.Click += (s, e) => PreviousStep();

            tutorialForm.Controls.AddRange([tutorialText, nextButton, previousButton]);
            tutorialForm.ShowDialog();
        }

        private string GetCurrentTutorialText()
        {
            return string.Join("\n\n", tutorials[currentStep]);
        }

        private void NextStep()
        {
            if (currentStep < TutorialStep.Advanced)
            {
                currentStep++;
                tutorialText.Text = GetCurrentTutorialText();
                UpdateButtonStates();
            }
        }

        private void PreviousStep()
        {
            if (currentStep > TutorialStep.Introduction)
            {
                currentStep--;
                tutorialText.Text = GetCurrentTutorialText();
                UpdateButtonStates();
            }
        }

        private void UpdateButtonStates()
        {
            previousButton.Enabled = currentStep > TutorialStep.Introduction;
            nextButton.Enabled = currentStep < TutorialStep.Advanced;
        }
    }
}