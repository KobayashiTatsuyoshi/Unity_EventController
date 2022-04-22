public class ReactiveProperty<T> where T : unmanaged
{
    private T m_value;
    public T Value {
        get => m_value;
        set {
            var prev = m_value;
            m_value = value;
            if (OnChanged != null && !prev.Equals(m_value))
            {
                OnChanged(m_value);
            }
        }
    }

    public event System.Action<T> OnChanged;
}