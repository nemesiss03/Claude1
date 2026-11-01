using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>
    /// Migration-only native type for hidden caches created by early releases.
    /// ProjectWindowDrawer disposes those objects; new drawing state is managed.
    /// Do not change this type's base class or create new instances in production.
    /// </summary>
    internal sealed class FolderProjectWindowDrawerCache : ScriptableObject { }
}
