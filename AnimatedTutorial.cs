using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;


public class AnimatedTutorial
{
    private Form tutorialForm;
    private PictureBox animationBox;
    private Label descriptionLabel;
    private List<TutorialStep> steps;
    private int currentStepIndex;

    private class TutorialStep
    {
        [JsonPropertyName("animation")]
        [JsonProperty("animation")]
        public required Image Animation { get; set; } // 'required' anahtar kelimesi kaldırıldı

        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public required string Description { get; set; } // 'required' anahtar kelimesi kaldırıldı

        [JsonPropertyName("duration")]
        [JsonProperty("duration")]
        public int Duration { get; set; }
    }

    public AnimatedTutorial()
    {
        tutorialForm = new Form(); // Initialize tutorialForm here
        InitializeSteps();
    }

    private void InitializeSteps()
    {
        steps = new List<TutorialStep>
        {
            new() {
                Animation = /* Set the animation image here, e.g., LoadImage("path/to/image") */,
                Description = "Kartlar dağıtılır ve her oyuncu 13 kart alır.",
                Duration = 3000
            },
            new() {
                Animation = /* Set the animation image here */,
                Description = "Deklare aşaması başlar. Güney oyuncusu ilk teklifi verir.",
                Duration = 2500
            },
            // Diğer adımlar...
        };
    }

    private static Image LoadImage(string path)
    {
        return Image.FromFile(path); // Adjust this method as needed
    }

    public async Task ShowTutorial()
    {
        tutorialForm = new Form
        {
            Text = "Animasyonlu Öğretici",
            Size = new Size(800, 600),
            StartPosition = FormStartPosition.CenterScreen
        };

        animationBox = new PictureBox
        {
            Size = new Size(600, 400),
            Location = new Point(100, 50),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        descriptionLabel = new Label
        {
            Location = new Point(100, 460),
            Size = new Size(600, 60),
            Font = new Font("Arial", 12),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var nextButton = new Button
        {
            Text = "İleri",
            Location = new Point(600, 520),
            Size = new Size(100, 30)
        };
        nextButton.Click += async (s, e) => await ShowNextStep();

        var prevButton = new Button
        {
            Text = "Geri",
            Location = new Point(100, 520),
            Size = new Size(100, 30)
        };
        prevButton.Click += async (s, e) => await ShowPreviousStep();

        tutorialForm.Controls.AddRange(
        new Control[] { animationBox, descriptionLabel, nextButton, prevButton });

        currentStepIndex = 0;
        await ShowCurrentStep();
        tutorialForm.ShowDialog();
    }

    private async Task ShowCurrentStep()
    {
        var step = steps[currentStepIndex];
        animationBox.Image = step.Animation;
        descriptionLabel.Text = step.Description;
        await Task.Delay(step.Duration);
    }

    private async Task ShowNextStep()
    {
        if (currentStepIndex < steps.Count - 1)
        {
            currentStepIndex++;
            await ShowCurrentStep();
        }
    }

    private async Task ShowPreviousStep()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            await ShowCurrentStep();
        }
    }
}