using GalaSoft.MvvmLight;

namespace adrilight.Settings.Automation
{
    public class SystemEventTriggerCondition : ViewModelBase, ITriggerCondition
    {
        public SystemEventTriggerCondition()
        {

        }
        public SystemEventTriggerCondition(string name, string description, SystemEventEnum eventname)
        {
            Name = name;
            Description = description;
            Event = eventname;

        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        private SystemEventEnum _event;// this is the condition such as key stroke code 
        public SystemEventEnum Event { get => _event; set { Set(() => Event, ref _event, value); } }
    }
}