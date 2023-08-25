namespace adrilight_shared.Enum
{
    public enum DeserializeMethodEnum
    {
        /// <summary>
        /// this only required a single json file for entire collection such as colors and gradients
        /// </summary>
        /// 
        SingleJson,
        /// <summary>
        /// this required config and thumbnail file, for example LEDSetup
        /// </summary>
        /// 
        FolderStructure,
        /// <summary>
        /// this require a root folder and that folder contains many json
        /// </summary>
        /// 
        MultiJson,
        /// <summary>
        /// this require a root folder and that folder contains many files
        /// </summary>
        /// 
        Files
    }
}