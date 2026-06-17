using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;

namespace GravitySim;

internal sealed class Ball
{
    public Vector2 Position;
    public Vector2 Velocity;
    public readonly float Radius;
    public readonly Color Color;

    public Ball(Vector2 position, Vector2 velocity, float radius, Color color)
    {
        Position = position;
        Velocity = velocity;
        Radius = radius;
        Color = color;
    }
}

internal sealed class SimulationForm : Form
{
    private const float Gravity = 600f;
    private const float Restitution = 0.85f;
    private const int BallCount = 16;
    private const float MaxWindowAcceleration = 6000f;
    private const float ContainerLineWidth = 6f;
    private static readonly Color BackgroundColor = Color.FromArgb(40, 40, 40);

    private readonly List<Ball> balls = new();
    private readonly Timer timer;
    private readonly Stopwatch stopwatch = new();
    private readonly Vector2 containerCenter;
    private readonly float containerRadius;

    private Point lastWindowLocation;
    private Vector2 windowVelocity = Vector2.Zero;

    public SimulationForm()
    {
        Text = "Gravity Simulator";
        ClientSize = new Size(800, 800);
        DoubleBuffered = true;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        containerCenter = new Vector2(ClientSize.Width / 2f, ClientSize.Height / 2f);
        containerRadius = Math.Min(ClientSize.Width, ClientSize.Height) / 2f - 20f;

        SpawnBalls();

        lastWindowLocation = Location;

        timer = new Timer { Interval = 16 };
        timer.Tick += (_, _) => Step();
        stopwatch.Start();
        timer.Start();
    }

    private void SpawnBalls()
    {
        var rng = new Random();
        for (int i = 0; i < BallCount; i++)
        {
            float radius = rng.Next(10, 24);
            float maxOffset = containerRadius - radius - 5f;
            double angle = rng.NextDouble() * Math.PI * 2;
            double dist = rng.NextDouble() * maxOffset;

            var position = containerCenter + new Vector2(
                (float)(Math.Cos(angle) * dist),
                (float)(Math.Sin(angle) * dist));

            var velocity = new Vector2(rng.Next(-150, 150), rng.Next(-150, 150));

            balls.Add(new Ball(position, velocity, radius, Color.White));
        }
    }

    private void Step()
    {
        float dt = (float)stopwatch.Elapsed.TotalSeconds;
        stopwatch.Restart();
        dt = Math.Min(dt, 0.033f);
        if (dt <= 0f) return;

        Vector2 windowAcceleration = MeasureWindowAcceleration(dt);

        foreach (var ball in balls)
        {
            ball.Velocity += new Vector2(0, Gravity * dt);
            ball.Velocity -= windowAcceleration * dt;
            ball.Position += ball.Velocity * dt;
            ResolveContainerCollision(ball);
        }

        ResolveBallCollisions();

        Invalidate();
    }

    private Vector2 MeasureWindowAcceleration(float dt)
    {
        Point currentLocation = Location;
        var currentWindowVelocity = new Vector2(
            currentLocation.X - lastWindowLocation.X,
            currentLocation.Y - lastWindowLocation.Y) / dt;
        lastWindowLocation = currentLocation;

        Vector2 windowAcceleration = (currentWindowVelocity - windowVelocity) / dt;
        windowVelocity = currentWindowVelocity;

        float accelerationMagnitude = windowAcceleration.Length();
        if (accelerationMagnitude > MaxWindowAcceleration)
        {
            windowAcceleration *= MaxWindowAcceleration / accelerationMagnitude;
        }

        return windowAcceleration;
    }

    private void ResolveContainerCollision(Ball ball)
    {
        Vector2 offset = ball.Position - containerCenter;
        float distance = offset.Length();
        float maxDistance = containerRadius - ball.Radius;

        if (distance > maxDistance && distance > 0f)
        {
            Vector2 normal = offset / distance;
            ball.Position = containerCenter + normal * maxDistance;

            float velocityAlongNormal = Vector2.Dot(ball.Velocity, normal);
            if (velocityAlongNormal > 0)
            {
                ball.Velocity -= normal * velocityAlongNormal * (1f + Restitution);
            }
        }
    }

    private void ResolveBallCollisions()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                Ball a = balls[i];
                Ball b = balls[j];

                Vector2 delta = b.Position - a.Position;
                float distance = delta.Length();
                float minDistance = a.Radius + b.Radius;

                if (distance < minDistance && distance > 0f)
                {
                    Vector2 normal = delta / distance;
                    float overlap = minDistance - distance;

                    a.Position -= normal * (overlap / 2f);
                    b.Position += normal * (overlap / 2f);

                    Vector2 relativeVelocity = b.Velocity - a.Velocity;
                    float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

                    if (velocityAlongNormal < 0)
                    {
                        float massA = a.Radius * a.Radius;
                        float massB = b.Radius * b.Radius;
                        float impulse = -(1f + Restitution) * velocityAlongNormal / (1f / massA + 1f / massB);
                        Vector2 impulseVector = normal * impulse;

                        a.Velocity -= impulseVector / massA;
                        b.Velocity += impulseVector / massB;
                    }
                }
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackgroundColor);

        using var containerPen = new Pen(Color.White, ContainerLineWidth);
        g.DrawEllipse(
            containerPen,
            containerCenter.X - containerRadius,
            containerCenter.Y - containerRadius,
            containerRadius * 2,
            containerRadius * 2);

        foreach (var ball in balls)
        {
            using var brush = new SolidBrush(ball.Color);
            g.FillEllipse(
                brush,
                ball.Position.X - ball.Radius,
                ball.Position.Y - ball.Radius,
                ball.Radius * 2,
                ball.Radius * 2);
        }
    }
}

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SimulationForm());
    }
}
