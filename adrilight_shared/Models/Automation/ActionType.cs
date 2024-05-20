using System.Collections.ObjectModel;

namespace adrilight_shared.Models.Automation
{
    public class ActionType
    {
        public string LinkText { get; set; }
        public string ToResultText { get; set; }
        public string Name { get; set; }
        public bool IsValueDisplayed { get; set; }
        public bool IsTargetDeviceDisplayed { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }

        public void Init()
        {

            string lnkText = string.Empty;
            string resultText = string.Empty;
            getTextForActionType(Type, out lnkText, out resultText);
            LinkText = lnkText;
            ToResultText = resultText;
        }
        private void getTextForActionType(out string linkTxt, out string resultTxt)
        {
            linkTxt = string.Empty;
            resultTxt = string.Empty;
            switch (Type)
            {
                case "Activate":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Activate_linktext;
                    resultTxt = "";
                    break;

                case "Increase":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Increase_linktext;
                    resultTxt = "";//could add more param
                    break;

                case "Decrease":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Increase_linktext;
                    resultTxt = "";
                    break;

                case "On":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Increase_linktext;
                    resultTxt = "";
                    break;

                case "Off":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Increase_linktext;
                    resultTxt = "";
                    break;
                case "On/Off":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Increase_linktext;
                    resultTxt = "";
                    break;
                case "Change":
                    linkTxt = adrilight_shared.Properties.Resources.ActionType_Increase_linktext;
                    resultTxt = adrilight_shared.Properties.Resources.ActionType_Change_resulttext;
                    break;
            }
        }
    }
}
