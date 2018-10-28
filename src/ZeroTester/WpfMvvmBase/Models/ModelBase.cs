// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

namespace Agebull.EntityModel
{
    /// <summary>
    ///     MVVM的Model的基类
    /// </summary>
    public abstract class ModelBase : MvvmBase
    {
        /// <summary>
        /// VM
        /// </summary>
        public ViewModelBase ViewModel { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            DoInitialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void DoInitialize()
        {
            
        }
    }

    /*
    public class Events
    {
        private static readonly DependencyProperty EventBehaviorsProperty =
                DependencyProperty.RegisterAttached(
                                                    "EventBehaviors",
                        typeof (EventBehaviorCollection),
                        typeof (Control),
                        null);

        private static readonly DependencyProperty InternalDataContextProperty =
                DependencyProperty.RegisterAttached(
                                                    "InternalDataContext",
                        typeof (Object),
                        typeof (Control),
                        new PropertyMetadata(null, DataContextChanged));

        private static void DataContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var target = dependencyObject as Control;
            if (target == null)
                return;

            foreach (var behavior in GetOrCreateBehavior(target))
                behavior.Bind();
        }

        public static readonly DependencyProperty CommandsProperty =
                DependencyProperty.RegisterAttached(
                                                    "Commands",
                        typeof (EventCommandCollection),
                        typeof (Events),
                        new PropertyMetadata(null, CommandsChanged));

        public static EventCommandCollection GetCommands(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(CommandsProperty) as EventCommandCollection;
        }

        public static void SetCommands(DependencyObject dependencyObject, EventCommandCollection eventCommands)
        {
            dependencyObject.SetValue(CommandsProperty, eventCommands);
        }

        private static void CommandsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var target = dependencyObject as Control;
            if (target == null)
                return;

            var behaviors = GetOrCreateBehavior(target);
            foreach (var eventCommand in e.NewValue as EventCommandCollection)
            {
                var behavior = new EventBehavior(target);
                behavior.Bind(eventCommand);
                behaviors.Add(behavior);
            }

        }

        private static EventBehaviorCollection GetOrCreateBehavior(FrameworkElement target)
        {
            var behavior = target.GetValue(EventBehaviorsProperty) as EventBehaviorCollection;
            if (behavior == null)
            {
                behavior = new EventBehaviorCollection();
                target.SetValue(EventBehaviorsProperty, behavior);
                target.SetBinding(InternalDataContextProperty, new Binding());
            }

            return behavior;
        }
    }

    public class EventCommand
    {
        public string CommandName
        {
            get;
            set;
        }

        public string EventName
        {
            get;
            set;
        }
    }

    public class EventCommandCollection : List<EventCommand>
    {
    }

    public class EventBehavior : Behavior<Control>
    {
        private EventCommand _bindingInfo;

        public EventBehavior(Control control)
                : base()
        {

        }

        public void Bind(EventCommand bindingInfo)
        {
            ValidateBindingInfo(bindingInfo);

            _bindingInfo = bindingInfo;

            Bind();
        }

        private void ValidateBindingInfo(EventCommand bindingInfo)
        {
            if (bindingInfo == null)
                throw new ArgumentException("bindingInfo");
            if (string.IsNullOrEmpty(bindingInfo.CommandName))
                throw new ArgumentException("bindingInfo.CommandName");
            if (string.IsNullOrEmpty(bindingInfo.EventName))
                throw new ArgumentException("bindingInfo.EventName");
        }

        public void Bind()
        {
            ValidateBindingInfo(_bindingInfo);
            HookPropertyChanged();
            HookEvent();
            SetCommand();
        }

        public void HookPropertyChanged()
        {
            var dataContext = this.AssociatedObject.DataContext as INotifyPropertyChanged;
            if (dataContext == null)
                return;

            dataContext.PropertyChanged -= DataContextPropertyChanged;
            dataContext.PropertyChanged += DataContextPropertyChanged;
        }

        private void DataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _bindingInfo.CommandName)
                SetCommand();
        }

        private void SetCommand()
        {
            var dataContext = TargetObject.DataContext;
            if (dataContext == null)
                return;

            var propertyInfo = dataContext.GetType().GetProperty(_bindingInfo.CommandName);
            if (propertyInfo == null)
                throw new ArgumentException("commandName");

            Command = propertyInfo.GetValue(dataContext, null) as ICommand;
        }

        private void HookEvent()
        {
            var eventInfo = TargetObject.GetType().GetEvent(
                                                            _bindingInfo.EventName, BindingFlags.Public | BindingFlags.Instance);
            if (eventInfo == null)
                throw new ArgumentException("eventName");

            eventInfo.RemoveEventHandler(TargetObject, GetEventMethod(eventInfo));
            eventInfo.AddEventHandler(TargetObject, GetEventMethod(eventInfo));
        }

        private Delegate _method;

        private Delegate GetEventMethod(EventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");
            if (eventInfo.EventHandlerType == null)
                throw new ArgumentException("EventHandlerType is null");

            if (_method == null)
            {
                _method = Delegate.CreateDelegate(
                                                  eventInfo.EventHandlerType, this,
                        GetType().GetMethod("OnEventRaised",
                                BindingFlags.NonPublic | BindingFlags.Instance));
            }

            return _method;
        }

        private void OnEventRaised(object sender, EventArgs e)
        {
            ExecuteCommand();
        }
    }

    public class EventBehaviorCollection : List<EventBehavior>
    {
    }*/
}
