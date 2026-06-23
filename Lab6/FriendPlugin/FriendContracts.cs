namespace FriendContracts;

/// <summary>
/// IFriendProcessor — the interface used by a classmate's plugin project.
///
/// NOTE: This interface is intentionally DIFFERENT from our IProcessorPlugin:
///   • Method names differ  (Transform / Restore  vs  ProcessBeforeSave / ProcessAfterLoad)
///   • Configuration uses a single Configure(string) instead of key-value pairs
///   • No IsConfigurable, Version, or Description properties
///
/// The Adapter pattern bridges this foreign interface to our IProcessorPlugin
/// WITHOUT modifying either interface.
/// </summary>
public interface IFriendProcessor
{
    /// <summary>Unique name of this processor.</summary>
    string Name { get; }

    /// <summary>
    /// Transforms content before it is written to a file.
    /// Equivalent to our ProcessBeforeSave().
    /// </summary>
    string Transform(string input);

    /// <summary>
    /// Restores content after it is read from a file.
    /// Equivalent to our ProcessAfterLoad().
    /// </summary>
    string Restore(string input);

    /// <summary>
    /// Accepts a single freeform configuration string.
    /// Equivalent to our ApplySetting() but uses a single string blob.
    /// </summary>
    void Configure(string configString);

    /// <summary>Returns current configuration as a freeform string.</summary>
    string GetConfig();
}
