using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelBookmarks(List<BookmarkData> bookmarks)
    {
        public RDLevelBookmarks() : this(null) { }

        public readonly IReadOnlyList<BookmarkData> Bookmarks = bookmarks.AsReadOnly();

        public static implicit operator List<BookmarkData>(RDLevelBookmarks instance)
            => instance.Bookmarks != null ? [.. instance.Bookmarks] : [];

        public static implicit operator RDLevelBookmarks(List<BookmarkData> list) => new(list);
    }
}
