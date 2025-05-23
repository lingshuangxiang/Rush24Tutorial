using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace TwentyFour.Scripts.Features.Save
{
    public class SerializeList
    {
        public static byte[] Serialize<T>(List<T> list)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, list);
                return stream.ToArray();
            }
        }
        
        public static List<T> Deserialize<T>(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (List<T>)formatter.Deserialize(stream);
            }
        }
    }
}