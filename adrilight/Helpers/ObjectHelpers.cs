using Newtonsoft.Json;

namespace adrilight.Helpers
{
    public class ObjectHelpers
    {
        /// <summary>
        /// Clones Any Object.
        /// </summary>
        /// <param name="objectToClone">The object to clone.</param>
        /// <return>The Clone</returns>
        public static T Clone<T>(T objectToClone)
        {
            T cloned_obj = default(T);

            var objectJson = JsonConvert.SerializeObject(objectToClone);

            cloned_obj = JsonConvert.DeserializeObject<T>(objectJson);


            return cloned_obj;
        }

    }
}
