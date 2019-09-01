namespace BBGo
{
    public class TabSet
    {
        public event System.Action<int> OnActiveIndexChanged;

        private int m_activeIndex;
        public int ActiveIndex
        {
            get { return m_activeIndex; }
            set
            {
                if (m_activeIndex != value)
                {
                    m_activeIndex = value;
                    OnActiveIndexChanged?.Invoke(value);
                }
            }
        }
        public Tab[] Tabs { get; }
        public Tab ActiveTab => Tabs[ActiveIndex];

        public TabSet(Tab[] tabs, int defaultTab = 0)
        {
            Tabs = tabs;
            ActiveIndex = defaultTab;
        }
    }
}