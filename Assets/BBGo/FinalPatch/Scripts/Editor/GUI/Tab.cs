using UnityEngine;

namespace BBGo
{
    public class Tab
    {
        public GUIContent Title { get; }
        public ITabView TabView { get; }
        public Vector2 ScrollRect { get; set; }

        public Tab() : this(null) { }

        public Tab(ITabView tabView) : this(null, null, tabView) { }

        public Tab(string label, Texture icon, ITabView tabView)
        {
            Title = new GUIContent(label, icon);
            TabView = tabView;
        }

        public void Dispose()
        {
            TabView?.OnDispose();
        }
    }
}