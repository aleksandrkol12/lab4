using System;
using System.Text.Json.Serialization;

namespace lab4
{
    public class Student
    {
        public int? Id { get; set; } = null;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = null;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = null;

        [JsonPropertyName("group")]
        public string Group { get; set; } = null;

        public DateTime? CreatedAt { get; set; } = null;

        public DateTime? UpdatedAt { get; set; } = null;
    }
}