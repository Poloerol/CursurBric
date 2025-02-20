using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json.Serialization;

public class CardPictureBox : PictureBox
{
    private bool isSelected;
    private bool isHighlighted;
    private int originalTop;
    private readonly Color highlightColor = Color.Yellow;

    [JsonPropertyName("isSelected")]
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            UpdateAppearance();
        }
    }

    [JsonPropertyName("isHighlighted")]
    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            isHighlighted = value;
            UpdateAppearance();
        }
    }

    public CardPictureBox()
    {
        this.Paint += CardPictureBox_Paint;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        originalTop = this.Top;
        this.Top -= 10;
        this.BringToFront();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        this.Top = originalTop;
    }

    private void UpdateAppearance()
    {
        this.Invalidate(); // Yeniden çizim için
    }

    private void CardPictureBox_Paint(object? sender, PaintEventArgs e)
    {
        base.OnPaint(e);

        if (isSelected)
        {
            using Pen pen = new(Color.Blue, 2);
            e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }

        if (isHighlighted)
        {
            using Pen pen = new(highlightColor, 2);
            e.Graphics.DrawRectangle(pen, 2, 2, this.Width - 5, this.Height - 5);
        }
    }
}