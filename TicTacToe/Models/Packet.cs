using System;
using Newtonsoft.Json;

namespace TicTacToe.Models
{
    public class Packet
    {
        [JsonProperty("command")]
        public String Command { get; set; }

        [JsonProperty("message")]
        public String Message { get; set; }

        public Packet(String command = "", String message = "")
        {
            Command = command;
            Message = message;
        }

        public override String ToString()
        {
            return "[Packet:\n" + $" Command='{Command}'\n" + $" Message='{Message}']";
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Packet FromJson(String jsonData)
        {
            return JsonConvert.DeserializeObject<Packet>(jsonData);
        }
    }
}
