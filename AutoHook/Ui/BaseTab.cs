using System;

namespace AutoHook.Ui;

public abstract class BaseTab : IDisposable
{
    public abstract string TabName { get; }
    public abstract bool Enabled { get; }

    public abstract void DrawHeader();

    public abstract void Draw();

    public virtual void Dispose()
    {
    }
}