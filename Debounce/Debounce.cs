namespace ExtendedThreading;

/// <summary>
/// A thread-safe mechanism that ensures an operation is not active more than once at a time.
/// </summary>
public sealed class Debounce {
#if NET9_0_OR_GREATER
    /// <summary>
    /// A byte that stores whether this debounce is currently active.
    /// </summary>
    private byte IsActiveByte = 0;
#else
    /// <summary>
    /// An int that stores whether this debounce is currently active.
    /// </summary>
    private int IsActiveByte = 0;
#endif

    /// <summary>
    /// Returns <see langword="true"/> if the debounce is currently active.
    /// <br/><br/>
    /// This property should not be used for synchronization, because it may become outdated immediately after it is read.
    /// </summary>
    public bool IsActive => Volatile.Read(ref IsActiveByte) != 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Debounce"/> class.
    /// </summary>
    public Debounce() {
    }
    /// <summary>
    /// Activates the debounce and returns a <see cref="DebounceDisposable"/> that deactivates the debounce when disposed.
    /// <br/>
    /// If the debounce is already active, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public DebounceDisposable Activate() {
        if (!TryActivate()) {
            throw new InvalidOperationException("Unable to activate debounce because it is already active.");
        }
        return new DebounceDisposable(this);
    }
    /// <summary>
    /// If the debounce is inactive, activates the debounce and returns <see langword="true"/>.
    /// <br/>
    /// Otherwise, returns <see langword="false"/>.
    /// </summary>
    public bool TryActivate() {
        if (Interlocked.CompareExchange(ref IsActiveByte, 1, 0) != 0) {
            return false;
        }
        return true;
    }
    /// <summary>
    /// If the debounce is inactive, activates the debounce, sets <paramref name="debounceDisposable"/> to a
    /// <see cref="DebounceDisposable"/> that deactivates the debounce when disposed, and returns <see langword="true"/>.
    /// <br/>
    /// Otherwise, sets <paramref name="debounceDisposable"/> to <see langword="default"/> and returns <see langword="false"/>.
    /// </summary>
    /// <param name="debounceDisposable">
    /// A struct that deactivates the debounce when disposed.
    /// </param>
    public bool TryActivate(out DebounceDisposable debounceDisposable) {
        if (!TryActivate()) {
            debounceDisposable = default;
            return false;
        }
        debounceDisposable = new DebounceDisposable(this);
        return true;
    }
    /// <summary>
    /// Deactivates the debounce.
    /// <br/>
    /// If the debounce is already inactive, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public void Deactivate() {
        if (!TryDeactivate()) {
            throw new InvalidOperationException("Unable to deactivate debounce because it is already inactive.");
        }
    }
    /// <summary>
    /// If the debounce is active, deactivates the debounce and returns <see langword="true"/>.
    /// <br/>
    /// Otherwise, returns <see langword="false"/>.
    /// </summary>
    public bool TryDeactivate() {
        if (Interlocked.CompareExchange(ref IsActiveByte, 0, 1) != 1) {
            return false;
        }
        return true;
    }
}

/// <summary>
/// A struct that deactivates a <see cref="Debounce"/> when disposed.
/// </summary>
public readonly struct DebounceDisposable : IDisposable {
    /// <summary>
    /// The <see cref="Debounce"/> that is deactivated when this struct is disposed.
    /// </summary>
    public Debounce? DebounceToDeactivate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceDisposable"/> struct.
    /// </summary>
    /// <param name="debounceToDeactivate">
    /// The debounce to deactivate when this struct is disposed.
    /// </param>
    public DebounceDisposable(Debounce? debounceToDeactivate) {
        DebounceToDeactivate = debounceToDeactivate;
    }
    /// <summary>
    /// Deactivates <see cref="DebounceToDeactivate"/> if not <see langword="null"/>.
    /// </summary>
    public void Dispose() {
        DebounceToDeactivate?.Deactivate();
    }
}